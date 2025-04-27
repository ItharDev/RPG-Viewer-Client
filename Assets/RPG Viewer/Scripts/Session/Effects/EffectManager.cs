using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace RPG
{
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance { get; private set; }
        public Dictionary<string, EffectData> Effects = new Dictionary<string, EffectData>();

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
            Events.OnEffectCreated.AddListener(CreateEffect);
            Events.OnEffectModified.AddListener(ModifyEffect);
            Events.OnEffectRemoved.AddListener(RemoveEffect);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnEffectCreated.RemoveListener(CreateEffect);
            Events.OnEffectModified.RemoveListener(ModifyEffect);
            Events.OnEffectRemoved.RemoveListener(RemoveEffect);
        }

        public void LoadEffects(List<string> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                LoadEffect(data[i]);
            }
        }

        public EffectData GetEffect(string id)
        {
            if (!Effects.ContainsKey(id)) return default;

            return Effects[id];
        }

        private void LoadEffect(string id)
        {
            SocketManager.EmitAsync("get-effect", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    EffectData data = JsonUtility.FromJson<EffectData>(callback.GetValue(1).ToString());
                    data.id = id;
                    Effects.Add(id, data);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
        private void CreateEffect(string id, EffectData data)
        {
            Effects.Add(id, data);
        }
        private void ModifyEffect(string id, EffectData data)
        {
            Effects[id] = data;
        }
        private void RemoveEffect(string id, EffectData data)
        {
            Effects.Remove(id);
        }
    }
}