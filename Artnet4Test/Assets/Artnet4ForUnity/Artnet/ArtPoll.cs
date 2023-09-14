using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArtnetForUnity
{

    public class ArtPoll
    {
        byte[] pkt_ID = new byte[8];        //Offset 0
        byte[] pkt_OpCodeLo = new byte[1];  //Offset 8
        byte[] pkt_OpCodeHi = new byte[1];  //Offset 9
        byte[] pkt_ProtVerHi = new byte[1]; //Offset 10
        byte[] pkt_ProtVerLo = new byte[1]; //Offset 11
        byte[] pkt_Flags = new byte[1];  //Offset 12
        byte[] pkt_DiagPriority = new byte[1];  //Offset 13 
        byte[] pkt_TargetPortAddressTopHi = new byte[1];    //Offset 14 - All bytes from here onwards are optional  //TODO: Implement the onwards;
        byte[] pkt_TargetPortAddressTopLo = new byte[1];       //Offset 15
        byte[] pkt_TargetPortAddressBottomHi = new byte[1];  //Offset 16
        byte[] pkt_TargetPortAddressBottomLo = new byte[1];  //Offset 17
        byte[] pkt_EstaManHi = new byte[1];    //Offset 18 
        byte[] pkt_EstaManLo = new byte[1];    //Offset 19 
        byte[] pkt_OemHi = new byte[1];    //Offset 20 
        byte[] pkt_OemLo = new byte[1];    //Offset 21 

        /// <summary>
        /// Targeted mode allows the ArtPoll to define a range of Port-Addresses. Nodes will only 
        /// reply to the ArtPoll is they are subscribed to a Port-Address that is inclusively in the
        /// range TargetPortAddressBottom to TargetPortAddressTop.The bit field ArtPoll->Flags->5 
        /// is used to enable Targeted Mode.
        /// </summary>
        public TargetedMode _TargetedMode;
        public VLCTransmission _VLCTransmission;
        public DiagnosticMessagesNetworkMode _DiagnosticMessagesNetworkMode;
        public DiagnosticMessages _DiagnosticsMessages;
        public ArtPollReplyMode _ArtPollReplyMode;
        public ArtnetForUnity.PriorityCodes _PriorityCodes;

        byte[] pkt_fullReturn;

        public byte[] CreateArtPollPacket()
        {
            pkt_ID = new byte[] { 0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00 };//"Art-Net"; 

            pkt_OpCodeLo[0] = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(OpCodes.OpPoll)[0];
            pkt_OpCodeHi[0] = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(OpCodes.OpPoll)[1];
            pkt_ProtVerHi[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[1];
            pkt_ProtVerLo[0] = ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[0];
            pkt_Flags[0] = createFlatByte();
            pkt_DiagPriority[0] = (byte)_PriorityCodes;

            CompilePacket();
            return pkt_fullReturn;
        }

        private void CompilePacket()
        {
            pkt_fullReturn = new byte[pkt_ID.Length + pkt_OpCodeLo.Length + pkt_OpCodeHi.Length + pkt_ProtVerHi.Length + pkt_ProtVerLo.Length + pkt_Flags.Length + pkt_DiagPriority.Length];
            Array.Copy(pkt_ID, 0, pkt_fullReturn, 0, pkt_ID.Length);
            Array.Copy(pkt_OpCodeLo, 0, pkt_fullReturn, 8, pkt_OpCodeLo.Length);
            Array.Copy(pkt_OpCodeHi, 0, pkt_fullReturn, 9, pkt_OpCodeHi.Length);
            Array.Copy(pkt_ProtVerHi, 0, pkt_fullReturn, 10, pkt_ProtVerHi.Length);
            Array.Copy(pkt_ProtVerLo, 0, pkt_fullReturn, 11, pkt_ProtVerLo.Length);
            Array.Copy(pkt_Flags, 0, pkt_fullReturn, 12, pkt_Flags.Length);
            Array.Copy(pkt_DiagPriority, 0, pkt_fullReturn, 13, pkt_DiagPriority.Length);

        }

        public void ReadArtPollPacket(byte[] data)
        {
            if (data == null) return;
        }


        private byte createFlatByte()
        {
            byte returnedByte = 0x00;
            returnedByte = (byte)((int)_TargetedMode + (int)_VLCTransmission + (int)_DiagnosticMessagesNetworkMode + (int)_DiagnosticsMessages + (int)_ArtPollReplyMode);

            return returnedByte;
        }


        public enum TargetedMode
        {
            DisableTargetedMode = 0,
            EnableTargetedMode = 16
        }

        public enum VLCTransmission
        {
            EnableVLCTransmission = 0,
            DisableVLCTransmission = 8
        }

        public enum DiagnosticMessagesNetworkMode
        {
            BroadcastDiagnosticMessages = 0,
            UnicastDiagnosticMessages = 4
        }

        public enum DiagnosticMessages
        {
            NoDiagnosticMessages = 0,
            SendDiagnosticMessages = 2
        }

        public enum ArtPollReplyMode
        {
            SendWhenPolled = 0, //= Only send ArtPollReply in response to an ArtPoll or ArtAddress.
            SendWhenChanged = 1 // Send ArtPollReply whenever Node conditions change. This selection allows the Controller to be informed of changes without the need to continuously poll
        }
    }
}
