using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.EditorCoroutines.Editor;
using System.Collections;


namespace ArtnetForUnity
{
    public class TimecodeWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        private ArtnetForUnity.ArtnetSettings settings;
        private VisualElement root;
       
  

        [MenuItem("Artnet/TimecodeViewer")]
        public static void ShowExample()
        {
            TimecodeWindow wnd = GetWindow<TimecodeWindow>();
            wnd.titleContent = new GUIContent("TimecodeWindow");
            wnd.minSize = new Vector2(740, 240);
            wnd.maxSize = new Vector2(740, 240);
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
            Button PlayButton = root.Q<Button>("TC_PlayButton");
            PlayButton.RegisterCallback<ClickEvent>(evt =>
            {
                
            });
            Button PauseButton = root.Q<Button>("TC_PauseButton");
            PauseButton.RegisterCallback<ClickEvent>(evt =>
            {

            });

            Button StopButton = root.Q<Button>("TC_StopButton");
            StopButton.RegisterCallback<ClickEvent>(evt =>
            {

            });

            Button BackButton = root.Q<Button>("TC_BackButton");
            BackButton.RegisterCallback<ClickEvent>(evt =>
            {

            });


        }

        public void OnDestroy()
        {
          
        }


    }

}

