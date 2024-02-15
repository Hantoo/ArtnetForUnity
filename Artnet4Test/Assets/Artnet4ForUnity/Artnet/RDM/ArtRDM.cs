using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace ArtnetForUnity.RDM
{

    public class ArtRDM : MonoBehaviour
    {

        byte[] pkt_ID = new byte[8];        //Offset 0
        byte[] pkt_OpCodeLo = new byte[1];  //Offset 8
        byte[] pkt_OpCodeHi = new byte[1];  //Offset 9
        byte[] pkt_ProtVerHi = new byte[1]; //Offset 10
        byte[] pkt_ProtVerLo = new byte[1]; //Offset 11
        byte[] pkt_RdmVer = new byte[1];  //Offset 12
        byte[] pkt_Filler2 = new byte[1];  //Offset 13 
        byte[] pkt_Spare = new byte[7];  //Offset 14 
        byte[] pkt_Net = new byte[1];  //Offset 21
        byte[] pkt_Command = new byte[1];  //Offset 22 
        byte[] pkt_Address = new byte[1];  //Offset 23 
        byte[] pkt_RdmPacket = new byte[1];  //Offset 24 


        byte[] pkt_fullReturn = new byte[25];

        public byte[] CreateArtRdmPacket(byte[] NetAddress)
        {
            
            pkt_ID = new byte[] { 0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00 };//"Art-Net"; 

            pkt_OpCodeLo[0] = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(OpCodes.OpRdm)[0];
            pkt_OpCodeHi[0] = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(OpCodes.OpRdm)[1];
            pkt_ProtVerHi[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[1];
            pkt_ProtVerLo[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[0];
            pkt_RdmVer[0] = (byte)RDMVersion.RdmStandard1_0;
            pkt_Filler2[0] = (byte)0;
            //pkt_Spare - Leave as 0
            pkt_Net[0] = NetAddress[0];
            pkt_Command[0] = (byte)RDMCommand.ArProcess;
            pkt_Address[0] = NetAddress[1];

            CompilePacket();
            return pkt_fullReturn;
        }

        private void CompilePacket()
        {
            Array.Copy(pkt_ID, 0, pkt_fullReturn, 0, pkt_ID.Length);
            Array.Copy(pkt_OpCodeLo, 0, pkt_fullReturn, 8, pkt_OpCodeLo.Length);
            Array.Copy(pkt_OpCodeHi, 0, pkt_fullReturn, 9, pkt_OpCodeHi.Length);
            Array.Copy(pkt_ProtVerHi, 0, pkt_fullReturn, 10, pkt_ProtVerHi.Length);
            Array.Copy(pkt_ProtVerLo, 0, pkt_fullReturn, 11, pkt_ProtVerLo.Length);
            Array.Copy(pkt_RdmVer, 0, pkt_fullReturn, 12, pkt_RdmVer.Length);
            Array.Copy(pkt_Filler2, 0, pkt_fullReturn, 13, pkt_Filler2.Length);
            Array.Copy(pkt_Spare, 0, pkt_fullReturn, 14, pkt_Spare.Length);
            Array.Copy(pkt_Net, 0, pkt_fullReturn, 21, pkt_Net.Length);
            Array.Copy(pkt_Command, 0, pkt_fullReturn, 22, pkt_Command.Length);
            Array.Copy(pkt_Address, 0, pkt_fullReturn, 23, pkt_Address.Length);
            Array.Copy(pkt_RdmPacket, 0, pkt_fullReturn, 24, pkt_RdmPacket.Length);
           

        }

    }

    public enum RDMVersion
    {
        RdmDraft1_0 = 0,
        RdmStandard1_0 = 1,
    }
     public enum RDMCommand
    {
        ArProcess = 0,
   
    }


}
