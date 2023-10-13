using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using ArtnetForUnity.Timecode;
using UnityEditor.UIElements;

namespace ArtnetForUnity.Timecode
{

    public class TimecodeWindow : EditorWindow
    {
        [SerializeField]
        private VisualTreeAsset m_VisualTreeAsset = default;
        private ArtnetForUnity.ArtnetSettings settings;
        private VisualElement root;

        Label labelFrameText;
        Label labelHoursText;
        Label labelSecondsText;
        Label labelMinutesText;
        Label labelTCTypeText;
        private int tCFrames;
        private int tCHour;
        private int tcSeconds;
        private int tCMinutes;
        private string tCType;

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
   
            TimecodeManager.TimecodeUpdate += TimecodeEvent;
            labelFrameText = root.Q<Label>("TC_Frame");
            labelHoursText = root.Q<Label>("TC_Hour");
            labelSecondsText = root.Q<Label>("TC_Sec");
            labelMinutesText = root.Q<Label>("TC_Min");
            labelTCTypeText = root.Q<Label>("TC_FrameRateType");


        }
        
        public void Update()
        {
            labelFrameText.text = tCFrames.ToString("00");
            labelHoursText.text = tCHour.ToString("00");
            labelSecondsText.text = tcSeconds.ToString("00");
            labelMinutesText.text = tCMinutes.ToString("00");
            labelTCTypeText.text = tCType;
        }

        public void TimecodeEvent(ArtTimecode e)
        {
           
            tCFrames = e.frames;
            tCHour = e.hours;
            tcSeconds = e.seconds;
            tCMinutes = e.mintues;

            switch (e.timecodeFPS)
            {
                case TimecodeType.Film_24FPS:
                    tCType = "24 FPS (Film)";
                    break;
                case TimecodeType.DF_29_97FPS:
                    tCType = "29.97 FPS (DF)";
                    break;
                case TimecodeType.EBU_25FPS:
                    tCType = "25 FPS (EBU)";
                    break;
                case TimecodeType.SMPTE_30FPS:
                    tCType = "30 FPS (SMPTE)";
                    break;
            }
            
        }
         
        public void OnDestroy()
        {
            TimecodeManager.TimecodeUpdate -= TimecodeEvent;
        }


    }

}

