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
            Socket.On("change-state", async (data) =>
            {
                bool synced = data.GetValue().GetBoolean();
                string scene = data.GetValue(1).GetString();
                SessionState newState = new SessionState(synced, scene);
                SessionState oldState = ConnectionManager.State;

                await UniTask.SwitchToMainThread();
                Events.OnStateChanged?.Invoke(oldState, newState);
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
                Events.OnDoorModified?.Invoke(wall.wallId, wall);
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
                LightData light = JsonUtility.FromJson<LightData>(data.GetValue().GetString());

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
            Socket.On("create-preset", (data) =>
            {
                // string id = data.GetValue().GetString();
                // LightPreset preset = JsonUtility.FromJson<LightPreset>(data.GetValue(1).ToString());

                // await UniTask.SwitchToMainThread();
                // Events.OnPresetCreated?.Invoke(id, preset);
            });
            Socket.On("modify-preset", (data) =>
            {
                // string id = data.GetValue().GetString();
                // LightPreset preset = JsonUtility.FromJson<LightPreset>(data.GetValue(1).ToString());

                // await UniTask.SwitchToMainThread();
                // Events.OnPresetModified?.Invoke(id, preset);
            });
            Socket.On("remove-preset", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnPresetRemoved?.Invoke(id);
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

            // Notes
            Socket.On("create-note", (data) =>
            {
                // string id = data.GetValue().GetString();
                // NoteData note = JsonUtility.FromJson<NoteData>(data.GetValue(1).ToString());
                // note.id = id;

                // await UniTask.SwitchToMainThread();
                // Events.OnNoteCreated?.Invoke(note);
            });
            Socket.On("modify-note-text", async (data) =>
            {
                string id = data.GetValue().GetString();
                string text = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnNoteTextModified?.Invoke(id, text);
            });
            Socket.On("modify-note-image", async (data) =>
            {
                string id = data.GetValue().GetString();
                string image = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnNoteImageModified?.Invoke(id, image);
            });
            Socket.On("modify-note-header", async (data) =>
            {
                string id = data.GetValue().GetString();
                string header = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnNoteHeaderModified?.Invoke(id, header);
            });
            Socket.On("set-note-state", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool state = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnNoteEnabled?.Invoke(id, state);
            });
            Socket.On("move-note", async (data) =>
            {
                string id = data.GetValue().GetString();
                Vector2 pos = JsonUtility.FromJson<Vector2>(data.GetValue(1).GetString());

                await UniTask.SwitchToMainThread();
                Events.OnNoteMoved?.Invoke(id, pos);
            });
            Socket.On("remove-note", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnNoteRemoved?.Invoke(id);
            });
            Socket.On("show-note", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnNoteShown?.Invoke(id);
            });

            Socket.On("modify-journal-text", async (data) =>
            {
                string id = data.GetValue().GetString();
                string text = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnJournalTextModified?.Invoke(id, text);
            });
            Socket.On("modify-journal-image", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                string id = data.GetValue().GetString();
                string image = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnJournalImageModified?.Invoke(id, image);
            });
            Socket.On("modify-journal-header", async (data) =>
            {
                string id = data.GetValue().GetString();
                string header = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnJournalHeaderModified?.Invoke(id, header);
            });
            Socket.On("set-collaborators", (data) =>
            {
                string id = data.GetValue().GetString();
                var list = data.GetValue(1).EnumerateArray().ToArray();

                // List<Collaborator> collaborators = new List<Collaborator>();
                // for (int i = 0; i < list.Length; i++)
                // {
                //     collaborators.Add(JsonUtility.FromJson<Collaborator>(list[i].ToString()));
                // }

                // await UniTask.SwitchToMainThread();
                // Events.OnCollaboratorsModified?.Invoke(id, collaborators);
            });
            Socket.On("remove-journal", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnJournalRemoved?.Invoke(id);
            });
            Socket.On("show-journal", (data) =>
            {
                // JournalData journal = JsonUtility.FromJson<JournalData>(data.GetValue().GetString());

                // await UniTask.SwitchToMainThread();
                // Events.OnJournalShown?.Invoke(journal);
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
