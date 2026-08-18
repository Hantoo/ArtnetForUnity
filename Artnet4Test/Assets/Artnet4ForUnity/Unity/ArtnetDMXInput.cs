using Unity.Collections;
using UnityEngine;

namespace ArtnetForUnity
{
    /// <summary>
    /// Add this component to a GameObject to receive a DMX universe from Art-Net.
    /// Set the Universe to the Art-Net Port Address you want to listen to, then read
    /// DMXData (NativeArray of 512 floats, 0f - 1f) or GetChannel() from your scripts.
    /// </summary>
    [AddComponentMenu("Artnet/Artnet DMX Input")]
    public class ArtnetDMXInput : MonoBehaviour
    {
        [Tooltip("The Art-Net Port Address (Universe) to receive. This should match the universe number the sender is outputting on.")]
        public int Universe = 0;

        private ArtnetManager artnetManager;
        private bool ownsManager;
        private int enabledUniverse = -1;

        /// <summary>
        /// The current DMX data for the universe as 512 floats (0f - 1f by default).
        /// Owned by the ArtnetInputManager - do not Dispose it. Read from the main thread.
        /// </summary>
        public NativeArray<float> DMXData
        {
            get
            {
                if (artnetManager == null || enabledUniverse == -1) return default;
                return artnetManager.inputManager.GetUniverseData(enabledUniverse);
            }
        }

        /// <summary>
        /// Returns the value of a DMX channel (1 - 512). Returns 0 if the universe is not enabled.
        /// </summary>
        public float GetChannel(int channel)
        {
            NativeArray<float> data = DMXData;
            if (!data.IsCreated || channel < 1 || channel > data.Length) return 0f;
            return data[channel - 1];
        }

        private void OnEnable()
        {
            //Attach if a manager already exists. If not, wait until Update - the scene's own scripts
            //may be about to create one (creating our own here would put two managers on the port).
            TryAttach(false);
        }

        private void TryAttach(bool createIfMissing)
        {
            if (artnetManager != null) return;
            artnetManager = ArtnetManager.Instance;
            if (artnetManager == null && createIfMissing)
            {
                //No manager anywhere (e.g. standalone build without a Tester style script) - create one
                artnetManager = new ArtnetManager();
                artnetManager.Start();
                ownsManager = true;
            }
            if (artnetManager != null)
            {
                artnetManager.inputManager.EnableUniverse(Universe);
                enabledUniverse = Universe;
            }
        }

        private void Update()
        {
            if (artnetManager == null)
            {
                TryAttach(true);
                return;
            }
            //Allow the universe to be changed at runtime via the inspector
            if (Universe != enabledUniverse)
            {
                artnetManager.inputManager.DisableUniverse(enabledUniverse);
                artnetManager.inputManager.EnableUniverse(Universe);
                enabledUniverse = Universe;
            }
        }

        private void OnDisable()
        {
            if (artnetManager != null && enabledUniverse != -1)
            {
                artnetManager.inputManager.DisableUniverse(enabledUniverse);
            }
            enabledUniverse = -1;
            if (ownsManager)
            {
                artnetManager.Stop();
                artnetManager.Dispose();
                ownsManager = false;
            }
            artnetManager = null;
        }
    }
}
