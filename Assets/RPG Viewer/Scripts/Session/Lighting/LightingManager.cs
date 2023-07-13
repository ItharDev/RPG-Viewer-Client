using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class LightingManager : MonoBehaviour
    {
        [SerializeField] private Light lightPrefab;
        [SerializeField] private Transform lightParent;

        private Dictionary<string, Light> lights = new Dictionary<string, Light>();

        private void OnEnable()
        {
            // Add event listeners
            Events.OnLightCreated.AddListener(CreateLight);
            Events.OnLightModified.AddListener(ModifyLight);
            Events.OnLightMoved.AddListener(MoveLight);
            Events.OnLightToggled.AddListener(ToggleLight);
            Events.OnLightRemoved.AddListener(RemoveLight);
            Events.OnStateChanged.AddListener(ReloadLights);
            Events.OnSceneLoaded.AddListener(LoadLights);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnLightCreated.RemoveListener(CreateLight);
            Events.OnLightModified.RemoveListener(ModifyLight);
            Events.OnLightMoved.RemoveListener(MoveLight);
            Events.OnLightToggled.RemoveListener(ToggleLight);
            Events.OnLightRemoved.RemoveListener(RemoveLight);
            Events.OnStateChanged.RemoveListener(ReloadLights);
            Events.OnSceneLoaded.RemoveListener(LoadLights);
        }

        private void CreateLight(KeyValuePair<string, LightData> info, PresetData data)
        {
            // Instantiate light and load its data
            Light light = Instantiate(lightPrefab, lightParent);
            light.LoadData(info.Key, info.Value, data);

            // Store light to dictionary
            lights.Add(info.Key, light);
        }
        private void GetLight(KeyValuePair<string, LightData> lightData)
        {
            SocketManager.EmitAsync("get-light", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    PresetData data = JsonUtility.FromJson<PresetData>(callback.GetValue(1).ToString());
                    data.id = lightData.Value.id;
                    CreateLight(lightData, data);

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, lightData.Value.id);
        }
        private void ModifyLight(string id, LightData info, PresetData data)
        {
            // Find the correct light
            Light light = lights[id];

            // Check if light was found
            if (light == null) return;

            // Load new data
            light.LoadData(id, info, data);
        }
        private void MoveLight(string id, LightData data)
        {
            // Find the correct light
            Light light = lights[id];

            // Check if light was found
            if (light == null) return;

            // Load new data
            light.LoadData(data);
        }
        private void ToggleLight(string id, bool enabled)
        {
            // Find the correct light
            Light light = lights[id];

            // Check if light was found
            if (light == null) return;

            // Load new data
            light.Toggle(enabled);
        }
        private void RemoveLight(string id)
        {
            // Find the correct light
            Light light = lights[id];

            // Check if light was found
            if (light == null) return;

            lights.Remove(id);
            Destroy(light.gameObject);
        }
        private void ReloadLights(SessionState oldState, SessionState newState)
        {
            // Check if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadLights();
            }
            else
            {
                // Unload lights if syncing was disabled
                if (!newState.synced)
                {
                    UnloadLights();
                    return;
                }

                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadLights();
            }
        }
        private void UnloadLights()
        {
            // Loop through each light
            foreach (var item in lights)
            {
                // Continue if light is null
                if (item.Value == null) continue;
                Destroy(item.Value.gameObject);
            }

            // Clear list
            lights.Clear();
        }
        private void LoadLights(SceneData settings)
        {
            // Get lighting data
            Dictionary<string, LightData> list = settings.lights;

            // Generate lights
            for (int i = 0; i < list.Count; i++)
            {
                GetLight(list.ElementAt(i));
            }
        }
    }
}