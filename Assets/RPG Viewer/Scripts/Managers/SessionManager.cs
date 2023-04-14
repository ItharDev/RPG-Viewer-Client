using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        public static bool IsMaster;
        public static string MasterId;

        public static bool Synced;
        public static string Scene;
        public static List<string> Users = new List<string>();
        public static Sprite BackgroundSprite;

        public static Session Session;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        [System.Obsolete]
        public void Subscribe()
        {
            SocketManager.Socket.On("user-connected", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                MessageManager.QueueMessage($"{data.GetValue().GetString()} connected");
            });
            SocketManager.Socket.On("user-disconnected", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                MessageManager.QueueMessage($"{data.GetValue().GetString()} disconnected");
            });
            SocketManager.Socket.On("set-scene", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                Scene = data.GetValue().ToString();
                SocketManager.Socket.Emit("update-scene", Scene);
                if (Synced || IsMaster) Session.LoadScene(Scene);
            });
            SocketManager.Socket.On("change-state", async (data) =>
            {
                await UniTask.SwitchToMainThread();

                var lastScene = Scene;
                Synced = data.GetValue().GetBoolean();
                Scene = data.GetValue(1).GetString();

                SocketManager.Socket.Emit("update-scene", Scene);
                if (IsMaster && Scene == lastScene) return;
                if (Synced && !string.IsNullOrEmpty(Scene)) Session.LoadScene(Scene);
                else Session.UnloadScene();
            });
            SocketManager.Socket.On("toggle-door", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.ToggleDoor(data.GetValue().GetString(), data.GetValue(1).GetBoolean());
            });
            SocketManager.Socket.On("modify-door", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.ModifyWall(JsonUtility.FromJson<WallData>(data.GetValue().GetString()));
            });
            SocketManager.Socket.On("create-light", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.CreateLight(JsonUtility.FromJson<LightData>(data.GetValue().GetString()));
            }); SocketManager.Socket.On("modify-light", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.ModifyLight(JsonUtility.FromJson<LightData>(data.GetValue().GetString()));
            });
            SocketManager.Socket.On("remove-light", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.RemoveLight(data.GetValue().GetString());
            });
            SocketManager.Socket.On("create-preset", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) LightingPresets.AddPreset(data.GetValue().GetString(), JsonUtility.FromJson<LightPreset>(data.GetValue(1).GetString()));
            });
            SocketManager.Socket.On("modify-preset", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) LightingPresets.ModifyPreset(data.GetValue().GetString(), JsonUtility.FromJson<LightPreset>(data.GetValue(1).GetString()));
            });
            SocketManager.Socket.On("remove-preset", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) LightingPresets.RemovePreset(data.GetValue().GetString());
            });
            SocketManager.Socket.On("create-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var token = JsonUtility.FromJson<TokenData>(data.GetValue().ToString());
                token.id = data.GetValue(1).GetString();
                if (Session != null) Session.CreateToken(token);
            });
            SocketManager.Socket.On("move-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                MovementData movement = JsonUtility.FromJson<MovementData>(data.GetValue(1).GetString());
                if (Session != null) Session.MoveToken(data.GetValue().GetString(), movement);
            });
            SocketManager.Socket.On("modify-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.ModifyToken(data.GetValue().GetString(), JsonUtility.FromJson<TokenData>(data.GetValue(1).ToString()));
            });
            SocketManager.Socket.On("remove-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.RemoveToken(data.GetValue().GetString());
            });
            SocketManager.Socket.On("update-visibility", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.UpdateVisibility(data.GetValue(0).GetString(), data.GetValue(1).GetBoolean());
            });
            SocketManager.Socket.On("update-conditions", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.UpdateConditions(data.GetValue().GetString(), data.GetValue(1).GetInt32());
            });
            SocketManager.Socket.On("lock-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.LockToken(data.GetValue().GetString(), data.GetValue(1).GetBoolean());
            });
            SocketManager.Socket.On("update-health", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.UpdateHealth(data.GetValue().GetString(), data.GetValue(1).GetInt32());
            });
            SocketManager.Socket.On("update-elevation", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.UpdateElevation(data.GetValue().GetString(), data.GetValue(1).GetString());
            });
            SocketManager.Socket.On("rotate-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (Session != null) Session.RotateToken(data.GetValue().GetString(), (float)data.GetValue(1).GetDouble());
            });

            SocketManager.Socket.On("create-note", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var note = JsonUtility.FromJson<NoteData>(data.GetValue().ToString());
                note.id = data.GetValue(1).GetString();
                if (Session != null) Session.CreateNote(note);
            });
            SocketManager.Socket.On("modify-note-text", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var text = data.GetValue(1).GetString();
                if (Session != null) Session.ModifyNoteText(id, text);
            });
            SocketManager.Socket.On("modify-note-header", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var text = data.GetValue(1).GetString();
                if (Session != null) Session.ModifyNoteHeader(id, text);
            });
            SocketManager.Socket.On("modify-note-image", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var image = data.GetValue(1).GetString();
                if (Session != null) Session.ModifyNoteImage(id, image);
            });
            SocketManager.Socket.On("set-note-state", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var state = data.GetValue(1).GetBoolean();
                if (Session != null) Session.SetNotePublic(id, state);
            });
            SocketManager.Socket.On("move-note", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var pos = JsonUtility.FromJson<Vector2>(data.GetValue(1).GetString());
                if (Session != null) Session.MoveNote(id, pos);
            });
            SocketManager.Socket.On("remove-note", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                if (Session != null) Session.RemoveNote(id);
            });
            SocketManager.Socket.On("show-note", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                if (Session != null) Session.ShowNote(id);
            });

            SocketManager.Socket.On("modify-journal-text", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var text = data.GetValue(1).GetString();
                if (Session != null) Session.ModifyJournalText(id, text);
            });
            SocketManager.Socket.On("modify-journal-header", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                Debug.Log(data);
                var id = data.GetValue().GetString();
                var text = data.GetValue(1).GetString();
                if (Session != null) Session.ModifyJournalHeader(id, text);
            });
            SocketManager.Socket.On("modify-journal-image", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var image = data.GetValue(1).GetString();
                if (Session != null) Session.ModifyJournalImage(id, image);
            });
            SocketManager.Socket.On("set-collaborators", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var list = data.GetValue(1).EnumerateArray().ToArray();
                List<Collaborator> collaborators = new List<Collaborator>();
                for (int i = 0; i < list.Length; i++)
                {
                    collaborators.Add(JsonUtility.FromJson<Collaborator>(list[i].ToString()));
                }
                if (Session != null) Session.SetCollaborators(id, collaborators);
            });
            SocketManager.Socket.On("remove-journal", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                if (Session != null) Session.RemoveJournal(id);
            });
            SocketManager.Socket.On("show-journal", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                Debug.Log(data);
                var dournalData = JsonUtility.FromJson<JournalData>(data.GetValue().GetString());
                if (Session != null) Session.ShowJournal(dournalData);
            });
        }
        private void Update()
        {
            if (Session == null) Session = FindObjectOfType<Session>();
        }

        [System.Obsolete]
        public static void JoinSession(JoinData data)
        {
            WebManager.Download(data.background, true, async (bytes) =>
            {
                await UniTask.SwitchToMainThread();

                for (int i = 0; i < data.lightingPresets.Count; i++)
                {
                    LoadPreset(data.lightingPresets[i]);
                }

                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                BackgroundSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                var Loader = SceneManager.LoadSceneAsync("Session");
                await UniTask.WaitUntil(() => Loader.isDone);

                IsMaster = data.master;
                Synced = data.synced;
                Scene = data.scene;
                Users = data.users;
                MasterId = data.masterId;

                if (Session == null) Session = FindObjectOfType<Session>();
                if (!IsMaster && !Synced) return;

                if (!string.IsNullOrEmpty(data.scene))
                {
                    SocketManager.Socket.Emit("update-scene", Scene);
                    Session.LoadScene(data.scene);
                }
            });
        }

        private static async void LoadPreset(string id)
        {
            await SocketManager.Socket.EmitAsync("load-preset", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    var data = JsonUtility.FromJson<LightPreset>(callback.GetValue(1).ToString());
                    LightingPresets.AddPreset(id, data);
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
    }

    [System.Serializable]
    public struct JoinData
    {
        public string id;
        public List<string> users;
        public string masterId;
        public bool master;
        public bool synced;
        public string scene;
        public string background;
        public List<string> lightingPresets;
    }
}