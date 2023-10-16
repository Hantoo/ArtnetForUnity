using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;

namespace ArtnetForUnity
{
    public static class ArtUtils
    {
        public static int ArtnetProtocolRevisionNumber = 14;
        public static int ArtnetPort = 6454;
        public static IPAddress InterfaceIPAddress = new IPAddress(new byte[] { 2, 0, 0, 255 }); //Updates on load of settings;
        public static IPAddress broadcastAddress = new IPAddress(new byte[] { 2, 255, 255, 255 }); //Updates on load of settings;
        public static PhysicalAddress InterfaceMacAddress;
        public static int ControllerFirmwareNumber = 1;
        public static int OemCode = 0x0001;
        public static int ESTACode = 32767;
        public static NetworkInterface[] NetworkInterfaces;
        public static NetworkInterface SelectedInterface;
        public static OpCodes GetOpCodeFromByte(byte OpCodeByte)
        {
            OpCodes code = (OpCodes)OpCodeByte;
            return code;
        }

        public static NodeReport GetNodeReportFromByte(byte NodeReportByte)
        {
            NodeReport code = (NodeReport)NodeReportByte;
            return code;
        }

        public static string ReturnNodeReportErrorMessage(NodeReport report)
        {
            switch (report)
            {
                case NodeReport.RcDebug:
                    return "Booted in debug mode (Only used in development)";
                    
                case NodeReport.RcPowerOk:
                    return "Power On Tests successful";
                    
                case NodeReport.RcPowerFail:
                    return "Hardware tests failed at Power On";
                    
                case NodeReport.RcSocketWr1:
                    return "Last UDP from Node failed due to truncated length, Most likely caused by a collision.";
                    
                case NodeReport.RcParseFail:
                    return "Unable to identify last UDP transmission. Check OpCode and packet length.";
                    
                case NodeReport.RcUdpFail:
                    return "Unable to open Udp Socket in last transmission attempt";
                    
                case NodeReport.RcShNameOkay:
                    return "Confirms that Port Name programming via ArtAddress, was successful.";
                    
                case NodeReport.RcLoNameOkay:
                    return "Confirms that Long Name programming via ArtAddress, was successful";
                    
                case NodeReport.RcDmxError:
                    return "DMX512 receive errors detected.";
                    
                case NodeReport.RcDmxUdpFull:
                    return "Ran out of internal DMX transmit buffers.";
                    
                case NodeReport.RcDmxRxFull:
                    return "Ran out of internal DMX Rx buffers.";
                    
                case NodeReport.RcSwitchErr:
                    return "Rx Universe switches conflict";
                    
                case NodeReport.RcConfigErr:
                    return "Product configuration does not match firmware";
                    
                case NodeReport.RcDmxShort:
                    return "DMX output short detected. See GoodOutput field.";
                    
                case NodeReport.RcFirmwareFail:
                    return "Last attempt to upload new firmware failed";
                    
                case NodeReport.RcUserFail:
                    return "User changed switch settings when address locked by remote programming. User changes ignored.";
                    
                case NodeReport.RcFactoryRes:
                    return "Factory reset has occurred."; 
                    
                default:
                    return "Unknown Command";
             
            }
        }

        public static byte[] GetOpCodeLowHighBytes(OpCodes opCode)
        {
            byte upper = (byte)((ushort)opCode >> 8);
            byte lower = (byte)((ushort)opCode & 0xff);
            return new byte[] { lower, upper };
        }

        public static byte[] GetLowHighFromInt(int interger)
        {
            byte upper = (byte)((ushort)interger >> 8);
            byte lower = (byte)((ushort)interger & 0xff);
            return new byte[] { lower, upper };
        }

        public static OpCodes ByteToOpCode(byte hi, byte lo)
        {
            int number = hi | lo << 8;
            return (OpCodes)number;
        }

        public static ArtnetSettings LoadSettings()
        {
            ArtnetSettings loadedSettings = new ArtnetSettings();
            string fileLocation = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string folderLocation = fileLocation.Substring(0, fileLocation.LastIndexOf('\\')) + "\\";
            string DataLocation = folderLocation + "ArtnetSettings.json";
            if (!File.Exists(DataLocation)) return loadedSettings;
            using (StreamReader sr = File.OpenText(DataLocation))
            {
                string output = sr.ReadToEnd();
                loadedSettings = JsonConvert.DeserializeObject<ArtnetSettings>(output);
            }
            System.Net.IPAddress ipaddress = System.Net.IPAddress.Parse(loadedSettings.IPAddress);
            //Debug.Log("ipaddress: " + ipaddress.ToString());
            InterfaceIPAddress = checkNICExists(ipaddress);
            //Debug.Log("InterfaceIPAddress: " + InterfaceIPAddress.ToString()) ;
            System.Net.IPAddress subnetMask = getSubnetMask(ipaddress);
            broadcastAddress = GetBroadcastAddress(ipaddress, subnetMask);
            try
            {
                getMACAddress(ipaddress);
            }
            catch { Debug.LogError("No MAC Found"); };


            return loadedSettings;
        }

        public static bool GetInterface(IPAddress ipaddress, out NetworkInterface NIcinterface)
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterfaces = Interfaces;
            IPAddress ip = ipaddress;
            bool exists = false;
            NIcinterface = null;
            foreach (NetworkInterface interf in Interfaces)
            {

                UnicastIPAddressInformationCollection UnicastIPInfoCol = interf.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol)
                {
                    if (UnicatIPInfo.Address.Equals(ip))
                    {
                        exists = true;
                        NIcinterface = interf;
                    }
                }

            }
            if (exists) { return true; }
            else
            {
               
                return false;
            }
        }

        public static IPAddress checkNICExists(IPAddress ipaddress)
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterfaces = Interfaces;
            IPAddress ip = ipaddress;
            bool exists = false;
            
            foreach (NetworkInterface interf in Interfaces)
            {

                UnicastIPAddressInformationCollection UnicastIPInfoCol = interf.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol)
                {
                    if (UnicatIPInfo.Address.Equals(ip))
                        exists = true;
                }

            }
            if (exists) { return ip; } else
            {
                Debug.LogError("Network Interface Changed");
                
                Debug.LogError("Changing Art-Net Interface to use IP 127.0.0.1" );
                return new IPAddress(new byte[] { 127,0,0,1 });
            }
        }

        public static bool GetDhcp()
        {
            if (SelectedInterface == null) return false;
            if (SelectedInterface.GetIPProperties().GetIPv4Properties() != null)
            {
                return SelectedInterface.GetIPProperties().GetIPv4Properties().IsDhcpEnabled;
            }
            else
            {
                return false;
            }
        }
        public static void SaveSettings(ArtnetSettings settings)
        {
            //ArtnetGeneralSettings wnd = GetWindow<ArtnetGeneralSettings>();
            //Find IP In Interfaces
            string fileLocation = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            string folderLocation = fileLocation.Substring(0, fileLocation.LastIndexOf('\\')) + "\\";
            string DataLocation = folderLocation + "ArtnetSettings.json";
            string output = JsonConvert.SerializeObject(settings);
            FileStream fcreate = File.Open(DataLocation, FileMode.Create);

            using (StreamWriter sw = new StreamWriter(fcreate))
            {
                sw.Write(output);
            }
            fcreate.Close();
        }

        private static IPAddress GetBroadcastAddress(IPAddress ipAddress, IPAddress subnetMask)
        {
            //determines a broadcast address from an ip and subnet
           

            byte[] ipAdressBytes = ipAddress.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPAddress(broadcastAddress);
        }

        private static IPAddress getSubnetMask(IPAddress iP)
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress ip = iP;
            foreach (NetworkInterface interf in Interfaces)
            {

                UnicastIPAddressInformationCollection UnicastIPInfoCol = interf.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol)
                {
                    if (UnicatIPInfo.Address.Equals(ip))
                        return UnicatIPInfo.IPv4Mask;
                }

            }

            return new IPAddress(new byte[] { 255, 0, 0, 0 });
            //throw new ArgumentException("Can't Find NIC with specified IP for Artnet.");
            ///return null;

        }

        private static PhysicalAddress getMACAddress(IPAddress iP)
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            IPAddress ip = iP;
            foreach (NetworkInterface interf in Interfaces)
            {

                UnicastIPAddressInformationCollection UnicastIPInfoCol = interf.GetIPProperties().UnicastAddresses;

                foreach (UnicastIPAddressInformation UnicatIPInfo in UnicastIPInfoCol)
                {
                    if (UnicatIPInfo.Address.Equals(ip))
                    {
                        InterfaceMacAddress = interf.GetPhysicalAddress();
                        return InterfaceMacAddress;

                    }
                }

            }

            //("Can't Find NIC with specified IP for Artnet.");
            return new PhysicalAddress(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00});
            ///return null;

        }

        public static bool IsBitSet(byte b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }
    }

    public enum OpCodes
    {
        OpPoll =      0x2000,
        OpPollReply = 0x2100,
        OpDiagData =  0x2300,
        OpCommand =   0x2400,
        OpDataRequest=0x2700,
        OpOutput     =0x5000,
        OpDmx        =0x5000,
        OpNzs        =0x5100,
        OpSync       =0x5200,
        OpAddress    =0x6000,
        OpInput      =0x7000,
        OpTodRequest =0x8000,
        OpTodData    =0x8100,
        OpTodControl =0x8200,
        OpRdm        =0x8300,
        OpRdmSub     =0x8400,
        OpVideoSetup =0xa010,
        OpVideoPalette=0xa020,
        OpVideoData  =0xa040,
        OpMacMaster  =0xf000,
        OpMacSlave   =0xf100,
        OpFirmwareMaster=0xf200,
        OpFirmwareReply=0xf300,
        OpFileTnMaster=0xf400,
        OpFileFnMaster=0xf500,
        OpFileFnReply=0xf600,
        OpIpProg     =0xf800,
        OpIpProgReply=0xf900,
        OpMedia      =0x9000,
        OpMediaPatch =0x9100,
        OpMediaControl=0x9200,
        OpMediaContrlReply=0x9300,
        OpTimeCode   =0x9700,
        OpTimeSync   =0x9800,
        OpTrigger    =0x9900,
        OpDirectory  =0x9a00,
        OpDirectoryReply=0x9b00
    }

    public enum NodeReport
    {
        RcDebug = 0x0000,
        RcPowerOk = 0x0001,
        RcPowerFail = 0x0002,
        RcSocketWr1 = 0x0003,
        RcParseFail = 0x0004,
        RcUdpFail = 0x0005,
        RcShNameOkay = 0x0006,
        RcLoNameOkay = 0x0007,
        RcDmxError = 0x0008,
        RcDmxUdpFull = 0x0009,
        RcDmxRxFull = 0x000a,
        RcSwitchErr = 0x000b,
        RcConfigErr = 0x000c,
        RcDmxShort = 0x000d,
        RcFirmwareFail = 0x000e,
        RcUserFail = 0x000f,
        RcFactoryRes = 0x0010

    }

    public enum StyleCode
    {
        StNode = 0x00, //A Dmx To / From Artnet Device
        StController = 0x01, //Lighting Console
        StMedia = 0x02, //Media server 
        StRouter = 0x03, //Network Routing
        StBackup = 0x04, //Backup Device
        StConfig = 0x05, //Config or diagnostic tool
        StVisual = 0x06 //Visualiser
    }


    public enum PriorityCodes
    {
        DpLow = 0x10,
        DpMed = 0x40,
        DpHigh = 0x80,
        DpCritical = 0xe0,
        DpVolatile = 0xf0
    }



    public struct ArtnetSettings
    {
        public string IPAddress;
        public string InterfaceName;
        public bool useArtSync;
    }

}