using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

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

        private void init()
        {
            ArtnetForUnity.ArtUtils.LoadSettings();
            //Debug.Log("Using: " + ArtnetForUnity.ArtUtils.InterfaceIPAddress.ToString());

            udpClient = new UdpClient();
            udpClient.ExclusiveAddressUse = false;
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            //udpClient.Client.Bind(endPointSend);

            udpRevcClient = new UdpClient();
            udpRevcClient.ExclusiveAddressUse = false;
            udpRevcClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpRevcClient.Client.Bind(endPointRecv);

            RecvCallBack += Recv_Callback;
            

        }

        public byte[] _data_PollRequest;
        public byte[] _data_PollRequestReply;

        private static ArtnetForUnity.ArtDMXPacket[] ArtnetUniverseValues;

        public void Start(int universesArrayAmount)
        {
            isArtnetActive = true;
            artnet = new ArtnetForUnity.ArtDmx();
            artPoll = new ArtnetForUnity.ArtPoll();
            artPollreply = new ArtnetForUnity.ArtPollReply();
            artPoll._PriorityCodes = ArtnetForUnity.PriorityCodes.DpMed;
            _data_PollRequest = artPoll.CreateArtPollPacket();
            _data_PollRequestReply = artPollreply.CreateArtPollPacket();

            ArtnetUniverseValues = new ArtnetForUnity.ArtDMXPacket[universesArrayAmount];
            new Thread(ArtnetThreadLoop) { IsBackground = true }.Start();
        }

        public void SetArtnetData(int universesArrayIndex, byte[] UniverseData, int Universe, IPAddress[] address = null)
        {
            if(address == null) { address = new IPAddress[] { ArtUtils.broadcastAddress }; }
            ArtnetForUnity.ArtDMXPacket pkt = new ArtnetForUnity.ArtDMXPacket();
            pkt.iPAddress = address;
            pkt.data = UniverseData;
            pkt.Universe = Universe;
            ArtnetUniverseValues[universesArrayIndex] = pkt;


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
            UnityEngine.Debug.Log("Main Loop Thread Started");
            while (isArtnetActive)
            {
              
                if (ArtnetUniverseValues == null)
                {
                   
                    return;
                }
            
                //Send DMX
                if (ArtnetUniverseValues != null)
                    try
                    {
                      
                        for (int i = 0; i < ArtnetUniverseValues.Length; i++)
                        {
                           
                            if (ArtnetUniverseValues[i].data == null || ArtnetUniverseValues[i].data.Length == 0) continue;
                            //pkt = new ArtnetForUnity.IPPacket();
                           
                            pkt.opCode = ArtnetForUnity.OpCodes.OpDmx;
                            pkt.pktData = dmxPkt.CreateArtDmxPacket(ArtnetUniverseValues[i].data, ArtnetUniverseValues[i].Universe);
                            for (int _ipIndex = 0; _ipIndex < ArtnetUniverseValues[i].iPAddress.Length; _ipIndex++)
                            {
                                ;
                                pkt.ipAddress = ArtnetUniverseValues[i].iPAddress[_ipIndex];
                                AddSenderPkt(pkt);
                              
                            }
                          


                        }
                    }catch(Exception e)
                    {
                        UnityEngine.Debug.LogError(e);
                    }
             
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
            UnityEngine.Debug.Log("[Art-Net] Main Loop Thread Finish");
            ArtnetPollTimer.Stop();
        }

        public void Stop()
        {
            isArtnetActive = false;
        }

        private void SendArtPoll()
        {
          
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
                CheckArtPortWithDeviceList(device);
            }
        
        }

        public void AddSenderPkt(IPPacket pkt)
        {
            if(pkt.opCode == OpCodes.OpPoll) { RefreshDeviceList(); }
            if (SendQueue != null)
                SendQueue.Add(pkt);
            //Debug.Log("deviceList Count:" + deviceList.Count);
        }

        private void SenderThread()
        {
            //Debug.Log("Started Sender Thread");
            while (!SendQueue.IsCompleted)
            {
                if (SendQueue.Count > 0)
                {

                    IPPacket pkt = SendQueue.Take();
                    //Debug.Log("Packet:" + pkt.opCode + " | " + pkt.ipAddress.ToString());
                    endPointSend = new IPEndPoint(pkt.ipAddress, ArtnetForUnity.ArtUtils.ArtnetPort);
                    //udpClient.Client.Bind(endPointSend);
                    udpClient.Send(pkt.pktData, pkt.pktData.Length, endPointSend);
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
                pkt.opCode = ArtUtils.ByteToOpCode(pkt.pktData[8], pkt.pktData[9]);
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
                if (device.ipAddress.ToString() == deviceList[i].ipAddress.ToString())
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

        public SubscriberTable subscriberTable;
    }

    public struct SubscriberTable
    {
        public SubscriberIndex NodeIndex;
    }
    
    public struct SubscriberIndex
    {
        public int PortIndex;
        public IPAddress IPAddress;
        public byte NetSwitch;
        public byte SubSwitch;
        public byte SwInOut;
    }


}
