using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using UnityEditor;
using UnityEngine;
using ArtnetForUnity.Timecode;
using ArtnetForUnity.RDM;
using System.Linq;

namespace ArtnetForUnity
{

    public class ArtnetManager : IDisposable
    {
        //If something can be static, and can be disposed of, does that mean its in a qauntum state?
        
      
        /// <summary>
        /// The most recently created ArtnetManager. Used by ArtnetDMXInput components to find the running manager.
        /// </summary>
        public static ArtnetManager Instance { get; private set; }

        public ArtnetManager()
        {
            Instance = this;
            // start consumer thread here
            init();
            new Thread(SenderThread) { IsBackground = true }.Start();
            new Thread(ListenerThread) { IsBackground = true }.Start();
          
        }

        //One socket per manager, used for both sending and receiving. With multiple sockets bound to the
        //Art-Net port, unicast packets (e.g. loopback 127.0.0.1) are delivered to only ONE of them - which
        //may be a socket nobody reads. Broadcast reaches every bound socket, which hid this problem.
        UdpClient udpClient;

        IPEndPoint endPointSend = new IPEndPoint(IPAddress.Any, ArtUtils.ArtnetPort);
        IPEndPoint endPointRecv = new IPEndPoint(IPAddress.Any, ArtUtils.ArtnetPort);
        bool isArtnetActive;
        private readonly BlockingCollection<IPPacket> SendQueue = new BlockingCollection<IPPacket>(new ConcurrentQueue<IPPacket>());
        private readonly BlockingCollection<IPPacket> ListenQueue = new BlockingCollection<IPPacket>(new ConcurrentQueue<IPPacket>());
        public delegate void ReceiveCallBack();
        public event ReceiveCallBack RecvCallBack;

        public static List<ArtnetDevice> deviceList = new List<ArtnetDevice>();
        ArtnetForUnity.ArtDmx artnet;
        ArtnetForUnity.ArtPoll artPoll;
        ArtnetForUnity.ArtPollReply artPollreply;

        public ArtnetSettings settings;
        ArtnetForUnity.IPPacket pkt_ArtSync = new IPPacket();
        public TimecodeManager timecodeManager;
        public RdmManager rdmManager;
        public ArtnetInputManager inputManager;

        public float frameRateOfSender;
        private float[] frameRateOfSenderCompile = new float[10];
        private int frameRateOfSenderCompileIncrement = 0;
        public float AvgframeRateOfSenderCompile;
        private Stopwatch frameRateStopWatch;
        private float[] frameRateOfPacketQueueCompile = new float[10];
        private int frameRateOfPacketQueueCompileIncrement = 0;
        public float AvgframeRateOfPacketQueueCompile;
        private Stopwatch frameRateOfPacketQueueCompileStopWatch;

        bool Diag_Verbose = false;

        private void init()
        {
            settings = ArtnetForUnity.ArtUtils.LoadSettings();
            NetworkInterface networkInterface;
            if(ArtnetForUnity.ArtUtils.GetInterface(ArtnetForUnity.ArtUtils.InterfaceIPAddress, out networkInterface))
            {
                ArtUtils.SelectedInterface = networkInterface;
            }
            
            //Add Ons
            timecodeManager = new TimecodeManager();
            timecodeManager.init(this);

            rdmManager = new RdmManager();
            rdmManager.init(this);

            inputManager = new ArtnetInputManager();
            inputManager.init(this);

            udpClient = new UdpClient();
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            if (IPAddress.IsLoopback(ArtUtils.InterfaceIPAddress))
            {
                //Bind specifically to 127.0.0.1 rather than 0.0.0.0. Other Art-Net applications on this
                //machine (consoles, DMXWorkshop etc) hold wildcard bindings on the port, and Windows
                //delivers a unicast packet to the most specific matching socket - a specific binding
                //guarantees loopback packets reach Unity instead of being swallowed by another app's socket.
                udpClient.Client.Bind(new IPEndPoint(ArtUtils.InterfaceIPAddress, ArtUtils.ArtnetPort));
            }
            else
            {
                udpClient.Client.Bind(endPointSend);
            }
            udpClient.EnableBroadcast = true;
            //Stop Windows raising ConnectionReset on Receive when a previous send hit a closed port (common on loopback)
            try
            {
                const int SIO_UDP_CONNRESET = -1744830452;
                udpClient.Client.IOControl((IOControlCode)SIO_UDP_CONNRESET, new byte[] { 0 }, null);
            }
            catch { /*Not supported outside Windows*/ }

            RecvCallBack += Recv_Callback;

          
            ArtSync artSync = new ArtSync();
            pkt_ArtSync.pktData = artSync.CombinePacket();
            pkt_ArtSync.opCode = OpCodes.OpSync;
            pkt_ArtSync.ipAddress = ArtUtils.broadcastAddress;

            frameRateOfPacketQueueCompileStopWatch = new Stopwatch();
            frameRateStopWatch = new Stopwatch();
        }

        public byte[] _data_PollRequest;
        public byte[] _data_PollRequestReply;

        private static ArtnetForUnity.ArtDMXPacket[] ArtnetUniverseValues;
        private bool gameRunning = false;
        public void Start()
        {
            //Creates byte array ready for data
            for (int i = 0; i < settings.artnetOutputs.Count; i++)
            {
                settings.artnetOutputs[i].DMXData = new byte[512];
            }
            //Enable any DMX inputs defined in settings
            if (settings.artnetInputs != null)
            {
                for (int i = 0; i < settings.artnetInputs.Count; i++)
                {
                    inputManager.EnableUniverse(settings.artnetInputs[i].Universe);
                }
            }
            isArtnetActive = true;
            artnet = new ArtnetForUnity.ArtDmx();
            artPoll = new ArtnetForUnity.ArtPoll();
            artPollreply = new ArtnetForUnity.ArtPollReply();
            artPoll._PriorityCodes = ArtnetForUnity.PriorityCodes.DpMed;
            _data_PollRequest = artPoll.CreateArtPollPacket();
            _data_PollRequestReply = artPollreply.CreateArtPollPacket();
            //Create array or artDMX packets ready to be sent 
            ArtnetUniverseValues = new ArtnetForUnity.ArtDMXPacket[settings.artnetOutputs.Count];
            new Thread(ArtnetThreadLoop) { IsBackground = true }.Start();
            gameRunning = Application.isPlaying;
        }

        public void SetArtnetData(int UnityUniverseNumber, byte[] UniverseData)
        {
            try
            {
                if (settings.artnetOutputs[UnityUniverseNumber] == null) { UnityEngine.Debug.LogError("[Artnet4Unity] DMX Universe Does Not Exist. Have you added a universe via general settings?"); }
                settings.artnetOutputs[UnityUniverseNumber].DMXData = UniverseData;
            }catch(Exception e) { UnityEngine.Debug.LogError("[Artnet4Unity] DMX Universe Does Not Exist. Have you added a universe via general settings? | "+ e.Message); }
        }

        byte[] EmptyArray = new byte[512];
        public byte[] GetArtnetData(int UnityDMXNumber)
        {
            try {
                return settings.artnetOutputs[UnityDMXNumber].DMXData;
            } catch (Exception e) { return EmptyArray; }
        }

        private void ArtnetThreadLoop()
        {
            ArtDmx dmxPkt = new ArtDmx();
            ArtnetForUnity.IPPacket pkt = new IPPacket();
            Stopwatch timer= new Stopwatch();
            Stopwatch ArtnetPollTimer= new Stopwatch();
            int millisecondsToSleep = 0;
            ArtnetPollTimer.Start();
            timer.Start();
            UnityEngine.Debug.Log("[Artnet4Unity] Main Loop Thread Started");

            

            while (isArtnetActive)
            { 


                if (ArtnetUniverseValues == null)
                {

                    return;
                }
                //Timecode
                
                timecodeManager.Update();


#if UNITY_STANDALONE
                //Send DMX
                if (gameRunning) 
                if (settings != null && settings.artnetOutputs != null)
                if (settings.artnetOutputs.Count > 0)
                    try
                    {
                        pkt.opCode = ArtnetForUnity.OpCodes.OpDmx;
                        for (int i = 0; i < settings.artnetOutputs.Count; i++)
                        {
                            if (settings.artnetOutputs[i].DMXData == null) continue;
                            if (settings.artnetOutputs[i].DMXData == null || settings.artnetOutputs[i].DMXData.Length == 0) continue;

                            pkt.pktData = dmxPkt.CreateArtDmxPacket(settings.artnetOutputs[i].DMXData, settings.artnetOutputs[i].Universe);
                            for (int nodeIPIndex = 0; nodeIPIndex < settings.artnetOutputs[i].NodeRevcIPAddress.Count; nodeIPIndex++)
                            {
                                //TODO: Change this from being parsed every cycle to having IPAddresses defined when saved / on start up
                                pkt.ipAddress = IPAddress.Parse(settings.artnetOutputs[i].NodeRevcIPAddress[nodeIPIndex]);
                                AddSenderPkt(pkt);
                            }
                        }

                        //Calculate time it's taken to process all the dmx packets and add to sender queue;
                        frameRateOfPacketQueueCompileStopWatch.Stop();
                        frameRateOfPacketQueueCompile[frameRateOfPacketQueueCompileIncrement] = frameRateOfPacketQueueCompileStopWatch.ElapsedMilliseconds;
                        frameRateOfPacketQueueCompileIncrement = (frameRateOfPacketQueueCompileIncrement + 1) % frameRateOfPacketQueueCompile.Length;
                        AvgframeRateOfPacketQueueCompile = frameRateOfPacketQueueCompile.Average();
                        ArtUtils.Diagnostic_DMXPacketQueueFrameRate = AvgframeRateOfPacketQueueCompile;
                        frameRateOfPacketQueueCompileStopWatch.Restart();


                        if (settings.useArtSync)
                        {
                            AddSenderPkt(pkt_ArtSync);
                        }
                    }
                    catch(Exception e)

                    {
                        UnityEngine.Debug.LogError(e);
                    }
            
                #endif

            //ArtPoll
            if (ArtnetPollTimer.Elapsed.TotalMilliseconds > 2800)
                { //2.8Seconds
                    //Poll
                    pkt.ipAddress = ArtnetForUnity.ArtUtils.broadcastAddress;
                    pkt.pktData = _data_PollRequest;
                    pkt.opCode = ArtnetForUnity.OpCodes.OpPoll;
                    AddSenderPkt(pkt);
                    //Internal Reply
                    pkt.ipAddress = ArtnetForUnity.ArtUtils.InterfaceIPAddress;
                    _data_PollRequestReply = artPollreply.CreateArtPollPacket();
                    pkt.pktData = _data_PollRequestReply;
                    pkt.opCode = ArtnetForUnity.OpCodes.OpPollReply;
                    AddSenderPkt(pkt);
                    ArtnetPollTimer.Restart();

                }
                //Calculate loop time
                timer.Stop();
                if (timer.ElapsedMilliseconds > 22){ millisecondsToSleep = 0; } else { millisecondsToSleep = (22 - (int)timer.ElapsedMilliseconds); }
           
                Thread.Sleep(millisecondsToSleep);
               
                timer.Restart();
            }
            UnityEngine.Debug.Log("[Artnet4Unity] Main Loop Thread Finish");
            ArtnetPollTimer.Stop();
        }

        public void Stop()
        {
            isArtnetActive = false;
        }

        private void Recv_Callback()
        {
            IPPacket pkt = ListenQueue.Take();
          
            if(pkt.opCode == OpCodes.OpPollReply)
            {
         
                ArtnetDevice device = new ArtnetDevice();
                device.connected = true;
                device.ipAddress = pkt.ipAddress;
                device.name = ArtPollReply.GetName(pkt.pktData);
                device.bindIndex = ArtPollReply.GetBindIndex(pkt.pktData);
                CheckArtPortWithDeviceList(device);
            }

            if (pkt.opCode == OpCodes.OpDmx)
            {
                inputManager.ProcessDmxPacket(pkt);
            }

            if (pkt.opCode == OpCodes.OpTimeCode)
            {
                ArtnetDevice device;
                getDeviceFromIP(pkt.ipAddress, out device);
                //UnityEngine.Debug.Log("pkt.ipAddress:" + pkt.ipAddress.ToString());
                //UnityEngine.Debug.Log("device.ipAddress:" + device.ipAddress.ToString());
                //UnityEngine.Debug.Log(", ArtUtils.InterfaceIPAddress:" + ArtUtils.InterfaceIPAddress.ToString());
                //Ignore our own timecode packets - unless on a loopback interface, where all local apps share the same IP
                if (!IPAddress.IsLoopback(ArtUtils.InterfaceIPAddress) && pkt.ipAddress.ToString() == ArtUtils.InterfaceIPAddress.ToString()) return;
                timecodeManager.UpdateCurrentTimecodeFromPacket(timecodeManager.ParseTimecodePacket(pkt, device));
            }
        }

        public void AddSenderPkt(IPPacket pkt)
        {
            if(pkt.opCode == OpCodes.OpPoll) { RefreshDeviceList(); }
            if (SendQueue != null)
                SendQueue.Add(pkt);
        }

        private void SenderThread()
        {
            //Debug.Log("Started Sender Thread");
            while (!SendQueue.IsCompleted)
            {
                {

                    IPPacket pkt;
                    //Blocks until a packet is queued; throws when the queue is completed or disposed on Dispose
                    //(InvalidOperationException also covers ObjectDisposedException)
                    try { pkt = SendQueue.Take(); }
                    catch (InvalidOperationException) { break; }
                    //UnityEngine.Debug.Log("Packet:" + pkt.ipAddress + " | " + pkt.ipAddress.ToString());
                    endPointSend = new IPEndPoint(pkt.ipAddress, ArtnetForUnity.ArtUtils.ArtnetPort);
                    //udpClient.Client.Bind(endPointSend);
                    try
                    {
                        udpClient.Send(pkt.pktData, pkt.pktData.Length, endPointSend);
                    }
                    catch (SocketException e)
                    {
                        //Don't let an unreachable address kill the sender thread - warn once per address
                        if (failedSendAddresses.Add(pkt.ipAddress.ToString()))
                            UnityEngine.Debug.LogError("[Artnet4Unity] Could not send to " + pkt.ipAddress + " (" + e.Message + "). Check the node IP addresses in Artnet General Settings match the selected interface.");
                        continue;
                    }

                    //Calculate time it's taken to process all the dmx packets and add to sender queue;
                    frameRateStopWatch.Stop();
                    frameRateOfSenderCompile[frameRateOfSenderCompileIncrement] = frameRateStopWatch.ElapsedMilliseconds;
                    frameRateOfSenderCompileIncrement = (frameRateOfSenderCompileIncrement + 1) % frameRateOfSenderCompile.Length;
                    AvgframeRateOfSenderCompile = frameRateOfSenderCompile.Average();
                    ArtUtils.Diagnostic_SenderFrameRate = AvgframeRateOfSenderCompile;
                    frameRateStopWatch.Restart();
                }
            }
            //Debug.Log("Finished Sender Thread");
        }

        private void ListenerThread()
        {
            //Debug.Log("Started Listener Thread");

            while (!ListenQueue.IsCompleted)
            {
                IPPacket pkt = new IPPacket();
                //Receive throws when the socket is closed on Dispose - use that to end the thread
                try
                {
                    pkt.pktData = udpClient.Receive(ref endPointRecv);
                }
                catch (SocketException) { if (isDisposed) break; continue; }
                catch (ObjectDisposedException) { break; }
                pkt.ipAddress = endPointRecv.Address;
                if(Diag_Verbose)UnityEngine.Debug.Log("Revc Packet From:" + pkt.ipAddress.ToString());
                if(pkt.pktData.Length < 9) UnityEngine.Debug.Log("Artnet Packet Recvievd Formatted Wrongly pkt.pktData Len:" + pkt.pktData.Length);
                pkt.opCode = ArtUtils.ByteToOpCode(pkt.pktData[8], pkt.pktData[9]);
                if(Diag_Verbose)UnityEngine.Debug.Log("Packet Type:" + pkt.opCode.ToString());
                if (ListenQueue != null)
                {
                    ListenQueue.Add(pkt);
                    RecvCallBack?.Invoke();
                }
                //}

            }
            //Debug.Log("Finished Listener Thread");
        }

        private volatile bool isDisposed;
        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            // this will "complete" GetConsumingEnumerable, so your thread will complete
            isArtnetActive = false;
            SendQueue.CompleteAdding();
            SendQueue.Dispose();
            ListenQueue.CompleteAdding();
            ListenQueue.Dispose();
            timecodeManager.Dispose();
            rdmManager.Dispose();
            inputManager.Dispose();
            //Release the Art-Net port and unblock the listener thread
            try { if (udpClient != null) udpClient.Close(); } catch { }
            if (Instance == this) Instance = null;
        }



        //// Update is called once per frame
        //void Update()
        //{

        //Check Queue For Sending Artnet
        //Check Queue For Receving Artnet / ArtPoll / ArtTrigger
        //Art Poll - Every 2.5-3 Seconds
        // }
        
        private void CheckArtPortWithDeviceList(ArtnetDevice device)
        {
            bool found = false;
            for (int i = 0; i < deviceList.Count; i++)
            {
                if ((device.ipAddress.ToString() == deviceList[i].ipAddress.ToString()) && device.bindIndex == deviceList[i].bindIndex)
                {
                    ArtnetDevice _d = deviceList[i];
                    _d.connected = true;
                    _d.offlineTicker = 0;
                    deviceList[i] = _d;
                    found = true;
                }
            }
            if (!found)
            {
                //Debug.Log("Adding Device:" + device.name);
                deviceList.Add(device);
            }
        }

        private bool getDeviceFromIP(IPAddress address, out ArtnetDevice device)
        {
            device = new ArtnetDevice();
            for (int i = 0; i < deviceList.Count; i++)
            {
                if(deviceList[i].ipAddress.ToString() == address.ToString())
                {
                    device = deviceList[i];
                    return true;
                }
            }
            return false;
        }

        private readonly HashSet<string> failedSendAddresses = new HashSet<string>();
        List<int> removeDevicesAtIndex = new List<int>();
        private void RefreshDeviceList()
        {
            removeDevicesAtIndex.Clear();
            for (int i = 0; i < deviceList.Count; i++)
            {
                if (deviceList[i].connected == false)
                {
                    ArtnetDevice _d = deviceList[i];
                    _d.offlineTicker++;
                    deviceList[i] = _d;
                }
                if (deviceList[i].offlineTicker > 3)
                {
                    removeDevicesAtIndex.Add(i);
                }
            }
            for (int i = removeDevicesAtIndex.Count - 1; i >= 0; i--)
            {
                deviceList.RemoveAt(removeDevicesAtIndex[i]);
            }
            for (int i = 0; i < deviceList.Count; i++)
            {
                ArtnetDevice _d = deviceList[i];
                _d.connected = false;
                deviceList[i] = _d;
            }
        }
    }

    public struct IPPacket
    {
        public IPAddress ipAddress;
        public byte[] pktData;
        public OpCodes opCode;
    }

    public struct ArtFrame
    {
        public int Universe;
        public byte[] data;
    }

    [Serializable]
    public struct ArtnetDevice
    {
        
        public string name;
        public string longName;
        public IPAddress ipAddress;
        public bool connected;
        public int offlineTicker;
        public int bindIndex;
        public SubscriberTable subscriberTable;
    }

    public struct SubscriberTable
    {
        public SubscriberIndex NodeIndex;
    }
    
    public struct SubscriberIndex
    {
        public int UID;
        public int PortIndex;
        public IPAddress IPAddress;
        public byte NetSwitch;
        public byte SubSwitch;
        public byte SwInOut;
    }



    #if UNITY_EDITOR
    [InitializeOnLoad]
    public class ArtnetManagerEditor
    {

        static ArtnetForUnity.ArtnetManager artnetManager;
        static ArtnetManagerEditor()
        {
            //Don't bind the Art-Net port while entering play mode - runtime managers own it during play
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                CreateManager();
            }
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += Quit;
        }

        private static void CreateManager()
        {
            if (artnetManager != null) return;
            UnityEngine.Debug.Log("[Artnet4Unity] Artnet Manager Started In Editor");
            artnetManager = new ArtnetForUnity.ArtnetManager();
            artnetManager.Start();
        }

        private static void DisposeManager()
        {
            if (artnetManager == null) return;
            artnetManager.Stop();
            artnetManager.Dispose();
            artnetManager = null;
            UnityEngine.Debug.Log("[Artnet4Unity] Artnet Manager Stopped In Editor");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            //Release the port before play starts so the game's own ArtnetManager is the only one bound to it,
            //then take it back when returning to edit mode (for the editor windows / node discovery)
            if (state == PlayModeStateChange.ExitingEditMode) DisposeManager();
            if (state == PlayModeStateChange.EnteredEditMode) CreateManager();
        }

        public static void Quit()
        {
            DisposeManager();
        }

    }
    #endif

}
