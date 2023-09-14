using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;

namespace ArtnetForUnity
{
    public class ArtDmx
    {
        byte[] pkt_ID = new byte[8];        //Offset 0
        byte[] pkt_OpCodeLo = new byte[1];  //Offset 8
        byte[] pkt_OpCodeHi = new byte[1];  //Offset 9
        byte[] pkt_ProtVerHi = new byte[1]; //Offset 10
        byte[] pkt_ProtVerLo = new byte[1]; //Offset 11
        byte[] pkt_Sequence = new byte[1];  //Offset 12
        byte[] pkt_Physical = new byte[1];  //Offset 13
        byte[] pkt_SubUni = new byte[1];    //Offset 14
        byte[] pkt_Net = new byte[1];       //Offset 15
        byte[] pkt_LengthHi = new byte[1];  //Offset 16
        byte[] pkt_LengthLo = new byte[1];  //Offset 17
        byte[] pkt_Data = new byte[512];    //Offset 18 - Variable Length from 2 - 512

        byte[] pkt_fullReturn;

        public byte[] CreateArtDmxPacket(byte[] dmxData, int SubUni, int physicalPortNumber = 0)
        {
            pkt_ID = new byte[] {0x41,0x72,0x74,0x2D,0x4E,0x65,0x74,0x00};//"Art-Net"; 
            
            pkt_OpCodeLo[0] = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(OpCodes.OpDmx)[0];
            pkt_OpCodeHi[0] = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(OpCodes.OpDmx)[1];
            pkt_ProtVerHi[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[1];
            pkt_ProtVerLo[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[0];
            pkt_Sequence = new byte[] { 0x00 }; //TODO: Implement Sequence
            pkt_Physical = new byte[] { (byte)physicalPortNumber };
            pkt_SubUni[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(SubUni)[0];
            pkt_Net[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(SubUni)[1]; //TODO: Possibly Wrong

            if(dmxData.Length % 2 == 1)
            {
                int length = dmxData.Length + 1;
                pkt_Data = new byte[length];
                pkt_LengthHi[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(length)[1];
                pkt_LengthLo[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(length)[0];
                Array.Copy(dmxData, 0, pkt_Data, 0, dmxData.Length);

            }
            else
            {
                int length = dmxData.Length;
                pkt_Data = new byte[length];
                pkt_LengthHi[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(length)[1];
                pkt_LengthLo[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(length)[0];
                Array.Copy(dmxData, 0, pkt_Data, 0, dmxData.Length);
            }

            CompilePacket();

            return pkt_fullReturn;

        }

        private void CompilePacket()
        {
            pkt_fullReturn = new byte[pkt_ID.Length + pkt_OpCodeLo.Length + pkt_OpCodeHi.Length + pkt_ProtVerHi.Length + pkt_ProtVerLo.Length + pkt_Sequence.Length + pkt_Physical.Length + pkt_SubUni.Length + pkt_Net.Length + pkt_LengthHi.Length + pkt_LengthLo.Length + pkt_Data.Length];
            Array.Copy(pkt_ID, 0, pkt_fullReturn, 0, pkt_ID.Length);
            Array.Copy(pkt_OpCodeLo, 0, pkt_fullReturn, 8, pkt_OpCodeLo.Length);
            Array.Copy(pkt_OpCodeHi, 0, pkt_fullReturn, 9, pkt_OpCodeHi.Length);
            Array.Copy(pkt_ProtVerHi, 0, pkt_fullReturn, 10, pkt_ProtVerHi.Length);
            Array.Copy(pkt_ProtVerLo, 0, pkt_fullReturn, 11, pkt_ProtVerLo.Length);
            Array.Copy(pkt_Sequence, 0, pkt_fullReturn, 12, pkt_Sequence.Length);
            Array.Copy(pkt_Physical, 0, pkt_fullReturn, 13, pkt_Physical.Length);
            Array.Copy(pkt_SubUni, 0, pkt_fullReturn, 14, pkt_SubUni.Length);
            Array.Copy(pkt_Net, 0, pkt_fullReturn, 15, pkt_Net.Length);
            Array.Copy(pkt_LengthHi, 0, pkt_fullReturn, 16, pkt_LengthHi.Length);
            Array.Copy(pkt_LengthLo, 0, pkt_fullReturn, 17, pkt_LengthLo.Length);
            Array.Copy(pkt_Data, 0, pkt_fullReturn, 18, pkt_Data.Length);
        }
    }

    public struct ArtDMXPacket
    {
        public byte[] data;
        public int Universe;
        public IPAddress[] iPAddress;
    }

   
}
