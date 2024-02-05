using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using Unity.EditorCoroutines.Editor;

public class ArtnetDiagnostics : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private Label label_ArtnetQueueFrameratems;
    private Label label_ArtnetQueueFramerate;
    private Label label_ArtnetSenderFrameratems;
    private Label label_ArtnetSenderFramerate;

    [MenuItem("Artnet/ArtnetDiagnostics")]
    public static void ShowExample()
    {
        ArtnetDiagnostics wnd = GetWindow<ArtnetDiagnostics>();
        wnd.titleContent = new GUIContent("ArtnetDiagnostics");
    }
    EditorCoroutine refreshEditor;
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        //VisualElement label = new Label("Hello World! From C#");
        //root.Add(label);

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);

        label_ArtnetQueueFramerate = root.Q<Label>("Label_SenderAddToQueueFrameRate");
        label_ArtnetQueueFrameratems = root.Q<Label>("Label_SenderAddToQueueFrameRate_ms");
        label_ArtnetSenderFramerate = root.Q<Label>("Label_SenderFrameRate");
        label_ArtnetSenderFrameratems = root.Q<Label>("Label_SenderFrameRate_ms");
        refreshEditor = EditorCoroutineUtility.StartCoroutine(TickTock(), this);
    }




    private IEnumerator TickTock()
    {
        while (true)
        {
            UpdateFramerates();
            yield return new EditorWaitForSeconds(0.2f);
            //root.Rebuild();
        }
    }

    private void UpdateFramerates()
    {
        label_ArtnetQueueFrameratems.text = ArtnetForUnity.ArtUtils.Diagnostic_DMXPacketQueueFrameRate.ToString("#.#");
        label_ArtnetQueueFramerate.text = (1000/ArtnetForUnity.ArtUtils.Diagnostic_DMXPacketQueueFrameRate).ToString("#.#");
        label_ArtnetSenderFrameratems.text = ArtnetForUnity.ArtUtils.Diagnostic_SenderFrameRate.ToString("#.#");
        label_ArtnetSenderFramerate.text = (1000/ArtnetForUnity.ArtUtils.Diagnostic_SenderFrameRate).ToString("#.#");
    }

    private void OnDestroy()
    {
        EditorCoroutineUtility.StopCoroutine(refreshEditor);
    }
}
