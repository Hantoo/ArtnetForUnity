using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using ArtnetForUnity.RDM;

public class RdmViewer : EditorWindow
{

    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    private ArtnetForUnity.ArtnetSettings settings;
    private VisualElement root;

    [MenuItem("Artnet/RdmViewer")]
    public static void ShowExample()
    {
        RdmViewer wnd = GetWindow<RdmViewer>();
        wnd.titleContent = new GUIContent("RdmViewer");
        wnd.minSize = new Vector2(740, 240);
        //wnd.maxSize = new Vector2(740, 240);
    }

    public void CreateGUI()
    {
        settings = ArtnetGeneralSettings_Functions.LoadSettings();
        // Each editor window contains a root VisualElement object

        root = rootVisualElement;

        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        Button RefreshRDMButton = root.Q<Button>("RefreshRDM");
        RefreshRDMButton.RegisterCallback<ClickEvent>(evt =>
        {
            RdmManager.refreshRDM?.Invoke();
        });

    }
}
