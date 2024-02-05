using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.EditorCoroutines.Editor;
using System.Collections;

public class ArtnetGeneralSettings : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private ArtnetGeneralSettings_Functions.NICInfo[] nics;
    private VisualElement root;
    private string IPUsage;
    private ArtnetForUnity.ArtnetSettings settings;
    private Color StatusConnected = new Color(0,1,0,1);
    private Color StatusDisconnected = new Color(1, 0.05882353f,0,1);
    private float lastRefreshTime;
    private EditorCoroutine refreshNodeRoutine;
    private ListView NodeListView;
    private ScrollView DMXListView;
    private bool unsavedChanges;
    [SerializeField]
    VisualTreeAsset m_ItemAsset;


    StyleColor SaveButtonStandardColour;

    public List<ArtnetForUnity.ArtnetDevice> artNetNodesList = new List<ArtnetForUnity.ArtnetDevice>();
    public List<ArtnetForUnity.ArtnetOutputs> artNetOutputs = new List<ArtnetForUnity.ArtnetOutputs>();
    [MenuItem("Artnet/ArtnetGeneralSettings")]
    public static void ShowExample()
    {
        ArtnetGeneralSettings wnd = GetWindow<ArtnetGeneralSettings>();
        wnd.titleContent = new GUIContent("Artnet General Settings");
        wnd.minSize = new Vector2(380, 400);
    }

    private IEnumerator TickTock()
    {
        while (true)
        {

            yield return new EditorWaitForSeconds(4f);
          
            artNetNodesList = ArtnetForUnity.ArtnetManager.deviceList;
            NodeListView.Rebuild();
        }
    }

    public void CreateGUI()
    {
        settings = ArtnetGeneralSettings_Functions.LoadSettings();
        artNetOutputs = settings.artnetOutputs;
        // Each editor window contains a root VisualElement object
        root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        //Save Button
        Button saveButton = root.Q<Button>("Button_SaveGeneralSettings");
        saveButton.RegisterCallback<ClickEvent>(evt =>
        {
            ArtnetGeneralSettings_Functions.SaveSettings(settings);
            unSavedChanges(false);
        });
        SaveButtonStandardColour = saveButton.style.backgroundColor;

        Toggle toggleArtSync = root.Q<Toggle>("ToggleArtSync");
        toggleArtSync.value = settings.useArtSync;
        toggleArtSync.RegisterCallback<ClickEvent>(evt =>
        {
            settings.useArtSync = toggleArtSync.value;
            unSavedChanges(true);
        });

        #region DMX Output
        //DMX Output

        DMXListView = root.Q<ScrollView>("DMXOutputList_Scroll");

        //Create DMX outputs for those already in config
      
        Button AddDMX = root.Q<Button>("Button_AddDMXOuput");
        AddDMX.RegisterCallback<ClickEvent>(evt =>
        {
            AddDMXOutput(DMXListView);
            unSavedChanges(true);
        });

        RefreshDMXList();

        #endregion


        #region Node Device

        //Node Device

        Button RefreshListButton = root.Q<Button>("Button_RefreshList");
        // The "makeItem" function will be called as needed
        // when the ListView needs more items to render
        var listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Artnet4ForUnity/Unity/Editor/GeneralSettings/ArtnetDevices.uxml");

        Func<VisualElement> makeItem = () => listItem.Instantiate();

        // As the user scrolls through the list, the ListView object
        // will recycle elements created by the "makeItem"
        // and invoke the "bindItem" callback to associate
        // the element with the matching data item (specified as an index in the list)
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            var label = e.Q<Label>("Label_Name");
            label.text = artNetNodesList[i].name;
            Button button_ip = e.Q<Button>("Label_IP");
            button_ip.text = artNetNodesList[i].ipAddress.ToString();
            var ve_status = e.Q<VisualElement>("VisualEl_ConnectionStatus");
            if (artNetNodesList[i].connected) { ve_status.style.backgroundColor = new StyleColor(StatusConnected); } else { ve_status.style.backgroundColor = new StyleColor(StatusDisconnected); }

            button_ip.RegisterCallback<ClickEvent>(evt =>
            {
                System.Diagnostics.Process.Start("http://"+artNetNodesList[i].ipAddress.ToString());
            });

        };
        NodeListView = root.Q<ListView>("NodeList");
        artNetNodesList = ArtnetForUnity.ArtnetManager.deviceList;
        NodeListView.itemsSource = artNetNodesList;
        NodeListView.bindItem = bindItem;
        NodeListView.makeItem = makeItem;
        NodeListView.Rebuild();
        RefreshListButton.RegisterCallback<ClickEvent>(evt =>
        {
            NodeListView = root.Q<ListView>("NodeList");
            artNetNodesList = ArtnetForUnity.ArtnetManager.deviceList;
            NodeListView.itemsSource = artNetNodesList;
            NodeListView.bindItem = bindItem;
            NodeListView.makeItem = makeItem;
            NodeListView.Rebuild();
            
        });

        

        //Set Ip Address Text
        Label NICIpAddress = root.Q<Label>("Label_IpAddressUsage");
        NICIpAddress.text = "Using " + settings.IPAddress;

        //Radio Button
        RadioButtonGroup NICRadioGroup = root.Q<RadioButtonGroup>("NetworkInterfaceRadioGroup");

        nics = ArtnetGeneralSettings_Functions.GetNetworkInterfaces();
        List<string> NicNames = new List<string>();
        int selectedValue = -1;
        for (int i = 0; i < nics.Length; i++)
        {
            NicNames.Add(nics[i].Name + " (" + nics[i].IPString+")" );
            if(nics[i].IPString == settings.IPAddress) { selectedValue = i; }
        }
        NICRadioGroup.choices = NicNames;
        if (selectedValue >= 0) NICRadioGroup.value = selectedValue;
        NICRadioGroup.RegisterValueChangedCallback(OnNICChange);

        refreshNodeRoutine = EditorCoroutineUtility.StartCoroutine(TickTock(), this);
        #endregion
    }


    /// <summary>
    /// Adds New IP Address
    /// </summary>
    /// <param name="root"></param>
    /// <param name="OutputIndex"></param>
    public void AddArtnetNodeIP(VisualElement root,int OutputIndex, int NodeIPIndex = -1)
    {
        
        var ArtnetDMXIPAddress = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Artnet4ForUnity/Unity/Editor/GeneralSettings/ArtnetDMXIpAddress.uxml");
        var e = ArtnetDMXIPAddress.Instantiate();
        var text_IPAddress = e.Q<TextField>("IPAddressText");
       
        if (NodeIPIndex == -1)
        {

            //Address is yet to be created
            artNetOutputs[OutputIndex].NodeRevcIPAddress.Add(ArtnetForUnity.ArtUtils.broadcastAddress.ToString());
            settings.artnetOutputs = artNetOutputs;
            NodeIPIndex = artNetOutputs[OutputIndex].NodeRevcIPAddress.Count - 1;
            text_IPAddress.value = artNetOutputs[OutputIndex].NodeRevcIPAddress[artNetOutputs[OutputIndex].NodeRevcIPAddress.Count - 1];
        }
        else
        {
            //Address exists
            text_IPAddress.value = artNetOutputs[OutputIndex].NodeRevcIPAddress[NodeIPIndex];
        }

        text_IPAddress.RegisterValueChangedCallback(evt =>
        {
            artNetOutputs[OutputIndex].NodeRevcIPAddress[NodeIPIndex] = text_IPAddress.value;
            unSavedChanges(true); ;
        });

        var btn_deleteIP = e.Q<Button>("RemoveIPAddressButton");
        btn_deleteIP.RegisterCallback<ClickEvent>(evt =>
        {
            RemoveArtnetNodeIP(root, OutputIndex, NodeIPIndex);
            unSavedChanges(true);
        });
       
        root.Add(e);
    }

  
    public void RefreshArtnetNodeIP(int OutputIndex)
    {
       
            try
            {
                DMXListView.ElementAt(OutputIndex).Q<VisualElement>("G_IpAddressesVE").Clear();
            }
            catch (Exception e) { Debug.LogError(e.Message); };
        try
        {
            for (int i = 0; i < settings.artnetOutputs[OutputIndex].NodeRevcIPAddress.Count; i++)
            {
                AddArtnetNodeIP(DMXListView.ElementAt(OutputIndex).Q<VisualElement>("G_IpAddressesVE"), OutputIndex, i);
            }
        }
        catch (Exception e) { Debug.LogError(e.Message); };

    }

    public void RemoveArtnetNodeIP(VisualElement root, int OutputIndex, int IPindex)
    {
       
        artNetOutputs[OutputIndex].NodeRevcIPAddress.RemoveAt(IPindex);
        root.RemoveAt(OutputIndex);
        settings.artnetOutputs = artNetOutputs;
        RefreshArtnetNodeIP(OutputIndex);
     
    }



    /// <summary>
    /// Add to Artnet Outputs as new
    /// </summary>
    /// <param name="root"></param>
    public void AddDMXOutput(VisualElement root)
    {
        unSavedChanges(true);
        //Add to Artnet Outputs as new
        ArtnetForUnity.ArtnetOutputs output = new ArtnetForUnity.ArtnetOutputs();
        output.NodeRevcIPAddress.Add(ArtnetForUnity.ArtUtils.broadcastAddress.ToString());
        artNetOutputs.Add(output);
        settings.artnetOutputs = artNetOutputs;

        CreateDMXOutputList(root, settings.artnetOutputs.Count -1);

    }

    /// <summary>
    /// Add to Artnet Outputs from already existing setting
    /// </summary>
    /// <param name="root"></param>
    /// <param name="i"></param>
    public void AddDMXOutput(VisualElement root, int i)
    {
        //Add to Artnet Outputs from already existing setting
        //ArtnetForUnity.ArtnetOutputs output = settings.artnetOutputs[i];
        CreateDMXOutputList(root, i);

    }

    public void RefreshDMXList()
    {
        DMXListView.Clear();
        for (int i = 0; i < settings.artnetOutputs.Count; i++)
        {
            AddDMXOutput(DMXListView, i);

        }

        for (int i = 0; i < settings.artnetOutputs.Count; i++)
        {
            RefreshArtnetNodeIP(i);

        }
    }

    public void CreateDMXOutputList(VisualElement root, int i)
{
        var ArtnetDMXItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Artnet4ForUnity/Unity/Editor/GeneralSettings/ArtnetDMX.uxml");
        
        var e = ArtnetDMXItem.Instantiate();

        #region Base Ui

        var TF_UnityUni = e.Q<TextField>("UnityUniText");
        var TF_ArtnetUni = e.Q<TextField>("ArtnetUniText");
        var TF_Net = e.Q<TextField>("NetText");
        var TF_Subnet = e.Q<TextField>("SubnetText");
        var TF_Uni = e.Q<TextField>("UniText");
        //L_DMXListNum.text = i.ToString();
        TF_UnityUni.value = i.ToString();//i.ToString(); 
        TF_ArtnetUni.value = artNetOutputs[i].Universe.ToString();
        TF_Net.value = artNetOutputs[i].Net.ToString(); ;
        TF_Subnet.value = artNetOutputs[i].Subnet.ToString();
        TF_Uni.value = artNetOutputs[i].Universe.ToString();
       // var Btn_AddAddress = e.Q<TextField>("Btn_IPAddressAdd");
        var G_IpAddresses = e.Q<ListView>("G_IpAddresses");

        Button DeleteButton = e.Q<Button>("Btn_Remove");
        DeleteButton.RegisterCallback<ClickEvent>(evt =>
        {
            settings.artnetOutputs.RemoveAt(i);
            DMXListView.RemoveAt(i);
            RefreshDMXList();

        });

        Button AddIPButton = e.Q<Button>("Btn_IPAddressAdd");
        AddIPButton.RegisterCallback<ClickEvent>(evt =>
        {
      
            AddArtnetNodeIP(DMXListView.ElementAt(i),i);
            unSavedChanges(true);

        });
        #endregion
        root.Add(e);

   
}
    



    public void OnDestroy()
    {
        EditorCoroutineUtility.StopCoroutine(refreshNodeRoutine);
    }

    private void OnNICChange(ChangeEvent<int> evt)
    {
        RadioButtonGroup NICRadioGroup = root.Q<RadioButtonGroup>("NetworkInterfaceRadioGroup");
        Label NICIpAddress = root.Q<Label>("Label_IpAddressUsage");
        settings.IPAddress = nics[NICRadioGroup.value].IPString;
        settings.InterfaceName = nics[NICRadioGroup.value].Name;
        NICIpAddress.text = "Using "+ settings.IPAddress;
        unSavedChanges(true);
    }

    private void unSavedChanges(bool AreUnsavedChanges)
    {
        Button saveBtn = root.Q<Button>("Button_SaveGeneralSettings");
        if (AreUnsavedChanges)
        {
            StyleColor col = new StyleColor(new Color(1.0f,0.57f,0f));
            saveBtn.style.backgroundColor = col;
        }
        else
        {
            saveBtn.style.backgroundColor = SaveButtonStandardColour;
        }
    }
}
