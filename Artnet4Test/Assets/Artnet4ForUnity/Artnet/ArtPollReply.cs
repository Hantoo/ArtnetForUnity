using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArtnetForUnity
{
    public class ArtPollReply
    {
        byte[] pkt_ID = new byte[8];        //Offset 0
        byte[] pkt_OpCodeLo = new byte[1];  //Offset 8
        byte[] pkt_OpCodeHi = new byte[1];  //Offset 9
        byte[] pkt_IPAddress = new byte[4]; //Offset 10
        byte[] pkt_PortLo = new byte[1]; //Offset 14
        byte[] pkt_PortHi = new byte[1]; //Offset 15
        byte[] pkt_VersInfoH = new byte[1];  //Offset 16
        byte[] pkt_VersInfoL = new byte[1];  //Offset 17 
        byte[] pkt_NetSwitch = new byte[1];    //Offset 18 
        byte[] pkt_SubSwitch = new byte[1];       //Offset 19
        byte[] pkt_OemHi = new byte[1];  //Offset 20
        byte[] pkt_OemLo = new byte[1];  //Offset 21
        byte[] pkt_UbeaVersion = new byte[1];    //Offset 22 
        byte[] pkt_Status1 = new byte[1];    //Offset 23
        byte[] pkt_EstaManLo = new byte[1];    //Offset 24 
        byte[] pkt_EstaManhi = new byte[1];    //Offset 25 
        byte[] pkt_PortName = new byte[18];    //Offset 26 
        byte[] pkt_LongName = new byte[64];    //Offset 44 
        byte[] pkt_NodeReport = new byte[64];    //Offset 108
        byte[] pkt_NumPortsHi = new byte[1];    //Offset 172
        byte[] pkt_NumPortsLo = new byte[1];    //Offset 173 
        byte[] pkt_PortTypes = new byte[4];    //Offset 174 
        byte[] pkt_GoodInput = new byte[4];    //Offset 178 
        byte[] pkt_GoodOutputA = new byte[4];    //Offset 182 
        byte[] pkt_SwIn = new byte[4];    //Offset 186 
        byte[] pkt_SwOut = new byte[4];    //Offset 190 
        byte[] pkt_AcnPriority = new byte[1];    //Offset 194 
        byte[] pkt_SwMacro = new byte[1];    //Offset 195
        byte[] pkt_SwRemote = new byte[1];    //Offset 196 
        byte[] pkt_Spare1 = new byte[1];    //Offset 197
        byte[] pkt_Spare2 = new byte[1];    //Offset 198 
        byte[] pkt_spare3 = new byte[1];    //Offset 199 
        byte[] pkt_Style = new byte[1];    //Offset 200
        byte[] pkt_Mac = new byte[6];    //Offset 201 
        //byte[] pkt_Mac2 = new byte[1];    //Offset 202 
        //byte[] pkt_Mac3 = new byte[1];    //Offset 203 
        //byte[] pkt_Mac4 = new byte[1];    //Offset 204 
        //byte[] pkt_Mac5 = new byte[1];    //Offset 205 
        //byte[] pkt_MacLo = new byte[1];    //Offset 206 -- Anything After this is optional
        byte[] pkt_BindIP = new byte[4];    //Offset 207
        byte[] pkt_Status2 = new byte[1];    //Offset 211 
        byte[] pkt_GoodOutputB = new byte[4];    //Offset 212 
        byte[] pkt_Status3 = new byte[1];    //Offset 216
        byte[] pkt_DefaulRespUIDHi = new byte[1];    //Offset 217 
        byte[] pkt_DefaulRespUID1 = new byte[1];    //Offset 218 
        byte[] pkt_DefaulRespUID2 = new byte[1];    //Offset 219 
        byte[] pkt_DefaulRespUID3 = new byte[1];    //Offset 220 
        byte[] pkt_DefaulRespUID4 = new byte[1];    //Offset 221 
        byte[] pkt_DefaulRespUIDLo = new byte[1];    //Offset 222 
        byte[] pkt_UserHi = new byte[1];    //Offset 223 
        byte[] pkt_UserLo = new byte[1];    //Offset 224 
        byte[] pkt_RefreshRateHi = new byte[1];    //Offset 225 
        byte[] pkt_RefreshRateLo = new byte[1];    //Offset 226 
        byte[] pkt_Filler = new byte[8];    //Offset 227 

        byte[] pkt_fullReturn = new byte[207];
        private Status1_IndicatorState status1_IndicatorState;
        private Status1_PortAddressProgrammingAuthority status1_PortAddressProgrammingAuthority;
        private Status1_FirmwareBoot status1_FirmwareBoot;
        private Status1_RDM status1_RDM;
        private Status1_UBEA status1_UBEA;

        public PortTypes_Type portTypes_Type;
        public PortTypes_PortCanInputToArtnetNetwork portTypes_PortCanInputToArtnetNetwork;
        public PortTypes_PortCanOutputDataFromArtnetNetwork portTypes_PortCanOutputDataFromArtnetNetwork;
        public byte[] CreateArtPollPacket()
        {
            status1_IndicatorState = Status1_IndicatorState.Unknown;
            status1_PortAddressProgrammingAuthority = Status1_PortAddressProgrammingAuthority.NotUsed;
            status1_FirmwareBoot = Status1_FirmwareBoot.NormalFirmwareBoot;
            status1_RDM = Status1_RDM.NoRDM;
            status1_UBEA = Status1_UBEA.UBEANotPresent;



            pkt_ID = new byte[] { 0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00 };//"Art-Net"; 

            pkt_OpCodeLo[0] = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(OpCodes.OpPollReply)[0];
            pkt_OpCodeHi[0] = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(OpCodes.OpPollReply)[1];
            pkt_IPAddress = ArtUtils.InterfaceIPAddress.GetAddressBytes();
            pkt_PortLo[0] = ArtUtils.GetLowHighFromInt(ArtUtils.ArtnetPort)[0];
            pkt_PortHi[0] = ArtUtils.GetLowHighFromInt(ArtUtils.ArtnetPort)[1];
            pkt_VersInfoH[0] = ArtUtils.GetLowHighFromInt(ArtUtils.ControllerFirmwareNumber)[1]; 
            pkt_VersInfoL[0] = ArtUtils.GetLowHighFromInt(ArtUtils.ControllerFirmwareNumber)[0];
            pkt_NetSwitch[0] = (byte)(pkt_PortHi[0] & 0x7F);
            pkt_SubSwitch[0] = (byte)((pkt_PortLo[0]>> 4) & 0x0F);
            pkt_OemHi[0] = ArtUtils.GetLowHighFromInt(ArtUtils.OemCode)[1];
            pkt_OemLo[0] = ArtUtils.GetLowHighFromInt(ArtUtils.OemCode)[0];
            pkt_UbeaVersion[0] = 0x00;
            pkt_Status1[0] = (byte)((int)status1_IndicatorState + (int)status1_PortAddressProgrammingAuthority + (int)status1_FirmwareBoot + (int)status1_RDM + (int)status1_UBEA);
            pkt_EstaManLo[0] = ArtUtils.GetLowHighFromInt(ArtUtils.ESTACode)[0];
            pkt_EstaManhi[0] = ArtUtils.GetLowHighFromInt(ArtUtils.ESTACode)[1];
            pkt_PortName = new byte[] { 0x55, 0x6e, 0x69, 0x74, 0x79, 0x41, 0x72, 0x74, 0x6e, 0x65, 0x74,0x4e,0x6f,0x64,0x65,0x30,0x31};
            //pkt_LongName
            Array.Copy(pkt_PortName, 0, pkt_LongName, 0, pkt_PortName.Length);
            //pkt_NodeReport
            NodeReport report = NodeReport.RcPowerOk;
            byte[] reportbytes = new byte[] { 0x23, 0x00, 0x00, 0x00, 0x01, 0x5b, 0x00, 0x00, 0x00, 0x00, 0x5d, 0x00, 0x00, 0x00, 0x00 };
            Array.Copy(reportbytes, 0, pkt_NodeReport, 0, reportbytes.Length);
            //--
            pkt_NumPortsHi[0] = 0x00;
            pkt_NumPortsLo[0] = 0x00; //Amount of input or output ports
            //PortTypes
            //TODO: Implment it working; - portTypes_Type - portTypes_PortCanInputToArtnetNetwork - portTypes_PortCanOutputDataFromArtnetNetwork
            pkt_PortTypes = new byte[] { 0xc0, 0xc0, 0xc0, 0xc0 };
            // 128 Data Received - 64 Channel Includes DMX512 Test Packets - 32 Channel Includes DMX512 SIPs - 16 DMX512 text packets - 8 input disabled - 4 recieve errors detected. - 2 and 1 not used.
            pkt_GoodInput = new byte[] { 0x08, 0x08, 0x08, 0x08 };
            // 128 ArtDMX Or sACN data outputed as DMX512 on this port - 64 Channel Includes DMX512 Test Packets - 32 Channel Includes DMX512 SIPs - 16 DMX512 text packets - 8 Output is merging artnet data - 4 DMX Output short detected on power up - 2 Merge Mode is LTP - 1 Output is selected to transmit sACN. Clr - Output is selected to transmit artnet.
            pkt_GoodInput = new byte[] { 0x00, 0x00, 0x00, 0x00 };
            //TODO: Implment it working; 
            //pkt_SwIn
            //TODO: Implment it working; 
            //pkt_SwOut
            //pkt_AcnPriority
            //pkt_SwMacro
            //pkt_SwRemote
            //pkt_Spare1
            //pkt_Spare2
            //pkt_spare3
            pkt_Style[0] = (byte)StyleCode.StController;
            pkt_Mac = ArtUtils.InterfaceMacAddress.GetAddressBytes();
            CompilePacket();
            return pkt_fullReturn;
        }

        private void CompilePacket()
        {
            Array.Copy(pkt_ID, 0, pkt_fullReturn, 0, pkt_ID.Length);
            Array.Copy(pkt_OpCodeLo, 0, pkt_fullReturn, 8, pkt_OpCodeLo.Length);
            Array.Copy(pkt_OpCodeHi, 0, pkt_fullReturn, 9, pkt_OpCodeHi.Length);
            Array.Copy(pkt_IPAddress, 0, pkt_fullReturn, 10, pkt_IPAddress.Length);
            Array.Copy(pkt_PortLo, 0, pkt_fullReturn, 14, pkt_PortLo.Length);
            Array.Copy(pkt_PortHi, 0, pkt_fullReturn, 15, pkt_PortHi.Length);
            Array.Copy(pkt_VersInfoH, 0, pkt_fullReturn, 16, pkt_VersInfoH.Length);
            Array.Copy(pkt_VersInfoL, 0, pkt_fullReturn, 17, pkt_VersInfoL.Length);
            Array.Copy(pkt_NetSwitch, 0, pkt_fullReturn, 18, pkt_NetSwitch.Length);
            Array.Copy(pkt_SubSwitch, 0, pkt_fullReturn, 19, pkt_SubSwitch.Length);
            Array.Copy(pkt_OemHi, 0, pkt_fullReturn, 20, pkt_OemHi.Length);
            Array.Copy(pkt_OemLo, 0, pkt_fullReturn, 21, pkt_OemLo.Length);
            Array.Copy(pkt_UbeaVersion, 0, pkt_fullReturn, 22, pkt_UbeaVersion.Length);
            Array.Copy(pkt_Status1, 0, pkt_fullReturn, 23, pkt_Status1.Length);
            Array.Copy(pkt_EstaManLo, 0, pkt_fullReturn, 24, pkt_EstaManLo.Length);
            Array.Copy(pkt_EstaManhi, 0, pkt_fullReturn, 25, pkt_EstaManhi.Length);
            Array.Copy(pkt_PortName, 0, pkt_fullReturn, 26, pkt_PortName.Length);
            Array.Copy(pkt_LongName, 0, pkt_fullReturn, 44, pkt_LongName.Length);
            Array.Copy(pkt_NodeReport, 0, pkt_fullReturn, 108, pkt_NodeReport.Length);
            Array.Copy(pkt_NumPortsHi, 0, pkt_fullReturn, 172, pkt_NumPortsHi.Length);
            Array.Copy(pkt_NumPortsLo, 0, pkt_fullReturn, 173, pkt_NumPortsLo.Length);
            Array.Copy(pkt_PortTypes, 0, pkt_fullReturn, 174, pkt_PortTypes.Length);
            Array.Copy(pkt_GoodInput, 0, pkt_fullReturn, 178, pkt_GoodInput.Length);
            Array.Copy(pkt_GoodOutputA, 0, pkt_fullReturn, 182, pkt_GoodOutputA.Length);
            Array.Copy(pkt_SwIn, 0, pkt_fullReturn, 186, pkt_SwIn.Length);
            Array.Copy(pkt_SwOut, 0, pkt_fullReturn, 190, pkt_SwOut.Length);
            Array.Copy(pkt_AcnPriority, 0, pkt_fullReturn, 194, pkt_AcnPriority.Length);
            Array.Copy(pkt_SwMacro, 0, pkt_fullReturn, 195, pkt_SwMacro.Length);
            Array.Copy(pkt_SwRemote, 0, pkt_fullReturn, 196, pkt_SwRemote.Length);
            Array.Copy(pkt_Spare1, 0, pkt_fullReturn, 197, pkt_Spare1.Length);
            Array.Copy(pkt_Spare2, 0, pkt_fullReturn, 198, pkt_Spare2.Length);
            Array.Copy(pkt_spare3, 0, pkt_fullReturn, 199, pkt_spare3.Length);
            Array.Copy(pkt_Style, 0, pkt_fullReturn, 200, pkt_Style.Length);
            Array.Copy(pkt_Mac, 0, pkt_fullReturn, 201, pkt_Mac.Length);
         
        }

        public static string GetName(byte[] data)
        {
            byte[] PortName = new byte[18];
            Array.Copy(data,26, PortName, 0, 18);
            return System.Text.Encoding.ASCII.GetString(PortName);
        }



        public enum Status1_IndicatorState
        {
            Unknown = 00,
            LocateIdentifyMode = 64,
            MuteMode = 128,
            NormalMode = 192,
        }

        public enum Status1_PortAddressProgrammingAuthority
        {
            PortAddressProgrammingAuthUnknown = 00,
            AllPortAddressSetByFrontPanel = 16,
            AllPortAddressProgrammedByWebBrowser = 32,
            NotUsed = 48
        }
        
        public enum Status1_FirmwareBoot
        {
            NormalFirmwareBoot = 0,
            BootedFromRom = 4
        }

        public enum Status1_RDM
        {
            NoRDM = 0,
            RDMCapable =2
        }

        public enum Status1_UBEA
        {
            UBEANotPresent = 0,
            UBEAPresent = 1
        }

        public enum PortTypes_Type
        {
            DMX512 = 0,
            MIDI = 1,
            Avab = 2,
            ColortanCMX = 3,
            ADB_62_5 = 4,
            Artnet = 5,
            DALI = 6
        }

        

        public enum PortTypes_PortCanInputToArtnetNetwork
        {
            //128,64,32,16,8,4,2,1
            True = 64,
            False = 0
        }

        public enum PortTypes_PortCanOutputDataFromArtnetNetwork
        {
            //128,64,32,16,8,4,2,1
            True = 64,
            False = 0
        }
    }
}
