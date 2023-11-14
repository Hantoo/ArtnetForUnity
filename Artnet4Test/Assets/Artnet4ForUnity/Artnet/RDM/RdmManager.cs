using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArtnetForUnity.RDM;

namespace ArtnetForUnity.RDM
{
    public class RdmManager : MonoBehaviour
    {
        private ArtnetManager ArtManager;
        public void init(ArtnetManager manager)
        {
            ArtManager = manager;
            refreshRDM += refreshRDMCommand;
        }

        public void Dispose()
        {
            refreshRDM -= refreshRDMCommand;
        }

        public void Update()
        {
        }

        public void refreshRDMCommand()
        {
            IPPacket packet = new IPPacket();
            packet.ipAddress = ArtUtils.broadcastAddress;
            packet.opCode = OpCodes.OpRdm;
            packet.pktData = ParseRDMPacket();
            ArtManager.AddSenderPkt(packet);
        }

        public byte[] ParseRDMPacket()
        {
            ArtRDM artRDM = new ArtRDM();
            byte[] data= artRDM.CreateArtRdmPacket(ArtUtils.GetPortAddress_HiLo(0, 0, 1));
            return data;
        }

        public delegate void RefreshRDM();
        public static RefreshRDM refreshRDM;

    }
}
