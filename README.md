# ArtnetForUnity

[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://paypal.me/joelrlb)

## What is this? - And why another Artnet Lib?
This is a project to implement Art-Net4 into Unity. I have used alot of different artnet libaries for Unity before and have not found one which has been really nice to use and simple to implement.


Currently Implemented Artnet 4 Features (Will be implementing more features of Art-Net4)

| ArtDMX | ArtPoll    | ArtPollReply   |  ArtSync   |  ArtTimecode     |  ArtIpProg   |   ArtInput   |  ArtRDM   |  ArtVlc  | ArtCommand | ArtTrigger |
| :---:   | :---: | :---: | :---: |:---: |:---: |:---: |:---: |:---: |:---: |:---: |
| ✅ | ✅   | ✅  |  ✅  |  ✅  |  ❌  | ❌ | ⏰ | ❌ | ❌ | ❌  |


✅ = Fully Implemented 
⏳ = Partly Implemented 
⏰ = SCheduled To Be Implemented 
❌ = Not Implemented Yet

## Usage 
Included in the project is a Tester.cs script which shows you exactly how to use it. It is a very simple setup. 
1) Install the ````com.unity.editorcoroutines```` and ````com.unity.nuget.newtonsoft-json```` packages via Package Manager > Add Via Git URL.
2) Import the package into unity.
3) Go to Artnet > Artnet General Settings and select the interface you wish to use for Art-Net. This will then be recorded into a Json file which is called at runtime to select the corret interface at start. A Json is also used incase you needed to change the interface running Artnet after it had been built.
4) Implement Artnet Start and On Destory:
### Start
````
    ArtnetForUnity.ArtnetManager artnetManager;
    public void Start()
    {
        artnetManager = new ArtnetForUnity.ArtnetManager();
        artnetManager.Start();
    }
````
Initalising the Artnet manager will create the UDP threaded sender and listener. 
The Start method initalises the Art-Net universes you've set to send via General Settings. The amount of universes are dictated by how many DMX Ouputs you've added in the General Settigns Window.

![image](https://github.com/Hantoo/ArtnetForUnity/assets/1647342/be16a17e-7706-49cc-a5d0-141eeb5c9c1b)


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
In the top line, we are sending that byte data directly to 1 IP addresses: 2.0.0.255.
We have the index number as 0 since we only initalised 1 universe above, therefore I want to put the Artnet data into the first universe index slot. 
```
    //artnetManager.SetArtnetData([(Int) Unity UNIVERSE INDEX], [(Byte[]) UNIVERSE DATA]);

    //Broadcast Artnet 
    artnetManager.SetArtnetData(0, _data);
```

## Packets
### ArtTimecode
ArtTimecode is implemented and allows for you to recieve or send ArtTimecode. 
All timecode interfaces use the timecode namespace, so if you want to interface with it in your project, then ensure you add ``` using ArtnetForUnity.Timecode ```.
#### Receiving 
If Unity dectects an ArtTimecode packet, it will latch onto that. There should only ever be one Timecode Source providing timecode on the network at once, or multiple Art-Net timecodes should be unicast.
To recieve TimeCode, you should check the public static ```CurrentTimecode``` variable, within TimecodeManager, is set to every frame - or you can subscribe to the ```TimecodeUpdate``` event by using a method such as the following: 
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

#### Sending
To send ArtTimecode you should set the timecode to be sent via the ```.SetTimeCode(hour, minute, second, frame);``` found in the timecodeManager. 
Once set, you play the Timecode by invoking the ``` TimecodeManager.playTimecode?.Invoke();```, pause the Timecode by invoking ```TimecodeManager.pauseTimecode?.Invoke();``` or reset the timecode back to the most recent SetTimecodev value by invoking ```TimecodeManager.resetTimecode?.Invoke();```.
If you're sending timecode from Unity, To stop the timecode receiver from reading the timecode packets sent - any timecode packets received from the same IP address as the selected Art-Net Interface for Unity will be ignored.

## Screenshots

![image](https://github.com/Hantoo/ArtnetForUnity/assets/1647342/ba96764c-1275-490f-9af2-d032c6c1b793)

Image above shows the UI panel, found under Artnet > General Settings. From here you can choose the NIC that Art-Net uses as well as the Art-Net complient nodes found on the network.
The nodes status update every 4 seconds. 
    

<img width="558" alt="Receiving Timecode" src="https://github.com/Hantoo/ArtnetForUnity/assets/1647342/ac560665-c629-4d05-acb7-7d25d842a4c5">    
     
<img width="558" alt="Sending Timecode" src="https://github.com/Hantoo/ArtnetForUnity/assets/1647342/e0f1f9c3-fd79-427b-ade6-66ce9bf83eba">


Image above shows the UI panel, found under Artnet > TimecodeViewer. You can see the incoming timecode and the framerate that the timecode has been set too.
If the nubers are red then you are receiving timecode from another device. If the numbers are green then you are sending timecode onto the network.

## External Packages Used
* Newtonsoft Json (com.unity.nuget.newtonsoft-json)
* Editor Coroutines (com.unity.editorcoroutines)

## TroubleShooting

### I can't see my ArtNet device in Unity
If you can't see the artnet device in unity or within the Artnet General Settings window, but you know it's definately on the correct network then check your FireWall rules for inbound connection.
By default, the unity editor is normally blocked on public connections. Allowing the connection should fix this.
![image](https://github.com/Hantoo/ArtnetForUnity/assets/1647342/4d3b042f-587a-41d6-8cad-5720839efeae)
![image](https://github.com/Hantoo/ArtnetForUnity/assets/1647342/4bce5c4f-507b-45a3-a07c-935d453d50f4)
