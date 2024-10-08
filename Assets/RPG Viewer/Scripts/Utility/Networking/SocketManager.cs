using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Cysharp.Threading.Tasks;
using RPG;
using SocketIOClient;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            SceneManager.LoadScene("Menu");
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

                EmitAsync("set-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (callback.GetValue().GetBoolean())
                    {
                        SessionState oldState = ConnectionManager.State;
                        Events.OnStateChanged?.Invoke(oldState, newState);
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
            Socket.On("change-scene-image", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnSceneImageChanged?.Invoke(id);
            });

            // Walls
            Socket.On("create-wall", async (data) =>
            {
                WallData wall = JsonUtility.FromJson<WallData>(data.GetValue().ToString());

                await UniTask.SwitchToMainThread();
                Events.OnWallCreated?.Invoke(wall, true);
            });
            Socket.On("modify-wall", async (data) =>
            {
                string id = data.GetValue().GetString();
                WallData wall = JsonUtility.FromJson<WallData>(data.GetValue(1).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnWallModified?.Invoke(id, wall);
            });
            Socket.On("remove-wall", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnWallRemoved?.Invoke(id);
            });

            // Grid
            Socket.On("modify-grid", async (data) =>
            {
                GridData gridData = JsonUtility.FromJson<GridData>(data.GetValue().ToString());

                await UniTask.SwitchToMainThread();
                Events.OnGridChanged?.Invoke(gridData, true, true);
            });

            // Lights
            Socket.On("create-light", async (data) =>
            {
                string id = data.GetValue().GetString();
                LightData info = JsonUtility.FromJson<LightData>(data.GetValue(1).ToString());
                PresetData light = JsonUtility.FromJson<PresetData>(data.GetValue(2).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnLightCreated?.Invoke(new KeyValuePair<string, LightData>(id, info), light);
            });
            Socket.On("modify-light", async (data) =>
            {
                string id = data.GetValue().GetString();
                LightData info = JsonUtility.FromJson<LightData>(data.GetValue(1).ToString());
                PresetData light = JsonUtility.FromJson<PresetData>(data.GetValue(2).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnLightModified?.Invoke(id, info, light);
            });
            Socket.On("move-light", async (data) =>
            {
                string id = data.GetValue().GetString();
                LightData info = JsonUtility.FromJson<LightData>(data.GetValue(1).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnLightMoved?.Invoke(id, info);
            });
            Socket.On("toggle-light", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool enabled = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnLightToggled?.Invoke(id, enabled);
            });
            Socket.On("remove-light", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnLightRemoved?.Invoke(id);
            });
            Socket.On("modify-lighting", async (data) =>
            {
                RPG.LightingSettings lightingData = JsonUtility.FromJson<RPG.LightingSettings>(data.GetValue().ToString());

                await UniTask.SwitchToMainThread();
                Events.OnLightingChanged?.Invoke(lightingData, true);
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
            Socket.On("lock-token", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool locked = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnTokenLocked?.Invoke(id, locked);
            });
            Socket.On("toggle-token-light", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool enabled = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnTokenLightToggled?.Invoke(id, enabled);
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
                int elevation = data.GetValue(1).GetInt32();

                await UniTask.SwitchToMainThread();
                Events.OnElevationModified?.Invoke(id, elevation);
            });
            Socket.On("rotate-token", async (data) =>
            {
                string id = data.GetValue().GetString();
                float angle = (float)data.GetValue(1).GetDouble();
                string user = data.GetValue(2).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnTokenRotated?.Invoke(id, angle, user);
            });
            Socket.On("rotate-token-light", async (data) =>
            {
                string id = data.GetValue().GetString();
                float angle = (float)data.GetValue(1).GetDouble();
                string user = data.GetValue(2).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnTokenLightRotated?.Invoke(id, angle, user);
            });
            Socket.On("show-image", async (data) =>
            {
                string id = data.GetValue().GetString();
                string uid = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnImageShowed?.Invoke(id, uid);
            });

            // Initiatives
            Socket.On("create-initiative", async (data) =>
            {
                InitiativeData initiative = JsonUtility.FromJson<InitiativeData>(data.GetValue(0).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnInitiativeCreated?.Invoke(initiative);
            });
            Socket.On("modify-initiative", async (data) =>
            {
                InitiativeData initiative = JsonUtility.FromJson<InitiativeData>(data.GetValue(0).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnInitiativeModified?.Invoke(initiative.id, initiative);
            });
            Socket.On("remove-initiative", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnInitiativeRemoved?.Invoke(id);
            });
            Socket.On("reset-initiatives", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                Events.OnInitiativesReset?.Invoke();
            });
            Socket.On("sort-initiative", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                Events.OnInitiativeSorted?.Invoke();
            });

            // Notes
            Socket.On("create-note", async (data) =>
            {
                string id = data.GetValue().GetString();
                NoteInfo noteInfo = JsonUtility.FromJson<NoteInfo>(data.GetValue(1).ToString());
                NoteData noteData = JsonUtility.FromJson<NoteData>(data.GetValue(2).ToString());

                noteInfo.id = id;
                noteData.id = id;

                await UniTask.SwitchToMainThread();
                Events.OnNoteCreated?.Invoke(noteInfo, noteData);
            });
            Socket.On("move-note", async (data) =>
            {
                string id = data.GetValue().GetString();
                NoteInfo noteInfo = JsonUtility.FromJson<NoteInfo>(data.GetValue(1).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnNoteMoved?.Invoke(id, noteInfo.position);
            });
            Socket.On("modify-note-text", async (data) =>
            {
                string id = data.GetValue().GetString();
                string text = data.GetValue(1).GetString();
                string uid = data.GetValue(2).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnNoteTextModified?.Invoke(id, text, uid);
            });
            Socket.On("modify-note-header", async (data) =>
            {
                string id = data.GetValue().GetString();
                string header = data.GetValue(1).GetString();
                string uid = data.GetValue(2).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnNoteHeaderModified?.Invoke(id, header, uid);
            });
            Socket.On("modify-note-image", async (data) =>
            {
                string id = data.GetValue().GetString();
                string image = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnNoteImageModified?.Invoke(id, image);
            });
            Socket.On("set-note-global", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool isGlobal = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnNoteSetToGlobal?.Invoke(id, isGlobal);
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
                Events.OnNoteShowed?.Invoke(id);
            });

            // Journals
            Socket.On("modify-journal-text", async (data) =>
            {
                string id = data.GetValue().GetString();
                string text = data.GetValue(1).GetString();
                string uid = data.GetValue(2).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnJournalTextModified?.Invoke(id, text, uid);
            });
            Socket.On("modify-journal-header", async (data) =>
            {
                string id = data.GetValue().GetString();
                string header = data.GetValue(1).GetString();
                string uid = data.GetValue(2).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnJournalHeaderModified?.Invoke(id, header, uid);
            });
            Socket.On("modify-journal-image", async (data) =>
            {
                string id = data.GetValue().GetString();
                string image = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnJournalImageModified?.Invoke(id, image);
            });
            Socket.On("remove-journal", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnJournalRemoved?.Invoke(id);
            });
            Socket.On("share-journal", async (data) =>
            {
                string id = data.GetValue().GetString();
                string uid = data.GetValue(1).GetString();
                JournalData journalData = JsonUtility.FromJson<JournalData>(data.GetValue(2).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnCollaboratorsUpdated?.Invoke(id, uid, journalData.collaborators);
            });
            Socket.On("show-journal", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnJournalShowed?.Invoke(id);
            });

            // Ping
            Socket.On("start-pointer", async (data) =>
            {
                string id = data.GetValue().GetString();
                Vector2 position = JsonUtility.FromJson<Vector2>(data.GetValue(1).GetString());

                await UniTask.SwitchToMainThread();
                Events.OnPointerStarted?.Invoke(id, position);
            });
            Socket.On("update-pointer", async (data) =>
            {
                string id = data.GetValue().GetString();
                Vector2 position = JsonUtility.FromJson<Vector2>(data.GetValue(1).GetString());

                await UniTask.SwitchToMainThread();
                Events.OnPointerUpdated?.Invoke(id, position);
            });
            Socket.On("stop-pointer", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnPointerStopped?.Invoke(id);
            });
            Socket.On("ping", async (data) =>
            {
                Vector2 position = JsonUtility.FromJson<Vector2>(data.GetValue(0).GetString());
                bool strong = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnPing?.Invoke(position, strong);
            });

            Socket.On("create-portal", async (data) =>
            {
                string id = data.GetValue().GetString();
                PortalData portal = JsonUtility.FromJson<PortalData>(data.GetValue(1).ToString());

                await UniTask.SwitchToMainThread();
                Events.OnPortalCreated?.Invoke(id, portal);
            });
            Socket.On("move-portal", async (data) =>
            {
                string id = data.GetValue().GetString();
                Vector2 position = JsonUtility.FromJson<Vector2>(data.GetValue(1).GetString());

                await UniTask.SwitchToMainThread();
                Events.OnPortalMoved?.Invoke(id, position);
            });
            Socket.On("link-portal", async (data) =>
            {
                string id = data.GetValue().GetString();
                string link = data.GetValue(1).GetString();

                await UniTask.SwitchToMainThread();
                Events.OnPortalLinked?.Invoke(id, link);
            });
            Socket.On("activate-portal", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool active = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnPortalEnabled?.Invoke(id, active);
            });
            Socket.On("remove-portal", async (data) =>
            {
                string id = data.GetValue().GetString();

                await UniTask.SwitchToMainThread();
                Events.OnPortalRemoved?.Invoke(id);
            });
            Socket.On("set-portal-radius", async (data) =>
            {
                string id = data.GetValue().GetString();
                float radius = (float)data.GetValue(1).GetDouble();

                await UniTask.SwitchToMainThread();
                Events.OnPortalRadiusUpdated?.Invoke(id, radius);
            });
            Socket.On("set-portal-continuous", async (data) =>
            {
                string id = data.GetValue().GetString();
                bool continuous = data.GetValue(1).GetBoolean();

                await UniTask.SwitchToMainThread();
                Events.OnPortalBehaviourUpdated?.Invoke(id, continuous);
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
