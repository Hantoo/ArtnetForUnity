using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System;
using System.Net.Sockets;

public class Tester : MonoBehaviour
{
    IPAddress IP_ArtnetNode1;

    private byte[] _data;
    private float elapsedTime;
    public float speed;

    ArtnetForUnity.ArtnetManager artnetManager;
    public void Start()
    {
        _data = new byte[512];
        //client = new UdpClient();
        artnetManager = new ArtnetForUnity.ArtnetManager();
        artnetManager.Start(1);

        IP_ArtnetNode1 = new IPAddress(new byte[] { 2, 161, 30, 77 });
    }

    private void OnDestroy()
    {
        artnetManager.Stop();
        artnetManager.Dispose();
    }


    public void Update()
    {
        elapsedTime += (Time.deltaTime * speed);
        int sine = (int)(((Mathf.Sin(elapsedTime) +1f)/2f)*255f);
        for(int i = 0; i < _data.Length; i++) {
            _data[i] = (byte)sine;
        }

        //Send Artnet to Direct IP Addresses 
        //artnetManager.SetArtnetData(0, _data, 1, new IPAddress[] { new IPAddress(new byte[] { 2, 0, 0, 102 }), new IPAddress(new byte[] { 2, 0, 0, 101 }) } );

        //Broadcast Artnet 
        artnetManager.SetArtnetData(0, _data, 1);
    }

}
