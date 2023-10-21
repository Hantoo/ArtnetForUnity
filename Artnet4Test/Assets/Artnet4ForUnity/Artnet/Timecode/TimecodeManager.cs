using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtnetForUnity.Timecode
{

    public class TimecodeManager
    {
        
        public static TimecodeTime CurrentTimecode;
        public static ArtTimecode SenderTimecode;
        private ArtTimecode SetTimecode;
        private ArtnetManager ArtManager;
        private TimecodeState timeCodeState;
        Stopwatch TimecodeSystemTime = new Stopwatch();
        float systemTimeMiliseconds;
        float SenderFramesPerSecond = 30;
        private enum TimecodeState
        {
            Pause = 0,
            Play = 1,
            Reset = 2
        }

        public void init(ArtnetManager manager)
        {
            ArtManager = manager;
            CurrentTimecode = new TimecodeTime();
            SenderTimecode = new ArtTimecode();
            SenderTimecode.mode = TimecodeMode.Sender;
            SetTimecode = new ArtTimecode();
            SetTimeCodeType(TimecodeType.SMPTE_30FPS);
            SetTimecode.timecodeFPS = TimecodeType.SMPTE_30FPS;
            TimecodeSystemTime.Stop();
            playTimecode += PlayTimeCode;
            pauseTimecode += PauseTimeCode;
            resetTimecode += ResetTimeCode;
        }

        public void Dispose()
        {

            playTimecode -= PlayTimeCode;
            pauseTimecode -= PauseTimeCode;
            resetTimecode -= ResetTimeCode;
        }

        public void Update()
        {
            switch (timeCodeState)
            {
                case TimecodeState.Play:
                    CalculateTimeCodeIncrement();
                    SendTimecode();
                    break;
                case TimecodeState.Pause:
                    TimecodeSystemTime.Stop();
                    TimecodeSystemTime.Reset();
                    //Do Nothing
                    break;
                case TimecodeState.Reset:
                    TimecodeSystemTime.Stop();
                    TimecodeSystemTime.Reset();
                    SenderTimecode = SetTimecode;
                    timeCodeState = TimecodeState.Pause;
                    UpdateTimecodeUI(SenderTimecode);
                    break;

            }
        }

        private void SendTimecode()
        {
            IPPacket packet = new IPPacket();
            packet.ipAddress = ArtUtils.broadcastAddress;
            packet.opCode = OpCodes.OpTimeCode;
            packet.pktData = ParseTimecodePacket();
            ArtManager.AddSenderPkt(packet);
        }

        private byte[] ParseTimecodePacket()
        {
            byte[] packetData = new byte[19];
            byte[] pkt_ID = new byte[] { 0x41, 0x72, 0x74, 0x2D, 0x4E, 0x65, 0x74, 0x00 };//"Art-Net"; 

            byte[] pkt_OpCodeLoHi = ArtnetForUnity.ArtUtils.GetOpCodeLowHighBytes(OpCodes.OpTimeCode);
            byte[] pkt_ProtVerHi = new byte[] { ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[1], ArtnetForUnity.ArtUtils.GetLowHighFromInt(ArtnetForUnity.ArtUtils.ArtnetProtocolRevisionNumber)[0] };
            byte[] timecode = new byte[] {0x00, 0x00, (byte)SenderTimecode.frames, (byte)SenderTimecode.seconds, (byte)SenderTimecode.mintues, (byte)SenderTimecode.hours, (byte)SenderTimecode.timecodeFPS };

            Array.Copy(pkt_ID, 0, packetData, 0, pkt_ID.Length);
            Array.Copy(pkt_OpCodeLoHi, 0, packetData, 8, pkt_OpCodeLoHi.Length);
            Array.Copy(pkt_ProtVerHi, 0, packetData, 10, pkt_ProtVerHi.Length);
            Array.Copy(timecode, 0, packetData, 12, timecode.Length);

            return packetData;
        }

        //public static event EventHandler<TimecodeEvent> TimecodeTriggerEvent;
        public delegate void TimecodeRecievedUpdate(ArtTimecode tc);
        public static TimecodeRecievedUpdate TimecodeUpdate;

        public delegate void PlayTimecode();
        public static PlayTimecode playTimecode;
        
        public delegate void PauseTimecode();
        public static PauseTimecode pauseTimecode;
        
        public delegate void ResetTimecode();
        public static ResetTimecode resetTimecode;
    

        public ArtTimecode ParseTimecodePacket(IPPacket pkt, ArtnetDevice device)
        {

            ArtTimecode TimecodePkt = new ArtTimecode();
            TimecodePkt.senderIP = pkt.ipAddress;
            TimecodePkt.senderName = device.name;
            TimecodePkt.mode = TimecodeMode.Receiver;
            TimecodePkt.frames = (int)pkt.pktData[14];
            TimecodePkt.seconds = (int)pkt.pktData[15];
            TimecodePkt.mintues = (int)pkt.pktData[16];
            TimecodePkt.hours = (int)pkt.pktData[17];
            TimecodePkt.timecodeFPS = (TimecodeType)pkt.pktData[18];

            return TimecodePkt;
        }

        public void UpdateCurrentTimecodeFromPacket(ArtTimecode timecodePkt)
        {
            if (timecodePkt.senderIP == null) return;
            CurrentTimecode.tc = timecodePkt;
            UpdateTimecodeUI(CurrentTimecode.tc);
          
        }

        private void UpdateTimecodeUI(ArtTimecode tc)
        {
            TimecodeUpdate?.Invoke(tc);
        }


        
        private void CalculateTimeCodeIncrement()
        {
            TimecodeSystemTime.Stop();

            systemTimeMiliseconds += TimecodeSystemTime.ElapsedMilliseconds;
            TimecodeSystemTime.Restart();

            SenderTimecode.frames = (int)(SenderFramesPerSecond * (float)(systemTimeMiliseconds / 1000));
            if (systemTimeMiliseconds >= 1000)
            {
                systemTimeMiliseconds = systemTimeMiliseconds % 1000;
                SenderTimecode.seconds++;
            }
            if (SenderTimecode.seconds >= 60)
            {
                SenderTimecode.seconds = 0;
                SenderTimecode.mintues++;
            }
            if (SenderTimecode.mintues >= 60)
            {
                SenderTimecode.mintues = 0;
                SenderTimecode.hours++;
            }
            if (SenderTimecode.hours >= 24)
            {
                timeCodeState = TimecodeState.Pause;
                SetTimeCode(0, 0, 0, 0);
            }
            SenderTimecode.mode = TimecodeMode.Sender;
            UpdateTimecodeUI(SenderTimecode);
            
        }

        public void SetTimeCode(int hour, int minute, int second, int frame)
        {
            SetTimeCodeHour(hour);
            SetTimeCodeMinute(minute);
            SetTimeCodeSecond(second);
            SetTimeCodeFrame(frame);
        }

        public void SetTimeCodeHour(int hour)
        {
            SetTimecode.hours = (hour % 24);
            SenderTimecode = SetTimecode;
        }

        public void SetTimeCodeMinute(int minute)
        {
            SetTimecode.mintues = (minute % 60);
            SenderTimecode = SetTimecode;
        }

        public void SetTimeCodeSecond(int second)
        {
            SetTimecode.seconds = (second % 60);
            SenderTimecode = SetTimecode;
        }

        public void SetTimeCodeFrame(int frame)
        {
            float limit = 0;
            switch (SenderTimecode.timecodeFPS)
            {
                case TimecodeType.Film_24FPS:
                    limit = 24;
                    break;
                case TimecodeType.EBU_25FPS:
                    limit = 25;
                    break;
                case TimecodeType.DF_29_97FPS:
                    limit = 29.97f;
                    break;
                case TimecodeType.SMPTE_30FPS:
                    limit = 30;
                    
                    break;
                SenderFramesPerSecond = limit;
            }
            SetTimecode.frames = (frame % (int)limit);
            SenderTimecode = SetTimecode;
        }


        public void SetTimeCodeType(TimecodeType type)
        {
            
            SenderTimecode.timecodeFPS = type;
        }

        public void PlayTimeCode()
        {
            timeCodeState = TimecodeState.Play;
        }

        public void PauseTimeCode()
        {
            timeCodeState = TimecodeState.Pause;
        }

        public void ResetTimeCode()
        {
            timeCodeState = TimecodeState.Reset;
        }




    }

    [Serializable]
    public class TimecodeTime 
    {
       
        [SerializeField]
        public ArtTimecode tc { get; set; }
    }


}