using System.Collections.Generic;
using System.Linq;
using Networking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class LightManager : MonoBehaviour
    {
        [SerializeField] private LightHolder lightPrefab;
        [SerializeField] private Transform lightParent;

        private List<LightHolder> lights = new List<LightHolder>();
        public StateManager StateManager;
        private LightData copyData;
        private bool lightEnabled;

        private void Update()
        {

            if (StateManager.ToolState == ToolState.Light && !lightEnabled)
            {
                lightEnabled = true;
                for (int i = 0; i < lights.Count; i++)
                {
                    lights[i].ShowLight(true);
                }
            }

            if (StateManager.ToolState != ToolState.Light && lightEnabled)
            {
                lightEnabled = false;
                for (int i = 0; i < lights.Count; i++)
                {
                    lights[i].ShowLight(false);
                }
            }

            if (lightEnabled)
            {
                if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    bool modified = false;
                    for (int i = 0; i < lights.Count; i++)
                    {
                        if (lights[i].Selected)
                        {
                            modified = true;
                            break;
                        }
                    }
                    if (!modified && StateManager.LightState == LightState.Create) CreateLight(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
                {
                    bool modified = false;
                    for (int i = 0; i < lights.Count; i++)
                    {
                        if (lights[i].Selected)
                        {
                            modified = true;
                            break;
                        }
                    }
                    if (!modified && !string.IsNullOrEmpty(copyData.id)) PasteLight(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                }
            }
        }

        private async void CreateLight(Vector2 pos)
        {
            var data = new LightData()
            {
                radius = 20.0f,
                enabled = false,
                position = pos,
                intensity = 1.0f,
                color = new Color(1, 1, 1, 1),
                effect = 0,
                flickerFrequency = 15.0f,
                flickerAmount = 0.1f,
                pulseInterval = 2.0f,
                pulseAmount = 0.6f,
                preset = ""
            };

            await SocketManager.Socket.EmitAsync("create-light", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(data));
        }
        private async void PasteLight(Vector2 pos)
        {
            copyData.position = pos;
            await SocketManager.Socket.EmitAsync("create-light", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(copyData));
        }

        public void CopyLight(LightData data)
        {
            copyData = data;
        }

        public async void RemoveLight(LightHolder holder)
        {
            await SocketManager.Socket.EmitAsync("remove-light", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, holder.Data.id);
        }
        public async void ModifyLight(LightHolder holder)
        {
            Debug.Log(holder.Data.effect);
            await SocketManager.Socket.EmitAsync("modify-light", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(holder.Data));
        }
        public void SelectLight(LightHolder holder)
        {
            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i] != holder) lights[i].Deselect();
            }
        }
        public void UnloadLights()
        {
            for (int i = 0; i < lights.Count; i++)
            {
                Destroy(lights[i].gameObject);
            }

            lights.Clear();
        }

        public void AddLight(LightData data)
        {
            var light = Instantiate(lightPrefab, lightParent);
            light.LoadData(data, this, StateManager.ToolState == ToolState.Light);
            lights.Add(light);
        }
        public void ModifyLight(LightData data)
        {
            var light = lights.FirstOrDefault(x => x.Data.id == data.id);
            if (light == null) return;

            light.LoadData(data, this, StateManager.ToolState == ToolState.Light);
        }
        public void RemoveLight(string id)
        {
            var light = lights.FirstOrDefault(x => x.Data.id == id);
            if (light == null) return;

            lights.Remove(light);
            Destroy(light.Config.gameObject);
            Destroy(light.gameObject);
        }
    }
}