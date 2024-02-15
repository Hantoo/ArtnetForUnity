using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.XR;


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
        byte[] pkt_GoodOutput = new byte[4];    //Offset 182 
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
        byte[] pkt_BindIndex = new byte[1];    //Offset 207
        byte[] pkt_Status2 = new byte[1];    //Offset 211 
        byte[] pkt_GoodOutputB = new byte[4];    //Offset 213 
        byte[] pkt_Status3 = new byte[1];    //Offset 217
        byte[] pkt_DefaulRespUIDHi = new byte[1];    //Offset 218 
        byte[] pkt_DefaulRespUID1 = new byte[1];    //Offset 219 
        byte[] pkt_DefaulRespUID2 = new byte[1];    //Offset 220 
        byte[] pkt_DefaulRespUID3 = new byte[1];    //Offset 221 
        byte[] pkt_DefaulRespUID4 = new byte[1];    //Offset 222 
        byte[] pkt_DefaulRespUIDLo = new byte[1];    //Offset 223 
        byte[] pkt_UserHi = new byte[1];    //Offset 224
        byte[] pkt_UserLo = new byte[1];    //Offset 225 
        byte[] pkt_RefreshRateHi = new byte[1];    //Offset 226 
        byte[] pkt_RefreshRateLo = new byte[1];    //Offset 227 
        byte[] pkt_Filler = new byte[11];    //Offset 228 
        
        byte[] pkt_fullReturn = new byte[239];
        private Status1_IndicatorState status1_IndicatorState;
        private Status1_PortAddressProgrammingAuthority status1_PortAddressProgrammingAuthority;
        private Status1_FirmwareBoot status1_FirmwareBoot;
        private Status1_RDM status1_RDM;
        private Status1_UBEA status1_UBEA;

        public PortTypes_Type portTypes_Type;
        public PortTypes_PortCanInputToArtnetNetwork portTypes_PortCanInputToArtnetNetwork;
        public PortTypes_PortCanOutputDataFromArtnetNetwork portTypes_PortCanOutputDataFromArtnetNetwork;
        static int ArtPollReplyCounter = 0;
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
            pkt_PortName = Encoding.ASCII.GetBytes(ArtUtils.Truncate(System.Environment.MachineName,18));
            //pkt_LongName
            Array.Copy(pkt_PortName, 0, pkt_LongName, 0, pkt_PortName.Length);
            //pkt_NodeReport
            NodeReport report = NodeReport.RcPowerOk;
            byte[] reportbytes = new byte[64];
            string nodeReportCodeString = ((int)report).ToString("0000");
            byte[] nodeReportCodeByte = Encoding.ASCII.GetBytes(nodeReportCodeString);
            string ArtPollReplyCounterString = ArtPollReplyCounter.ToString("0000");
            byte[] ArtPollReplyCounterByte = Encoding.ASCII.GetBytes(ArtPollReplyCounterString);
            byte[] reportbytes_test = new byte[] { 0x23, nodeReportCodeByte[0], nodeReportCodeByte[1], nodeReportCodeByte[2], nodeReportCodeByte[3], 0x20, 0x5b, ArtPollReplyCounterByte[0] , ArtPollReplyCounterByte[1], ArtPollReplyCounterByte[2] , ArtPollReplyCounterByte[3], 0x5d, 0x20, 0x00, 0x00, 0x00, 0x00 };
            Array.Copy(reportbytes_test, 0, reportbytes, 0, reportbytes_test.Length);
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
            if(ArtUtils.InterfaceMacAddress != null)
                pkt_Mac = ArtUtils.InterfaceMacAddress.GetAddressBytes();
            pkt_BindIP = ArtUtils.InterfaceIPAddress.GetAddressBytes(); //Find better way for root ip identifiction
            pkt_Status2[0] = (byte)(ParseStatus2Byte(false, false, false, false, true, true, ArtUtils.GetDhcp(), false));
            pkt_GoodOutputB = new byte[] { 0xC0, 0xC0, 0xC0, 0xC0 }; // 128 = 1 RDM disabled, 0 RDM Enabled, 64 = 1 Output Continious, 0 output delta, rest of bits not used
            pkt_Status3[0] = (byte)(ParseStatus3Byte(Status3_HoldingState.AllOutputsToZero, false, false, false));
            pkt_DefaulRespUIDHi[0] = 0x00;
            pkt_DefaulRespUID1[0] = 0x00;
            pkt_DefaulRespUID2[0] = 0x00;
            pkt_DefaulRespUID3[0] = 0x00;
            pkt_DefaulRespUID4[0] = 0x00;
            pkt_DefaulRespUIDLo[0] = 0x00;
            pkt_UserHi[0] = 0x00;
            pkt_UserLo[0] = 0x00;
            pkt_UserLo[0] = 0x00;
            pkt_RefreshRateHi[0] = 0x00;
            pkt_RefreshRateLo[0] = 0x00;
            pkt_Filler[0] = 0x00;
            CompilePacket();
            ArtPollReplyCounter = ArtPollReplyCounter + 1;
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
            Array.Copy(pkt_GoodOutput, 0, pkt_fullReturn, 182, pkt_GoodOutput.Length);
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
            Array.Copy(pkt_BindIP, 0, pkt_fullReturn, 207, pkt_BindIP.Length);
            Array.Copy(pkt_BindIndex, 0, pkt_fullReturn, 211, pkt_BindIndex.Length);
            Array.Copy(pkt_Status2, 0, pkt_fullReturn, 212, pkt_Status2.Length);
            Array.Copy(pkt_GoodOutputB, 0, pkt_fullReturn, 213, pkt_GoodOutputB.Length);
            Array.Copy(pkt_Status3, 0, pkt_fullReturn, 217, pkt_Status3.Length);
            Array.Copy(pkt_DefaulRespUIDHi, 0, pkt_fullReturn, 218, pkt_DefaulRespUIDHi.Length);
            Array.Copy(pkt_DefaulRespUID1, 0, pkt_fullReturn, 219, pkt_DefaulRespUID1.Length);
            Array.Copy(pkt_DefaulRespUID2, 0, pkt_fullReturn, 220, pkt_DefaulRespUID2.Length);
            Array.Copy(pkt_DefaulRespUID3, 0, pkt_fullReturn, 221, pkt_DefaulRespUID3.Length);
            Array.Copy(pkt_DefaulRespUID4, 0, pkt_fullReturn, 222, pkt_DefaulRespUID4.Length);
            Array.Copy(pkt_DefaulRespUIDLo, 0, pkt_fullReturn, 223, pkt_DefaulRespUIDLo.Length);
            Array.Copy(pkt_UserHi, 0, pkt_fullReturn, 224, pkt_UserHi.Length);
            Array.Copy(pkt_UserLo, 0, pkt_fullReturn, 225, pkt_UserLo.Length);
            Array.Copy(pkt_RefreshRateHi, 0, pkt_fullReturn, 226, pkt_RefreshRateHi.Length);
            Array.Copy(pkt_RefreshRateLo, 0, pkt_fullReturn, 227, pkt_RefreshRateLo.Length);
            Array.Copy(pkt_Filler, 0, pkt_fullReturn, 228, pkt_Filler.Length);

        }

        public static string GetName(byte[] data)
        {
            byte[] PortName = new byte[18];
            Array.Copy(data, 26, PortName, 0, PortName.Length);
            //Remove any 0x00 in array as it messes up concat-ing strings
            for (int i = 0; i < PortName.Length; i++)
                if (PortName[i] == 0x00) PortName[i] = 0x20;
            
           
            byte[] LongName = new byte[64];
            Array.Copy(data,44, LongName, 0, LongName.Length);
            for (int i = 0; i < LongName.Length; i++)
                if (LongName[i] == 0x00) LongName[i] = 0x20;

           
            string name = String.Format("[{1}] | {0}",System.Text.Encoding.ASCII.GetString(PortName).TrimEnd().ToString(),(System.Text.Encoding.ASCII.GetString(LongName).TrimEnd().ToString()));
          
            return (name); 
        }

        public static int GetBindIndex(byte[] data)
        {
            return (int)data[211];
        }

        public struct GoodInput
        {
            public void ParseByteToGoodInput(byte GoodInputValue)
            {
                if (ArtUtils.IsBitSet(GoodInputValue,7)) { dataRecieved = GoodInput_DataRecieved.DataReceieved; } else { dataRecieved = GoodInput_DataRecieved.NoData; }
                if (ArtUtils.IsBitSet(GoodInputValue, 6)) { incDMXTestPkts = GoodInput_IncDMXTestPkts.TestPacketsIncluded; } else { incDMXTestPkts = GoodInput_IncDMXTestPkts.NoTestPackets; }
                if (ArtUtils.IsBitSet(GoodInputValue, 5)) { incDMXSIPPkts = GoodInput_IncDMXSIPPkts.SIPPacketsIncluded; } else { incDMXSIPPkts = GoodInput_IncDMXSIPPkts.NoSIPPackets; }
                if (ArtUtils.IsBitSet(GoodInputValue, 4)) { incDMXTextPkts = GoodInput_IncDMXTextPkts.TextPacketsIncluded; } else { incDMXTextPkts = GoodInput_IncDMXTextPkts.NoTextPackets; }
                if (ArtUtils.IsBitSet(GoodInputValue, 3)) { inputDisabled = GoodInput_InputDisabled.Disabled; } else { inputDisabled = GoodInput_InputDisabled.Enabled; }
                if (ArtUtils.IsBitSet(GoodInputValue, 2)) { recieveErrorsDetected = GoodInput_RecieveErrorsDetected.ErrorsDetected; } else { recieveErrorsDetected = GoodInput_RecieveErrorsDetected.NoErrors; }
                // if (ArtUtils.IsBitSet(GoodInputValue, 1)) { NOT USED }
                // if (ArtUtils.IsBitSet(GoodInputValue, 0)) { NOT USED }
            }

            public byte GoodInputToByte()
            {
                int returnValue = 0;
                if (dataRecieved == GoodInput_DataRecieved.DataReceieved) returnValue += 128;
                if (incDMXTestPkts == GoodInput_IncDMXTestPkts.TestPacketsIncluded) returnValue += 64;
                if (incDMXSIPPkts == GoodInput_IncDMXSIPPkts.SIPPacketsIncluded) returnValue += 32;
                if (incDMXTextPkts == GoodInput_IncDMXTextPkts.TextPacketsIncluded) returnValue += 16;
                if (inputDisabled == GoodInput_InputDisabled.Disabled) returnValue += 8;
                if (recieveErrorsDetected == GoodInput_RecieveErrorsDetected.ErrorsDetected) returnValue += 4;

                return (byte)returnValue;
            }

            public byte byteValue;
            public GoodInput_DataRecieved dataRecieved;
            public GoodInput_IncDMXTestPkts incDMXTestPkts;
            public GoodInput_IncDMXSIPPkts incDMXSIPPkts;
            public GoodInput_IncDMXTextPkts incDMXTextPkts;
            public GoodInput_InputDisabled inputDisabled;
            public GoodInput_RecieveErrorsDetected recieveErrorsDetected;

            public enum GoodInput_RecieveErrorsDetected
            {
                //128,64,32,16,8,4,2,1
                ErrorsDetected = 4,
                NoErrors = 0
            }

            public enum GoodInput_InputDisabled
            {
                Disabled = 8,
                Enabled = 0
            }

            public enum GoodInput_IncDMXTextPkts
            {
                TextPacketsIncluded = 16,
                NoTextPackets = 0
            }

            public enum GoodInput_IncDMXSIPPkts
            {
                SIPPacketsIncluded = 32,
                NoSIPPackets = 0
            }

            public enum GoodInput_IncDMXTestPkts
            {
                TestPacketsIncluded = 64,
                NoTestPackets = 0
            }

            public enum GoodInput_DataRecieved
            {
                DataReceieved = 128,
                NoData = 0
            }
        }

        public struct GoodOutput
        {
            public void ParseByteToGoodInput(byte GoodOutputValue)
            {
                if (ArtUtils.IsBitSet(GoodOutputValue, 7)) { DMXOutput = GoodOutput_ArtDmxOrScanOutput.ArtDmxOrsACNOutputAsDMX; } else { DMXOutput = GoodOutput_ArtDmxOrScanOutput.NoArtDMXorsACN; }
                if (ArtUtils.IsBitSet(GoodOutputValue, 6)) { incDMXTestPkts = GoodOutput_IncDMXTestPkts.TestPacketsIncluded; } else { incDMXTestPkts = GoodOutput_IncDMXTestPkts.NoTestPackets; }
                if (ArtUtils.IsBitSet(GoodOutputValue, 5)) { incDMXSIPPkts = GoodOutput_IncDMXSIPPkts.SIPPacketsIncluded; } else { incDMXSIPPkts = GoodOutput_IncDMXSIPPkts.NoSIPPackets; }
                if (ArtUtils.IsBitSet(GoodOutputValue, 4)) { incDMXTextPkts = GoodOutput_IncDMXTextPkts.TextPacketsIncluded; } else { incDMXTextPkts = GoodOutput_IncDMXTextPkts.NoTextPackets; }
                if (ArtUtils.IsBitSet(GoodOutputValue, 3)) { mergeArtnet = GoodOutput_OutputMergingArtNetData.IsMerging; } else { mergeArtnet = GoodOutput_OutputMergingArtNetData.IsNotMerging; }
                if (ArtUtils.IsBitSet(GoodOutputValue, 2)) { dMXShort = GoodOutput_DMXOutputShort.OutputShorted; } else { dMXShort = GoodOutput_DMXOutputShort.NoShort; }
                if (ArtUtils.IsBitSet(GoodOutputValue, 1)) { lTPMerge = GoodOutput_LTPMergeMode.LTPMerge; } else { lTPMerge = GoodOutput_LTPMergeMode.NoLTPMerge; }
                if (ArtUtils.IsBitSet(GoodOutputValue, 0)) { outputProtocol = GoodOutput_Output.TransmitsACN; } else { outputProtocol = GoodOutput_Output.TransmitsArtNet; }
     
            }

            public byte GoodInputToByte()
            {
                int returnValue = 0;
                if (DMXOutput == GoodOutput_ArtDmxOrScanOutput.ArtDmxOrsACNOutputAsDMX) returnValue += 128;
                if (incDMXTestPkts == GoodOutput_IncDMXTestPkts.TestPacketsIncluded) returnValue += 64;
                if (incDMXSIPPkts == GoodOutput_IncDMXSIPPkts.SIPPacketsIncluded) returnValue += 32;
                if (incDMXTextPkts == GoodOutput_IncDMXTextPkts.TextPacketsIncluded) returnValue += 16;
                if (mergeArtnet == GoodOutput_OutputMergingArtNetData.IsMerging) returnValue += 8;
                if (dMXShort == GoodOutput_DMXOutputShort.OutputShorted) returnValue += 4;
                if (lTPMerge == GoodOutput_LTPMergeMode.LTPMerge) returnValue += 2;
                if (outputProtocol == GoodOutput_Output.TransmitsACN) returnValue += 1;

                return (byte)returnValue;
            }

            public byte byteValue;
            public GoodOutput_ArtDmxOrScanOutput DMXOutput;
            public GoodOutput_IncDMXTestPkts incDMXTestPkts;
            public GoodOutput_IncDMXSIPPkts incDMXSIPPkts;
            public GoodOutput_IncDMXTextPkts incDMXTextPkts;
            public GoodOutput_OutputMergingArtNetData mergeArtnet;
            public GoodOutput_DMXOutputShort dMXShort;
            public GoodOutput_LTPMergeMode lTPMerge;
            public GoodOutput_Output outputProtocol;

            public enum GoodOutput_ArtDmxOrScanOutput
            {
                //128,64,32,16,8,4,2,1
                ArtDmxOrsACNOutputAsDMX = 128,
                NoArtDMXorsACN = 0
            }

            public enum GoodOutput_IncDMXTestPkts
            {
                TestPacketsIncluded = 64,
                NoTestPackets = 0
            }


           
            public enum GoodOutput_IncDMXSIPPkts
            {
                SIPPacketsIncluded = 32,
                NoSIPPackets = 0
            }

            public enum GoodOutput_IncDMXTextPkts
            {
                TextPacketsIncluded = 16,
                NoTextPackets = 0
            }


            public enum GoodOutput_OutputMergingArtNetData
            {
                IsMerging = 8,
                IsNotMerging = 0
            }

            public enum GoodOutput_DMXOutputShort
            {
                OutputShorted = 4,
                NoShort = 0
            }

            public enum GoodOutput_LTPMergeMode
            {
                LTPMerge = 2,
                NoLTPMerge = 0
            }

            public enum GoodOutput_Output
            {
                TransmitsACN = 2,
                TransmitsArtNet = 0
            }
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

        public struct ReadableArtPoll
        {
            public IPAddress iPAddress;
            public int port;
            public int version;
            public int PortAddress;
            public int NetSwitch;
            public int SubSwitch;
            public int ESTAManufactureCode;
            public string PortName;
            public string LongName;
            public ReadableNodeReport[] nodeReport;
            public int NodeOutputPortAmount;
            public int NodeInputPortAmount;
            public int OEMCode;
            public int UbeaVersion;
            public Status1 status1;
            public GoodInput[] goodInputs;
            public GoodOutput[] goodOutputs;
        }

        public struct ReadableNodeReport
        {
            
            public void ParseNodeReport(byte[] pktData)
            {
                 char counterStartChar = '[';
                char counterEndChar = ']';
                int counterStartPos = -1;
                int counterendPos = -1;
                for(int i = 0; i < pktData.Length; i++)
                {
                    if (pktData[i] == (byte)counterStartChar) counterStartPos = i;
                    if (pktData[i] == (byte)counterEndChar) counterendPos = i;
                    //Node Report Code
                    byte[] NodeReportCode = new byte[4];
                    Array.Copy(pktData, 1, NodeReportCode, 0, 4);
                    Int32 value = BitConverter.ToInt32(NodeReportCode);
                    nodeReportFeedback = (NodeReport)value;

                    //ArtPollResponse Counter
                    byte[] ArtPollResponseTick = new byte[4];
                    Array.Copy(pktData, counterStartPos + 1, ArtPollResponseTick, 0, 4);
                    Int32 ArtPollCounter = BitConverter.ToInt32(ArtPollResponseTick);
                    ArtPollResponseCounter = ArtPollCounter;

                    //Message
                    byte[] TextArray = new byte[54];
                    Array.Copy(pktData, counterendPos+1, TextArray, 0, 4);
                    message = System.Text.Encoding.ASCII.GetString(TextArray);
                }


            //byte Pos 1 = Error Code. byte Pos 7 = Counter 
            }

            public NodeReport nodeReportFeedback;
            public Int32 ArtPollResponseCounter;
            public string message;
        }

        public int ParseStatus2Byte(bool SupportsRDMViaArtCommand, bool NodeSupportsArtCommandOutputSwitching, bool Squawking, bool AbleToSwitchBetweenArtnetAndScan, bool canUseArtnet3, bool DCHPCapable, bool isIPDCHP, bool configurableViaWebBrowser)
        {
            int val = 0;
            if (configurableViaWebBrowser) val += 1;
            if (isIPDCHP) val += 2;
            if (DCHPCapable) val += 4;
            if (canUseArtnet3) val += 8;
            if (AbleToSwitchBetweenArtnetAndScan) val += 16;
            if (Squawking) val += 32;
            if (NodeSupportsArtCommandOutputSwitching) val += 64;
            if (SupportsRDMViaArtCommand) val += 128;

            return val;
        }

        public int ParseStatus3Byte(Status3_HoldingState NoDataState, bool NodeSupportsFailOver, bool NodeSupportsLLRP, bool NodeSupportsSwitchingInputAndOutputs)
        {
            int val = 0;
            switch (NoDataState)
            {
                case Status3_HoldingState.HoldLastState:
                    val += 0;
                    break;
                case Status3_HoldingState.AllOutputsToZero:
                    val += 64;
                    break;
                case Status3_HoldingState.AllOutputsToFull:
                    val += 128;
                    break;
                case Status3_HoldingState.PlaybackDailSafeScene:
                    val += 192;
                    break;
            }
            if (NodeSupportsFailOver) val += 32;
            if (NodeSupportsLLRP) val += 16;
            if (NodeSupportsSwitchingInputAndOutputs) val += 8;
   

            return val;
        }

        public enum Status3_HoldingState
        {
            HoldLastState = 0,
            AllOutputsToZero = 1,
            AllOutputsToFull = 2,
            PlaybackDailSafeScene = 3
        }

        public struct Status1
        {
            public void status1UnPack(byte byteval)
            {

            }
            public Status1_FirmwareBoot firmwareBoot;
            public Status1_IndicatorState indicatorState;
            public Status1_PortAddressProgrammingAuthority portProgramming;
            public Status1_RDM rDM;
            public Status1_UBEA uBEA;
     


        }
    }
}
