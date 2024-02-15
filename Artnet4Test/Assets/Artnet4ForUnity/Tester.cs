using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;
using ArtnetForUnity;

public class Tester : MonoBehaviour
{
    IPAddress IP_ArtnetNode1;

    private byte[] _data;
    [SerializeField]
    private byte[] dmxChannels = new byte[512];
    private float elapsedTime;
    public float speed;
    public Mode mode;
    [Range(0f, 1f)]
    public float RedMaster;
    [Range(0f, 1f)]
    public float GreenMaster;
    [Range(0f, 1f)]
    public float BlueMaster;

    public enum Mode
    {
        DMXChannels,
        RGBPulse,
    }

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

    public float timer;
    private float elpasedTime;
    
    public void Update()
    {
        switch (mode)
        {

            case Mode.DMXChannels:
                _data = dmxChannels;
                break;


            case Mode.RGBPulse:

                _data = new byte[512];
                elapsedTime += (Time.deltaTime * speed);
                int sine = (int)(((Mathf.Sin(elapsedTime) + 1f) / 2f) * 255f);
                if (timer < elpasedTime) elpasedTime = 0;
                int pxl = (int)((elpasedTime / timer) * 169) * 3;
                _data[pxl] = (byte)(sine * RedMaster);
                _data[pxl + 1] = (byte)(sine * GreenMaster);
                _data[pxl + 2] = (byte)(sine * BlueMaster);
                elpasedTime += Time.deltaTime;
                break;
        }
     


    

        //Send Artnet to Direct IP Addresses in settings
        artnetManager.SetArtnetData(0, _data);
        //artnetManager.SetArtnetData(1, _data);
        //artnetManager.SetArtnetData(2, _data);
        //artnetManager.SetArtnetData(3, _data);
    }

}
