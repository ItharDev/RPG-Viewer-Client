using System.Collections.Generic;
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
        private bool canCreate;
        private bool canRemove;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnLightCreated.AddListener(CreateLight);
            Events.OnLightModified.AddListener(ModifyLight);
            Events.OnLightRemoved.AddListener(RemoveLight);
            Events.OnStateChanged.AddListener(ReloadLights);
            Events.OnSceneLoaded.AddListener(LoadLights);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnLightCreated.RemoveListener(CreateLight);
            Events.OnLightModified.RemoveListener(ModifyLight);
            Events.OnLightRemoved.RemoveListener(RemoveLight);
            Events.OnStateChanged.RemoveListener(ReloadLights);
            Events.OnSceneLoaded.RemoveListener(LoadLights);
        }

        private void CreateLight(LightData data)
        {
            // Instantiate light and load its data
            Light light = Instantiate(lightPrefab, lightParent);
            light.LoadData(data);

            // Store light to dictionary
            lights.Add(data.id, light);
        }
        private void GetLight(string id)
        {
            SocketManager.EmitAsync("get-light", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    LightData data = JsonUtility.FromJson<LightData>(callback.GetValue(1).ToString());
                    data.id = id;
                    CreateLight(data);

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
        private void ModifyLight(string id, LightData data)
        {
            // Find the correct light
            Light light = lights[id];

            // Check if light was found
            if (light == null) return;

            // Load new data
            light.LoadData(data);
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
                // Unload tokens if syncing was disabled
                if (oldState.synced && !newState.synced)
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
            List<string> list = settings.lights;

            // Generate lights
            for (int i = 0; i < list.Count; i++)
            {
                GetLight(list[i]);
            }
        }
    }
}