using Unity.Collections;
using UnityEngine;
using ArtnetForUnity;

/// <summary>
/// Example showing how to read incoming Art-Net DMX data.
/// Add an ArtnetDMXInput component to a GameObject, assign it here and watch the values in the inspector.
/// </summary>
public class InputTester : MonoBehaviour
{
    public ArtnetDMXInput dmxInput;
    [Range(1, 512)]
    public int channel = 1;
    public float channelValue;
    public Light targetLight; //Optional - intensity driven by the selected channel

    public void Update()
    {
        //Read a single channel (1 - 512)
        channelValue = dmxInput.GetChannel(channel);
        if (targetLight != null) targetLight.intensity = channelValue;

        //Or read the whole universe as a NativeArray<float>
        NativeArray<float> universeData = dmxInput.DMXData;
        if (universeData.IsCreated)
        {
            //universeData[0] is channel 1, universeData[511] is channel 512
        }
    }
}
