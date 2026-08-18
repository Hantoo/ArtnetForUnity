using System;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArtnetForUnity
{
    /// <summary>
    /// Editor window that displays a 512 element NativeArray of floats as a DMX channel table.
    /// Cell backgrounds fade from black (0f) to orange (1f).
    /// Open via Artnet > DMX Universe Viewer to watch a live input universe, or call
    /// DMXUniverseViewer.ShowWindow(myNativeArray) to display your own array.
    /// </summary>
    public class DMXUniverseViewer : EditorWindow
    {
        private const int ChannelCount = 512;
        private static readonly Color CellColourOff = Color.black;
        private static readonly Color CellColourOn = new Color(1f, 0.5f, 0f); //Orange
        private static readonly Color TextColourDim = new Color(0.85f, 0.85f, 0.85f);
        private static readonly Color TextColourBright = Color.black;

        //External array mode (set via ShowWindow(values) / SetData)
        private NativeArray<float> externalData;
        private bool useExternalData;

        //Live Artnet input mode
        private int universe;
        private bool autoEnableUniverse = true;
        private int enabledUniverse = -1;

        private VisualElement[] cells = new VisualElement[ChannelCount];
        private Label[] valueLabels = new Label[ChannelCount];
        private Label statusLabel;

        [MenuItem("Artnet/DMX Universe Viewer")]
        public static void ShowWindow()
        {
            DMXUniverseViewer wnd = GetWindow<DMXUniverseViewer>();
            wnd.titleContent = new GUIContent("DMX Universe Viewer");
            wnd.minSize = new Vector2(420, 300);
        }

        /// <summary>
        /// Opens the viewer displaying the given array (expects 512 elements).
        /// The array is read every refresh - do not Dispose it while the window is open.
        /// </summary>
        public static DMXUniverseViewer ShowWindow(NativeArray<float> values)
        {
            ShowWindow();
            DMXUniverseViewer wnd = GetWindow<DMXUniverseViewer>();
            wnd.SetData(values);
            return wnd;
        }

        /// <summary>
        /// Displays the given array (expects 512 elements) instead of a live input universe.
        /// </summary>
        public void SetData(NativeArray<float> values)
        {
            externalData = values;
            useExternalData = true;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            //Toolbar
            VisualElement toolbar = new VisualElement();
            toolbar.style.flexDirection = FlexDirection.Row;
            toolbar.style.alignItems = Align.Center;
            toolbar.style.paddingTop = 4;
            toolbar.style.paddingBottom = 4;
            toolbar.style.paddingLeft = 4;

            IntegerField universeField = new IntegerField("Universe");
            universeField.value = universe;
            universeField.style.width = 160;
            universeField.RegisterValueChangedCallback(evt =>
            {
                universe = Mathf.Clamp(evt.newValue, 0, 32767);
                useExternalData = false; //Typing a universe switches back to live input mode
            });
            toolbar.Add(universeField);

            Toggle autoEnableToggle = new Toggle("Auto Enable");
            autoEnableToggle.tooltip = "Enables the selected universe on the Artnet Input Manager while this window is open.";
            autoEnableToggle.value = autoEnableUniverse;
            autoEnableToggle.RegisterValueChangedCallback(evt => autoEnableUniverse = evt.newValue);
            toolbar.Add(autoEnableToggle);

            statusLabel = new Label("");
            statusLabel.style.marginLeft = 8;
            statusLabel.style.color = TextColourDim;
            toolbar.Add(statusLabel);

            root.Add(toolbar);

            //Cell grid
            ScrollView scroll = new ScrollView();
            scroll.style.flexGrow = 1;
            VisualElement grid = new VisualElement();
            grid.style.flexDirection = FlexDirection.Row;
            grid.style.flexWrap = Wrap.Wrap;
            grid.style.paddingLeft = 4;
            grid.style.paddingRight = 4;

            for (int i = 0; i < ChannelCount; i++)
            {
                VisualElement cell = new VisualElement();
                cell.style.width = 38;
                cell.style.height = 30;
                cell.style.marginLeft = 1;
                cell.style.marginRight = 1;
                cell.style.marginTop = 1;
                cell.style.marginBottom = 1;
                cell.style.backgroundColor = CellColourOff;
                cell.style.alignItems = Align.Center;
                cell.style.justifyContent = Justify.Center;

                Label channelLabel = new Label((i + 1).ToString());
                channelLabel.style.fontSize = 8;
                channelLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                channelLabel.style.marginBottom = 0;
                channelLabel.style.paddingBottom = 0;
                cell.Add(channelLabel);

                Label valueLabel = new Label("0.00");
                valueLabel.style.fontSize = 10;
                valueLabel.style.color = TextColourDim;
                valueLabel.style.marginTop = 0;
                valueLabel.style.paddingTop = 0;
                cell.Add(valueLabel);

                cells[i] = cell;
                valueLabels[i] = valueLabel;
                grid.Add(cell);
            }

            scroll.Add(grid);
            root.Add(scroll);

            //Refresh the table roughly 20 times per second
            root.schedule.Execute(Refresh).Every(50);
        }

        private void Refresh()
        {
            NativeArray<float> data = default;

            if (useExternalData)
            {
                data = externalData;
                statusLabel.text = "Viewing external array";
            }
            else
            {
                ArtnetManager manager = ArtnetManager.Instance;
                if (manager == null || manager.inputManager == null)
                {
                    statusLabel.text = "No ArtnetManager running";
                    PaintEmpty();
                    return;
                }

                //Keep the selected universe enabled while the window is open
                if (autoEnableUniverse && enabledUniverse != universe)
                {
                    if (enabledUniverse != -1) manager.inputManager.DisableUniverse(enabledUniverse);
                    manager.inputManager.EnableUniverse(universe);
                    enabledUniverse = universe;
                }

                if (!manager.inputManager.IsUniverseEnabled(universe))
                {
                    statusLabel.text = "Universe " + universe + " not enabled";
                    PaintEmpty();
                    return;
                }

                data = manager.inputManager.GetUniverseData(universe);
                statusLabel.text = "Universe " + universe;
            }

            if (!data.IsCreated || data.Length < ChannelCount)
            {
                statusLabel.text += " - waiting for data";
                PaintEmpty();
                return;
            }

            try
            {
                for (int i = 0; i < ChannelCount; i++)
                {
                    float value = Mathf.Clamp01(data[i]);
                    cells[i].style.backgroundColor = Color.Lerp(CellColourOff, CellColourOn, value);
                    valueLabels[i].text = value.ToString("0.00");
                    valueLabels[i].style.color = value > 0.55f ? TextColourBright : TextColourDim;
                }
            }
            catch (Exception)
            {
                //External array was most likely disposed while the window was open
                useExternalData = false;
                externalData = default;
                statusLabel.text = "Array disposed - switching to live input";
                PaintEmpty();
            }
        }

        private void PaintEmpty()
        {
            for (int i = 0; i < ChannelCount; i++)
            {
                cells[i].style.backgroundColor = CellColourOff;
                valueLabels[i].text = "-";
                valueLabels[i].style.color = TextColourDim;
            }
        }

        private void OnDisable()
        {
            if (enabledUniverse != -1 && ArtnetManager.Instance != null && ArtnetManager.Instance.inputManager != null)
            {
                ArtnetManager.Instance.inputManager.DisableUniverse(enabledUniverse);
            }
            enabledUniverse = -1;
        }
    }
}
