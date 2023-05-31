using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace RPG
{
    public class PresetManager : MonoBehaviour
    {
        public static PresetManager Instance { get; private set; }
        public Dictionary<string, LightData> Presets = new Dictionary<string, LightData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        public void LoadPresets(List<string> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                LoadPreset(data[i]);
            }
        }
        public LightData GetPreset(string id)
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
                    LightData data = JsonUtility.FromJson<LightData>(callback.GetValue(1).ToString());
                    data.id = id;
                    Presets.Add(id, data);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
    }
}