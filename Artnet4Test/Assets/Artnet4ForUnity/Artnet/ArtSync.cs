using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArtnetForUnity
{
    public class ArtSync
    {
        byte[] pkt_ID = new byte[8];
        byte[] pkt_OpCode = new byte[2];
        byte[] pkt_ProtVerHi = new byte[1];
        byte[] pkt_ProtVerLo = new byte[1];
        byte[] pkt_Aux1 = new byte[1];
        byte[] pkt_Aux2 = new byte[1];

        public byte[] pkt_fullReturn = new byte[15];

        public byte[] CombinePacket()
        {
            pkt_ID = new byte[] { 0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00 };//"Art-Net"; 
            pkt_OpCode = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(ArtnetForUnity.OpCodes.OpSync);
            pkt_ProtVerHi[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[1];
            pkt_ProtVerLo[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[0];
            pkt_Aux1[0] = 0;
            pkt_Aux2[0] = 0;

            Array.Copy(pkt_ID, 0, pkt_fullReturn, 0, pkt_ID.Length);
            Array.Copy(pkt_OpCode, 0, pkt_fullReturn, 8, pkt_OpCode.Length);
            Array.Copy(pkt_ProtVerHi, 0, pkt_fullReturn, 10, pkt_ProtVerHi.Length);
            Array.Copy(pkt_ProtVerLo, 0, pkt_fullReturn, 11, pkt_ProtVerLo.Length);
            Array.Copy(pkt_Aux1, 0, pkt_fullReturn, 12, pkt_Aux1.Length);
            Array.Copy(pkt_Aux2, 0, pkt_fullReturn, 13, pkt_Aux2.Length);

            return pkt_fullReturn;
        }




    }
}