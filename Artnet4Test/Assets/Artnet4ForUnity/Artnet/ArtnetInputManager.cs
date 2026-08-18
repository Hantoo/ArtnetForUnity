using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using Unity.Collections;
using UnityEngine;

namespace ArtnetForUnity
{
    /// <summary>
    /// Receives ArtDMX packets from the network and stores the DMX data for each enabled universe.
    /// Each enabled universe is exposed as a NativeArray of 512 floats via GetUniverseData().
    /// Enable a universe with EnableUniverse(), or add an ArtnetDMXInput component to a GameObject.
    /// </summary>
    public class ArtnetInputManager : IDisposable
    {
        ArtnetManager artnetManager;

        /// <summary>
        /// When true, ArtDMX packets sent from the selected Art-Net interface IP are ignored.
        /// This stops universes sent out of Unity from being read straight back in when broadcasting.
        /// Not applied when the selected interface is a loopback adapter (e.g. 127.0.0.1), as every
        /// local application shares that address - Unity's own output cannot be told apart from other senders.
        /// </summary>
        public bool IgnoreOwnPackets = true;

        /// <summary>
        /// When true (default), GetUniverseData() returns values normalized 0f - 1f.
        /// When false, values are the raw DMX range 0f - 255f.
        /// </summary>
        public bool NormalizeData = true;

        public delegate void UniverseReceivedCallback(int Universe);
        /// <summary>
        /// Fired when an ArtDMX packet is received for an enabled universe.
        /// NOTE: This is invoked from the network listener thread, not the Unity main thread.
        /// </summary>
        public event UniverseReceivedCallback OnUniverseReceived;

        private readonly ConcurrentDictionary<int, DmxInputUniverse> inputUniverses = new ConcurrentDictionary<int, DmxInputUniverse>();
        private readonly HashSet<int> warnedUniverses = new HashSet<int>();

        public void init(ArtnetManager manager)
        {
            artnetManager = manager;
        }

        /// <summary>
        /// Enables receiving of an Art-Net universe (Port Address 0 - 32767). Safe to call multiple times -
        /// each EnableUniverse() should be matched with a DisableUniverse().
        /// </summary>
        public bool EnableUniverse(int Universe)
        {
            if (Universe < 0 || Universe > 32767)
            {
                Debug.LogError("[Artnet4Unity] Cannot enable input universe " + Universe + ". Universe must be between 0 and 32767.");
                return false;
            }
            DmxInputUniverse input = inputUniverses.GetOrAdd(Universe, (u) => new DmxInputUniverse(u));
            lock (input.lockObject)
            {
                input.refCount++;
            }
            return true;
        }

        /// <summary>
        /// Disables receiving of an Art-Net universe. The NativeArray for the universe is disposed
        /// once every EnableUniverse() call has been matched by a DisableUniverse() call.
        /// </summary>
        public void DisableUniverse(int Universe)
        {
            if (!inputUniverses.TryGetValue(Universe, out DmxInputUniverse input)) return;
            bool remove;
            lock (input.lockObject)
            {
                input.refCount--;
                remove = input.refCount <= 0;
            }
            if (remove && inputUniverses.TryRemove(Universe, out input))
            {
                lock (input.lockObject)
                {
                    if (input.DMXData.IsCreated) input.DMXData.Dispose();
                }
            }
        }

        public bool IsUniverseEnabled(int Universe)
        {
            return inputUniverses.ContainsKey(Universe);
        }

        /// <summary>
        /// Returns the DMX data of an enabled universe as a NativeArray of 512 floats.
        /// Values are 0f - 1f by default (see NormalizeData). Call from the Unity main thread.
        /// Do not Dispose the returned array - the manager owns it.
        /// </summary>
        public NativeArray<float> GetUniverseData(int Universe)
        {
            if (!inputUniverses.TryGetValue(Universe, out DmxInputUniverse input))
            {
                if (warnedUniverses.Add(Universe))
                    Debug.LogWarning("[Artnet4Unity] DMX Input Universe " + Universe + " is not enabled. Call EnableUniverse() or add an ArtnetDMXInput component first.");
                return default;
            }
            lock (input.lockObject)
            {
                if (input.dirty && input.DMXData.IsCreated)
                {
                    float scale = NormalizeData ? (1f / 255f) : 1f;
                    for (int i = 0; i < 512; i++)
                    {
                        input.DMXData[i] = input.rawData[i] * scale;
                    }
                    input.dirty = false;
                }
            }
            return input.DMXData;
        }

        /// <summary>
        /// Copies the raw DMX bytes (0 - 255) of an enabled universe into destination (512 bytes). Returns false if the universe is not enabled.
        /// </summary>
        public bool GetUniverseBytes(int Universe, byte[] destination)
        {
            if (destination == null || !inputUniverses.TryGetValue(Universe, out DmxInputUniverse input)) return false;
            lock (input.lockObject)
            {
                Array.Copy(input.rawData, destination, Math.Min(destination.Length, input.rawData.Length));
            }
            return true;
        }

        /// <summary>
        /// Called by ArtnetManager from the listener thread when an ArtDMX packet is received.
        /// </summary>
        public void ProcessDmxPacket(IPPacket pkt)
        {
            if (IgnoreOwnPackets && !IPAddress.IsLoopback(ArtUtils.InterfaceIPAddress) && pkt.ipAddress.ToString() == ArtUtils.InterfaceIPAddress.ToString()) return;
            byte[] data = pkt.pktData;
            if (data == null || data.Length < 20) return; //18 byte header + minimum 2 bytes of DMX data
            int universe = ArtDmx.GetUniverse(data);
            if (!inputUniverses.TryGetValue(universe, out DmxInputUniverse input)) return;
            int length = ArtDmx.GetDmxLength(data);
            if (length > data.Length - 18) length = data.Length - 18;
            if (length > 512) length = 512;
            lock (input.lockObject)
            {
                Array.Copy(data, 18, input.rawData, 0, length);
                input.dirty = true;
            }
            OnUniverseReceived?.Invoke(universe);
        }

        public void Dispose()
        {
            foreach (DmxInputUniverse input in inputUniverses.Values)
            {
                lock (input.lockObject)
                {
                    if (input.DMXData.IsCreated) input.DMXData.Dispose();
                }
            }
            inputUniverses.Clear();
        }
    }

    public class DmxInputUniverse
    {
        public int Universe;
        public byte[] rawData = new byte[512];
        public NativeArray<float> DMXData;
        public volatile bool dirty;
        public int refCount;
        public readonly object lockObject = new object();

        public DmxInputUniverse(int universe)
        {
            Universe = universe;
            DMXData = new NativeArray<float>(512, Allocator.Persistent);
        }
    }
}
