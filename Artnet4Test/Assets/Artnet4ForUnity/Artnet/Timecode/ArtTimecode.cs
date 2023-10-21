using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

namespace ArtnetForUnity.Timecode
{

    public struct ArtTimecode 
    {
        public TimecodeMode mode;
        public IPAddress senderIP;
        public string senderName;

        public int frames; // 0-3000 - 3000 = 30Frames
        public int seconds; // 0-59
        public int mintues; // 0-59
        public int hours; // 0-23
        public TimecodeType timecodeFPS;
    }

    public enum TimecodeType
    {
        Film_24FPS = 0,
        EBU_25FPS = 1,
        DF_29_97FPS = 2,
        SMPTE_30FPS = 3
    }

    public enum TimecodeMode
    {
        Sender=0,
        Receiver=1,
    }
}
