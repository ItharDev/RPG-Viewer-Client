using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace RPG
{
    public class PresetManager : MonoBehaviour
    {
        public static PresetManager Instance { get; private set; }
        public Dictionary<string, PresetData> Presets = new Dictionary<string, PresetData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnPresetCreated.AddListener(CreatePreset);
            Events.OnPresetModified.AddListener(ModifyPreset);
            Events.OnPresetRemoved.AddListener(RemovePreset);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnPresetCreated.RemoveListener(CreatePreset);
            Events.OnPresetModified.RemoveListener(ModifyPreset);
            Events.OnPresetRemoved.RemoveListener(RemovePreset);
        }

        public void LoadPresets(List<string> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                LoadPreset(data[i]);
            }
        }

        public PresetData GetPreset(string id)
        {
            if (!Presets.ContainsKey(id)) return default;

            return Presets[id];
        }

        private void LoadPreset(string id)
        {
            SocketManager.EmitAsync("get-light", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    PresetData data = JsonUtility.FromJson<PresetData>(callback.GetValue(1).ToString());
                    data.id = id;
                    Presets.Add(id, data);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
        private void CreatePreset(string id, PresetData data)
        {
            Presets.Add(id, data);
        }
        private void ModifyPreset(string id, PresetData data)
        {
            Presets[id] = data;
        }
        private void RemovePreset(string id, PresetData data)
        {
            Presets.Remove(id);
        }
    }
}