using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;

public class Tester : MonoBehaviour
{
    UdpClient client;
    IPEndPoint IPEnd;
    IPEndPoint IPEndBroadcastPoll;
    public byte[] _data;
    public byte[] _data_PollRequest;
    public byte[] _data_PollRequestReply;

    public float ArtPollTimer = 0;

    ArtnetForUnity.ArtnetManager artnetManager;
    public void Start()
    {
        //client = new UdpClient();
        artnetManager = new ArtnetForUnity.ArtnetManager();
        ArtnetForUnity.ArtDmx artnet = new ArtnetForUnity.ArtDmx();
        ArtnetForUnity.ArtPoll artPoll = new ArtnetForUnity.ArtPoll();
        ArtnetForUnity.ArtPollReply artPollreply = new ArtnetForUnity.ArtPollReply();
        byte[] data = new byte[] { 0xff, 0xff, 0xff, 0xff };
        _data = artnet.CreateArtDmxPacket(data, 0, 0);
        artPoll._PriorityCodes = ArtnetForUnity.PriorityCodes.DpCritical;
        _data_PollRequest = artPoll.CreateArtPollPacket();
        _data_PollRequestReply = artPollreply.CreateArtPollPacket();
        //IPEnd = new IPEndPoint(new IPAddress(new byte[] { 127, 0, 0, 1 }), 0x1936);
        //IPEndBroadcastPoll = new IPEndPoint(new IPAddress(new byte[] { 2, 255, 255, 255 }), 0x1936);
        //client.Send(_data, _data.Length, IPEnd);
        //CheckArtPoll();
    }

    private void OnDestroy()
    {
        artnetManager.Dispose();
    }


    public void Update()
    {
        CheckArtPoll();
        //client.Send(_data, _data.Length, IPEnd);
    }

    private void CheckArtPoll()
    {
        ArtPollTimer -= Time.deltaTime;
        if (ArtPollTimer > 0) return;
        ArtPollTimer = 2.8f;
        //client.Send(_data_PollRequest, _data_PollRequest.Length, IPEndBroadcastPoll);
        ArtnetForUnity.IPPacket pkt = new ArtnetForUnity.IPPacket();
        pkt.ipAddress = ArtnetForUnity.ArtUtils.broadcastAddress;
        pkt.pktData = _data_PollRequest;
        pkt.opCode = ArtnetForUnity.OpCodes.OpPoll;
        artnetManager.AddSenderPkt(pkt);
        ArtnetForUnity.IPPacket pkt_reply = new ArtnetForUnity.IPPacket();
        pkt_reply.ipAddress = ArtnetForUnity.ArtUtils.InterfaceIPAddress;
        pkt_reply.pktData = _data_PollRequestReply;
        pkt_reply.opCode = ArtnetForUnity.OpCodes.OpPollReply;
        artnetManager.AddSenderPkt(pkt_reply);
    }
}
