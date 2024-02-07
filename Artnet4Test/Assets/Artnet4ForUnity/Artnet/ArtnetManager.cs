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

        
        //// Start is called before the first frame update
        //void Start()
        //{

        //}
        public ArtnetManager()
        {
            // start consumer thread here
            init();
            new Thread(SenderThread) { IsBackground = true }.Start();
            new Thread(ListenerThread) { IsBackground = true }.Start();
          
        }

        static UdpClient udpClient;
        static UdpClient udpRevcClient;

        static IPEndPoint endPointSend = new IPEndPoint(IPAddress.Any, ArtUtils.ArtnetPort);
        static IPEndPoint endPointRecv = new IPEndPoint(IPAddress.Any, ArtUtils.ArtnetPort);
        static bool isArtnetActive;
        private readonly BlockingCollection<IPPacket> SendQueue = new BlockingCollection<IPPacket>(new ConcurrentQueue<IPPacket>());
        private readonly BlockingCollection<IPPacket> ListenQueue = new BlockingCollection<IPPacket>(new ConcurrentQueue<IPPacket>());
        public delegate void ReceiveCallBack();
        public event ReceiveCallBack RecvCallBack;

        public static List<ArtnetDevice> deviceList = new List<ArtnetDevice>();
        ArtnetForUnity.ArtDmx artnet;
        ArtnetForUnity.ArtPoll artPoll;
        ArtnetForUnity.ArtPollReply artPollreply;

        ArtnetSettings settings;
        ArtnetForUnity.IPPacket pkt_ArtSync = new IPPacket();
        public TimecodeManager timecodeManager;
        public RdmManager rdmManager;

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

            udpClient = new UdpClient();
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(endPointSend);

            udpRevcClient = new UdpClient();
            udpRevcClient.ExclusiveAddressUse = false;
            udpRevcClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //udpRevcClient.Client.Bind(endPointRecv);
            udpRevcClient.Client.Bind(endPointSend);

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

            settings.artnetOutputs[UnityUniverseNumber].DMXData = UniverseData;
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

            if (pkt.opCode == OpCodes.OpTimeCode)
            {
                ArtnetDevice device;
                getDeviceFromIP(pkt.ipAddress, out device);
                //UnityEngine.Debug.Log("pkt.ipAddress:" + pkt.ipAddress.ToString());
                //UnityEngine.Debug.Log("device.ipAddress:" + device.ipAddress.ToString());
                //UnityEngine.Debug.Log(", ArtUtils.InterfaceIPAddress:" + ArtUtils.InterfaceIPAddress.ToString());
                if (pkt.ipAddress.ToString() == ArtUtils.InterfaceIPAddress.ToString()) return;
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
                if (SendQueue.Count > 0)
                {

                    IPPacket pkt = SendQueue.Take();
                    //UnityEngine.Debug.Log("Packet:" + pkt.ipAddress + " | " + pkt.ipAddress.ToString());
                    endPointSend = new IPEndPoint(pkt.ipAddress, ArtnetForUnity.ArtUtils.ArtnetPort);
                    //udpClient.Client.Bind(endPointSend);
                    udpClient.Send(pkt.pktData, pkt.pktData.Length, endPointSend);

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
                //if (udpRevcClient.Available > 0)
                //{


                IPPacket pkt = new IPPacket();
                pkt.pktData = udpRevcClient.Receive(ref endPointRecv);
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

        public void Dispose()
        {
            // this will "complete" GetConsumingEnumerable, so your thread will complete
            isArtnetActive = false;
            SendQueue.CompleteAdding();
            SendQueue.Dispose();
            ListenQueue.CompleteAdding();
            ListenQueue.Dispose();
            timecodeManager.Dispose();
            rdmManager.Dispose();
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
            UnityEngine.Debug.Log("[Artnet4Unity] Artnet Manager Started In Editor");
            artnetManager = new ArtnetForUnity.ArtnetManager();
            artnetManager.Start();
            EditorApplication.quitting += Quit;
        }

        public static void Quit()
        {
         
            artnetManager.Stop();
            artnetManager.Dispose();
            UnityEngine.Debug.Log("[Artnet4Unity] Artnet Manager Stopped In Editor");

        }

        ~ArtnetManagerEditor()
        {
            artnetManager.Stop();
            artnetManager.Dispose();
            UnityEngine.Debug.Log("[Artnet4Unity] Artnet Manager Stopped In Editor");
        }

    }
    #endif

}
