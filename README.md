# ArtnetForUnity

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://paypal.me/joelrlb)

## What is this? - And why another Artnet Lib?
This is a project to implement Art-Net4 into Unity. I have used alot of different artnet libaries for Unity before and have not found one which has been really nice to use and simple to implement.

Currently Implemented Artnet 4 Features (Will be implementing more features of Art-Net4)
| ArtDMX | ArtPoll    | ArtPollReply    |  ArtTrigger     |  ArtSync   |   ArtInput   |  ArtRDM   |  ArtVlc  | ArtCommand | ArtTimecode |
| :---:   | :---: | :---: |:---: |:---: |:---: |:---: |:---: |:---: |:---: |
| ✅ | ✅   | ⏳  | ❌   |  ❌  | ❌ | ❌ | ❌ | ❌ | ❌ |

✅ = Fully Implemented 
⏳ = Partly Implemented 
❌ = Not Implemented Yet

## Usage 
Included in the project is a Tester.cs script which shows you exactly how to use it. It is a very simple setup. 
1) Install the package into unity.
2) Go to Artnet > Artnet General Settings and select the interface you wish to use for Art-Net. This will then be recorded into a Json file which is called at runtime to select the corret interface at start. A Json is also used incase you needed to change the interface running Artnet after it had been built.
3) Implement Artnet Start and On Destory:
### Start
````
    ArtnetForUnity.ArtnetManager artnetManager;
    public void Start()
    {
        artnetManager = new ArtnetForUnity.ArtnetManager();
        artnetManager.Start(1);
    }
````
Initalising the Artnet manager will create the UDP threaded sender and listener. 
The Start method initalises how many Art-Net universes you wish to send. These universes don't have to be in order, for instance, I could initalise 4 universes. Index 0 of my universes could be DMX universe 1, index 1 of my unverses could be Universe 3, index 2 = Uni 12 and index 3 = Uni 14.
In the example above we are initalising with one universe.

### OnDestory
````
    private void OnDestroy()
    {
        artnetManager.Stop();
        artnetManager.Dispose();
    }
````
4)  Set the DMX values for the universe Index.
In the instance below, we have a byte[] array called '_data'. This array holds 512 values, however this can be as short as 2 bytes to 512 bytes.
In the top line, we are sending that byte data directly to 2 IP addresses: 2.0.0.102 and 2.0.0.101.
The bottome line, since we have not specified an IP address, it will broadcast the Art-Net. The broadcast address is calculated based on the IP address and the subnet mask of the selected interface.
We have the index number as 0 since we only initalised 1 universe above, therefore I want to put the Artnet data into the first universe index slot. I then want this data to show on Art-Net Universe 1, which is why we have a value of 1.
```
    //artnetManager.SetArtnetData([(Int) UNIVERSE INDEX], [(Byte[]) UNIVERSE DATA], [(Int) ART-NET UNIVERSE], [(IPAddress[]) IPAddresses] );

    //Send Artnet to Direct IP Addresses 
    artnetManager.SetArtnetData(0, _data, 1, new IPAddress[] { new IPAddress(new byte[] { 2, 0, 0, 102 }), new IPAddress(new byte[] { 2, 0, 0, 101 }) } );

    //Broadcast Artnet 
    artnetManager.SetArtnetData(0, _data, 1);
```

## Packets
### ArtTimecode
ArtTimecode is implemented and allows for you to recieve or send ArtTimecode. 
All timecode interfaces use the timecode namespace, so if you want to interface with it in your project, then ensure you add ``` using ArtnetForUnity.Timecode ```.
#### Receiving 
If Unity dectects an ArtTimecode packet, it will latch onto that. There should only ever be one Timecode Source providing timecode on the network at once, or multiple Art-Net timecodes should be unicast.
To recieve TimeCode, you can either check what the public static ```CurrentTimecode``` variable, within TimecodeManager, is set to every frame - or you can subscribe to the ```TimecodeUpdate``` event by using a method such as the following: 
```
       TimecodeManager.TimecodeUpdate += TimecodeEvent;
       
       public void TimecodeEvent(ArtTimecode e)
       {
           
            tCFrames = e.frames;
            tCHour = e.hours;
            tcSeconds = e.seconds;
            tCMinutes = e.mintues;
        }
```

## Screenshots
<img width="499" alt="Screenshot 2023-09-12 103735" src="https://github.com/Hantoo/ArtnetForUnity/assets/1647342/6c70a732-a2ae-4b1c-8c5b-6a4c783f220b">


Image above shows the UI panel, found under Artnet > General Settings. From here you can choose the NIC that Art-Net uses as well as the Art-Net complient nodes found on the network.
The nodes status update every 4 seconds. 


## External Packages Used
* Newtonsoft Json (com.unity.nuget.newtonsoft-json)
* Editor Coroutines (com.unity.editorcoroutines)
