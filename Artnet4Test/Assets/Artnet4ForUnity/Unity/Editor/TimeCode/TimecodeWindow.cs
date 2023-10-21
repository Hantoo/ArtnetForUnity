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
        Label labelTCMode;
        VisualElement veTCMode;
        private int tCFrames;
        private int tCHour;
        private int tcSeconds;
        private int tCMinutes;
        private string tCType;
        private string tcMode;

        private Color RevcTextColor = new Color(1f, 0.41f, 0f, 1f);
        private Color RevcBgColor = new Color(0.78f, 0f, 0f, 1f);
        private Color SendTextColor = new Color(0f, 1f, 0.11f, 1f);
        private Color SendBgColor = new Color(0f, 0.5f, 0.01f, 1f);
        private Color TextColor = new Color(1f, 1f, 1f, 1f);
        private Color BgColor = new Color(0.15f, 0.15f, 0.15f, 1f);

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
                TimecodeManager.playTimecode?.Invoke();
            });
            Button PauseButton = root.Q<Button>("TC_PauseButton");
            PauseButton.RegisterCallback<ClickEvent>(evt =>
            {
                TimecodeManager.pauseTimecode?.Invoke();
            });

            Button StopButton = root.Q<Button>("TC_StopButton");
            StopButton.RegisterCallback<ClickEvent>(evt =>
            {

            });

            Button BackButton = root.Q<Button>("TC_BackButton");
            BackButton.RegisterCallback<ClickEvent>(evt =>
            {
                TimecodeManager.resetTimecode?.Invoke();
            });
   
            TimecodeManager.TimecodeUpdate += TimecodeEvent;
            labelFrameText = root.Q<Label>("TC_Frame");
            labelHoursText = root.Q<Label>("TC_Hour");
            labelSecondsText = root.Q<Label>("TC_Sec");
            labelMinutesText = root.Q<Label>("TC_Min");
            labelTCTypeText = root.Q<Label>("TC_FrameRateType");
            labelTCMode = root.Q<Label>("TC_Mode");
            veTCMode = root.Q<VisualElement>("TCModeBackground");
            tcMode = "No TimeCode Detected";


        }
        
        public void Update()
        {
            labelFrameText.text = tCFrames.ToString("00");
            labelHoursText.text = tCHour.ToString("00");
            labelSecondsText.text = tcSeconds.ToString("00");
            labelMinutesText.text = tCMinutes.ToString("00");
            labelTCTypeText.text = tCType;
            labelTCMode.text = tcMode;
            veTCMode.style.backgroundColor = BgColor;
            labelFrameText.style.color = TextColor;
            labelHoursText.style.color = TextColor;
            labelSecondsText.style.color = TextColor;
            labelMinutesText.style.color = TextColor;
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
 

            //UnityEngine.Debug.Log("e.mode:" + e.mode.ToString());
            switch (e.mode)
            {
                case TimecodeMode.Sender:
                    TextColor = SendTextColor;
                    BgColor = SendBgColor;
                    tcMode = "Sending";
                    break;
                case TimecodeMode.Receiver:
                    TextColor = RevcTextColor;
                    BgColor = RevcBgColor;
                    tcMode = "Recieving";
                    break;
            }
        
            

        }
         
        public void OnDestroy()
        {
            TimecodeManager.TimecodeUpdate -= TimecodeEvent;
        }


    }

}

