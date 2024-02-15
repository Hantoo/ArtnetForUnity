using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;
using ArtnetForUnity;

public class GLPx4Tester : MonoBehaviour
{
    IPAddress IP_ArtnetNode1;

    private byte[] _data;
    [Range(0,255)]
    public byte MasterIntensity;

    [Range(0, 255)]
    public byte Red;
    [Range(0, 255)]
    public byte Green;
    [Range(0, 255)]
    public byte Blue;
    [Range(0, 255)]
    public byte White;
    [Range(0, 255)]
    public byte Zoom;

    [Range(0, 255)]
    public byte CoolWhite; 
    [Range(0, 255)]
    public byte WarmWhite;
    [Range(0, 255)]
    public byte Output3Zoom;

    public enum mode
    {
        staticLight,
        animation
    }

    public mode Mode;
    public AnimationCurve RedCurve;
    public AnimationCurve GreenCurve;
    public AnimationCurve BlueCurve;
    public AnimationCurve WhiteCurve;
    public AnimationCurve Zoom1Curve;
    public AnimationCurve CoolWhiteCurve; 
    public AnimationCurve WarmWhiteCurve;
    public AnimationCurve Zoom2Curve;

    
    ArtnetForUnity.ArtnetManager artnetManager;
    public void Start()
    {
        _data = new byte[512];
        //client = new UdpClient();
        artnetManager = new ArtnetForUnity.ArtnetManager();
        artnetManager.Start();

    }

    private void OnDestroy()
    {
        artnetManager.Stop();
        artnetManager.Dispose();
    }


    public void Update()
    {
        _data = new byte[512];
        _data[0] = (byte)MasterIntensity;
        _data[1] = (byte)0;
        switch (Mode)
        {
            case mode.staticLight:

                _data[15] = (byte)255;
                _data[16] = (byte)Red;
                _data[17] = (byte)Green;
                _data[18] = (byte)Blue;
                _data[19] = (byte)White;
                _data[20] = (byte)Zoom;

                _data[3] = (byte)255;
                _data[4] = (byte)CoolWhite;
                _data[5] = (byte)WarmWhite;
                _data[6] = (byte)CoolWhite;
                _data[7] = (byte)WarmWhite;
                _data[8] = (byte)Output3Zoom;
                break;
            case mode.animation:
                _data[15] = (byte)255;
                _data[16] = (byte)(RedCurve.Evaluate(Time.time % RedCurve.keys[RedCurve.keys.Length-1].time)*255);
                _data[17] = (byte)(GreenCurve.Evaluate(Time.time % GreenCurve.keys[GreenCurve.keys.Length - 1].time) * 255); ;
                _data[18] = (byte)(BlueCurve.Evaluate(Time.time % BlueCurve.keys[BlueCurve.keys.Length - 1].time) * 255); ;
                _data[19] = (byte)(WhiteCurve.Evaluate(Time.time % WhiteCurve.keys[WhiteCurve.keys.Length - 1].time) * 255); ;
                _data[20] = (byte)(Zoom1Curve.Evaluate(Time.time % Zoom1Curve.keys[Zoom1Curve.keys.Length - 1].time) * 255); ;

                _data[3] = (byte)255;
                _data[4] = (byte)(CoolWhiteCurve.Evaluate(Time.time % CoolWhiteCurve.keys[CoolWhiteCurve.keys.Length - 1].time) * 255); ;
                _data[5] = (byte)(WarmWhiteCurve.Evaluate(Time.time % WarmWhiteCurve.keys[WarmWhiteCurve.keys.Length - 1].time) * 255); ;
                _data[6] = (byte)(CoolWhiteCurve.Evaluate(Time.time % CoolWhiteCurve.keys[CoolWhiteCurve.keys.Length - 1].time) * 255); ;
                _data[7] = (byte)(WarmWhiteCurve.Evaluate(Time.time % WarmWhiteCurve.keys[WarmWhiteCurve.keys.Length - 1].time) * 255); ;
                _data[8] = (byte)(Zoom2Curve.Evaluate(Time.time % Zoom2Curve.keys[Zoom2Curve.keys.Length - 1].time)*255); ;
                break;
        }
        //PSU
      
      



        //Send Artnet to Direct IP Addresses in settings
        artnetManager.SetArtnetData(0, _data);
        //artnetManager.SetArtnetData(1, _data);
        //artnetManager.SetArtnetData(2, _data);
        //artnetManager.SetArtnetData(3, _data);
    }

}
