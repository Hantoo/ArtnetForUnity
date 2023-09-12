using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ArtnetGeneralSettings_Functions 
{
    public static NICInfo[] GetNetworkInterfaces()
    {
        List<NICInfo> NICList = new List<NICInfo>();
        foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            NICInfo info = new NICInfo();
            if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                info.Name = ni.Name;
                foreach (UnicastIPAddressInformation ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        info.IPString = ip.Address.ToString();
                        info.IPAddress = ip.Address;

                    }
                }
            }
            NICList.Add(info);
        }
        return NICList.ToArray();
    }

    public static void SaveSettings(ArtnetForUnity.ArtnetSettings settings)
    {

        ArtnetForUnity.ArtUtils.SaveSettings(settings);
    }

    public static ArtnetForUnity.ArtnetSettings LoadSettings()
    {

        return ArtnetForUnity.ArtUtils.LoadSettings();

    }

    public static void RefreshNodeList(VisualElement root)
    {
        Debug.Log("Refresh List");
        ListView NodeListView = root.Q<ListView>("NodeList");
        IEnumerable<VisualElement> els = NodeListView.Children();
        foreach (VisualElement item in els)
        {
            NodeListView.Remove(item);
        }
        for(int i = 0; i < ArtnetForUnity.ArtnetManager.deviceList.Count; i++)
        {
            Label nodeLabel = new Label();
            nodeLabel.text = i + " " + ArtnetForUnity.ArtnetManager.deviceList[i].name + " (" + ArtnetForUnity.ArtnetManager.deviceList[i].ipAddress + ")";
            NodeListView.Add(nodeLabel);
        }
        
        
    }

    public struct NICInfo
    {
        public string Name;
        public string IPString;
        public IPAddress IPAddress;
    }




}
