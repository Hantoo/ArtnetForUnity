using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtnetForUnity.Timecode
{

    public class TimecodeManager
    {
        
        public static TimecodeTime CurrentTimecode;

        public void init()
        {
            CurrentTimecode = new TimecodeTime();
        }

        //public static event EventHandler<TimecodeEvent> TimecodeTriggerEvent;
        public delegate void TimecodeRecievedUpdate(ArtTimecode tc);
        public static TimecodeRecievedUpdate TimecodeUpdate;

        public ArtTimecode ParseTimecodePacket(IPPacket pkt, ArtnetDevice device)
        {

            ArtTimecode TimecodePkt = new ArtTimecode();
            TimecodePkt.senderIP = pkt.ipAddress;
            TimecodePkt.senderName = device.name;

            TimecodePkt.frames = (int)pkt.pktData[14];
            TimecodePkt.seconds = (int)pkt.pktData[15];
            TimecodePkt.mintues = (int)pkt.pktData[16];
            TimecodePkt.hours = (int)pkt.pktData[17];
            TimecodePkt.timecodeFPS = (TimecodeType)pkt.pktData[18];

            return TimecodePkt;
        }

        public void UpdateCurrentTimecode(ArtTimecode timecodePkt)
        {
            if (timecodePkt.senderIP == null) return;
            CurrentTimecode.tc = timecodePkt;
            UpdateTimecodeUI();
          
        }

        private void UpdateTimecodeUI()
        {
            TimecodeUpdate?.Invoke(CurrentTimecode.tc);
        }


    }

    [Serializable]
    public class TimecodeTime 
    {
       
        [SerializeField]
        public ArtTimecode tc { get; set; }
    }


    public class TimecodeEvent : EventArgs
    {
        public ArtTimecode Data { get; set; }
        public TimecodeEvent(ArtTimecode data)
        {
            Data = data;
        }
    }

}