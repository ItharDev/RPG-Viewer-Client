using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RPG;
using SocketIOClient;
using UnityEngine;

namespace Networking
{
    public class SocketManager : MonoBehaviour
    {
        public static SocketIOUnity Socket;
        public static bool IsConnected
        {
            get { return Socket == null ? false : Socket.Connected; }
        }

        private void OnDisable()
        {
            // Return if connection is not established
            if (Socket == null) return;

            // Remove connection listeners
            Socket.OnDisconnected -= OnDisconnected;
            Socket.OnConnected -= OnConnected;
            Socket.OnReconnectError -= OnReconnect;

            // Disconnect socket from server
            if (Application.isPlaying) Socket.Disconnect();
        }

        private static void OnReconnect(object sender, Exception e)
        {
            MessageManager.QueueMessage("Failed to establish connection to the server. Reconnecting after a while");
        }

        private static async void OnConnected(object sender, EventArgs e)
        {
            await UniTask.SwitchToMainThread();
            MessageManager.QueueMessage("Connection established to the server");

            AddListeners();

            // Send connection event
            Events.OnConnected?.Invoke();
        }

        private static async void OnDisconnected(object sender, string e)
        {
            await UniTask.SwitchToMainThread();

            MessageManager.QueueMessage("Disconnected from the server. Returning to menu");

            // Send disconnection event
            Events.OnDisconnected?.Invoke();
        }

        private static void AddListeners()
        {
            // Scene state
            Socket.On("user-connected", async (data) =>
            {
                string name = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnUserConnected?.Invoke(name);
            });
            Socket.On("user-disconnected", async (data) =>
            {
                string name = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnUserDisconnected?.Invoke(name);
            });
            Socket.On("set-state", (data) =>
            {
                string scene = data.GetValue().GetString();
                bool synced = data.GetValue(1).GetBoolean();

                SessionState newState = new SessionState(synced, scene);
                SessionState oldState = ConnectionManager.State;
                EmitAsync("set-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (callback.GetValue().GetBoolean())
                    {
                        Debug.Log("Runs");
                        Debug.Log(Events.OnStateChanged);
                        Events.OnStateChanged?.Invoke(oldState, newState);
                        Debug.Log("Runs 1");
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, scene);
            });
            Socket.On("change-landing-page", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnLandingPageChanged?.Invoke(id);
            });

            // Doors
            Socket.On("toggle-door", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool open = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnDoorOpened?.Invoke(id, open);
            });
            Socket.On("modify-door", async (data) =>
            {
                WallData wall = JsonUtility.FromJson<WallData>(data.GetValue().GetString());

                await UniTask.SwitchToMainThread();
                Events.OnDoorModified?.Invoke(wall.id, wall);
            });

            // Grid
            Socket.On("modify-grid", async (data) =>
            {
                GridData gridData = JsonUtility.FromJson<GridData>(data.GetValue().ToString());

                await UniTask.SwitchToMainThread();
                Events.OnGridChanged?.Invoke(gridData, true, true);
            });

            // Lights
            Socket.On("create-light", (data) =>
            {
                // LightData light = JsonUtility.FromJson<LightData>(data.GetValue().GetString());

                // await UniTask.SwitchToMainThread();
                // Events.OnLightCreated?.Invoke(light);
            });
            Socket.On("modify-light", async (data) =>
            {
                PresetData light = JsonUtility.FromJson<PresetData>(data.GetValue().GetString());

                await UniTask.SwitchToMainThread();
                Events.OnLightModified?.Invoke(light.id, light);
            });
            Socket.On("remove-light", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnLightRemoved?.Invoke(id);
            });

            // Presets
            Socket.On("create-preset", async (data) =>
            {
                string id = data.GetValue().GetString();
                PresetData preset = JsonUtility.FromJson<PresetData>(data.GetValue(1).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnPresetCreated?.Invoke(id, preset);
            });
            Socket.On("modify-preset", async (data) =>
            {
                string id = data.GetValue().GetString();
                PresetData preset = JsonUtility.FromJson<PresetData>(data.GetValue(1).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnPresetModified?.Invoke(id, preset);
            });
            Socket.On("remove-preset", async (data) =>
            {
                string id = data.GetValue().GetString();
                PresetData light = JsonUtility.FromJson<PresetData>(data.GetValue(1).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnPresetRemoved?.Invoke(id, light);
            });

            // Tokens
            Socket.On("create-token", async (data) =>
            {
                string id = data.GetValue().GetString();
                TokenData token = JsonUtility.FromJson<TokenData>(data.GetValue(1).ToString());
                token.id = id;

                await UniTask.SwitchToMainThread();
                Events.OnTokenCreated?.Invoke(token);
            });
            Socket.On("move-token", async (data) =>
            {
                string id = data.GetValue().GetString();
                MovementData movement = JsonUtility.FromJson<MovementData>(data.GetValue(1).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnTokenMoved?.Invoke(id, movement);
            });
            Socket.On("modify-token", async (data) =>
            {
                string id = data.GetValue().GetString();
                TokenData token = JsonUtility.FromJson<TokenData>(data.GetValue(1).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnTokenModified?.Invoke(id, token);
            });
            Socket.On("remove-token", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnTokenRemoved?.Invoke(id);
            });
            Socket.On("update-visibility", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool enabled = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnTokenEnabled?.Invoke(id, enabled);
            });
            Socket.On("update-visibility", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool enabled = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnTokenEnabled?.Invoke(id, enabled);
            });
            Socket.On("update-conditions", async (data) =>
            {
                string id = data.GetValue().GetString();
                int conditions = data.GetValue(1).GetInt32();

                await UniTask.SwitchToMainThread();
                Events.OnConditionsModified?.Invoke(id, conditions);
            });
            Socket.On("update-health", async (data) =>
            {
                string id = data.GetValue().GetString();
                int health = data.GetValue(1).GetInt32();

                await UniTask.SwitchToMainThread();
                Events.OnHealthModified?.Invoke(id, health);
            });
            Socket.On("update-elevation", async (data) =>
            {
                string id = data.GetValue().GetString();
                string elevation = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnElevationModified?.Invoke(id, elevation);
            });
            Socket.On("rotate-token", async (data) =>
            {
                string id = data.GetValue().GetString();
                float angle = (float)data.GetValue(1).GetDouble();

                await UniTask.SwitchToMainThread();
                Events.OnTokenRotated?.Invoke(id, angle);
            });
        }

        public static void Connect(string address)
        {
            // Create new socket
            CreateSocket(address);

            // Add connection listeners
            Socket.OnDisconnected += OnDisconnected;
            Socket.OnConnected += OnConnected;
            Socket.OnReconnectError += OnReconnect;

            // Send connection request
            Socket.Connect();
        }

        private static void CreateSocket(string address)
        {
            // Check if previous connection exists
            if (Socket != null)
            {
                // Disconnect socket
                Socket.Disconnect();

                // Remove connection listeners
                Socket.OnDisconnected -= OnDisconnected;
                Socket.OnConnected -= OnConnected;
                Socket.OnReconnectError -= OnReconnect;
            }

            // Generate new address
            var uri = new Uri($"http://{address}");
            Socket = new SocketIOUnity(uri, new SocketIOOptions
            {
                // Create handshake
                Query = new Dictionary<string, string>
                {
                    { "token", "UNITY" }
                },
                EIO = 4,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                ReconnectionDelay = 10000,
                ReconnectionAttempts = 5
            });
        }

        public static async void EmitAsync(string eventName, Action<SocketIOResponse> callback, params object[] data)
        {
            // Return if socket is not connected
            if (!Socket.Connected) return;

            // Emit new event to socket
            await Socket.EmitAsync(eventName, (res) =>
            {
                // Callback on ack
                callback(res);
            }, data);
        }
        public static void Emit(string eventName, params object[] data)
        {
            // Return if socket is not connected
            if (!Socket.Connected) return;

            // Emit new event to socket
            Socket.Emit(eventName, data);
        }
    }
}
