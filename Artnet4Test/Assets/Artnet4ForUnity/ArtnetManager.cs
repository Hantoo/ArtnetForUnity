using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        static bool shouldRun;
        private readonly BlockingCollection<IPPacket> SendQueue = new BlockingCollection<IPPacket>(new ConcurrentQueue<IPPacket>());
        private readonly BlockingCollection<IPPacket> ListenQueue = new BlockingCollection<IPPacket>(new ConcurrentQueue<IPPacket>());
        public delegate void ReceiveCallBack();
        public event ReceiveCallBack RecvCallBack;

        public static List<ArtnetDevice> deviceList = new List<ArtnetDevice>();

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

                ListenQueue.Add(pkt);
                RecvCallBack?.Invoke();
                //}

            }
            //Debug.Log("Finished Listener Thread");
        }

        public void Dispose()
        {
            // this will "complete" GetConsumingEnumerable, so your thread will complete
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
    }


}