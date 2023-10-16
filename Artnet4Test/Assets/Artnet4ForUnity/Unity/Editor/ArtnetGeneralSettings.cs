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
    private bool unsavedChanges;
    [SerializeField]
    VisualTreeAsset m_ItemAsset;


    StyleColor SaveButtonStandardColour;

    public List<ArtnetForUnity.ArtnetDevice> artNetNodesList = new List<ArtnetForUnity.ArtnetDevice>();
    [MenuItem("Artnet/ArtnetGeneralSettings")]
    public static void ShowExample()
    {
        ArtnetGeneralSettings wnd = GetWindow<ArtnetGeneralSettings>();
        wnd.titleContent = new GUIContent("ArtnetGeneralSettings");
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

        Button RefreshListButton = root.Q<Button>("Button_RefreshList");
        // The "makeItem" function will be called as needed
        // when the ListView needs more items to render
        var listItem = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Artnet4ForUnity/Unity/Editor/ArtnetDevices.uxml");

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
