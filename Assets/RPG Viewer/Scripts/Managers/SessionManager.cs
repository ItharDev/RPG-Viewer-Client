using System.Collections.Generic;
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

        public static bool Synced;
        public static string Scene;
        public static List<string> Users = new List<string>();
        public static Sprite BackgroundSprite;

        public static Session session;

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
        private void Start()
        {
            if (SocketManager.Socket == null) return;

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
                if (Synced || IsMaster) session.LoadScene(Scene);
            });
            SocketManager.Socket.On("change-state", async (data) =>
            {
                await UniTask.SwitchToMainThread();

                Synced = data.GetValue().GetBoolean();
                Scene = data.GetValue(1).GetString();

                SocketManager.Socket.Emit("update-scene", Scene);

                if (Synced && !string.IsNullOrEmpty(Scene)) session.LoadScene(Scene);
                else session.UnloadScene();
            });
            SocketManager.Socket.On("toggle-door", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.ToggleDoor(data.GetValue().GetString(), data.GetValue(1).GetBoolean());
            });
            SocketManager.Socket.On("modify-door", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.ModifyWall(JsonUtility.FromJson<WallData>(data.GetValue().GetString()));
            });
            SocketManager.Socket.On("create-light", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.CreateLight(JsonUtility.FromJson<LightData>(data.GetValue().GetString()));
            }); SocketManager.Socket.On("modify-light", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.ModifyLight(JsonUtility.FromJson<LightData>(data.GetValue().GetString()));
            }); SocketManager.Socket.On("remove-light", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.RemoveLight(data.GetValue().GetString());
            });
            SocketManager.Socket.On("create-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var token = JsonUtility.FromJson<TokenData>(data.GetValue().ToString());
                token.id = data.GetValue(1).GetString();
                if (session != null) session.CreateToken(token);
            });
            SocketManager.Socket.On("move-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                MovementData movement = JsonUtility.FromJson<MovementData>(data.GetValue(1).GetString());
                if (session != null) session.MoveToken(data.GetValue().GetString(), movement);
            });
            SocketManager.Socket.On("modify-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.ModifyToken(data.GetValue().GetString(), JsonUtility.FromJson<TokenData>(data.GetValue(1).ToString()));
            });
            SocketManager.Socket.On("remove-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.RemoveToken(data.GetValue().GetString());
            });
            SocketManager.Socket.On("update-visibility", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.UpdateVisibility(data.GetValue(0).GetString(), data.GetValue(1).GetBoolean());
            });
            SocketManager.Socket.On("update-conditions", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.UpdateConditions(data.GetValue().GetString(), data.GetValue(1).GetInt32());
            });
            SocketManager.Socket.On("lock-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.LockToken(data.GetValue().GetString(), data.GetValue(1).GetBoolean());
            });
            SocketManager.Socket.On("update-health", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.UpdateHealth(data.GetValue().GetString(), data.GetValue(1).GetInt32());
            });
            SocketManager.Socket.On("update-elevation", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.UpdateElevation(data.GetValue().GetString(), data.GetValue(1).GetString());
            });
            SocketManager.Socket.On("rotate-token", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                if (session != null) session.RotateToken(data.GetValue().GetString(), (float)data.GetValue(1).GetDouble());
            });

            SocketManager.Socket.On("create-note", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var note = JsonUtility.FromJson<NoteData>(data.GetValue().ToString());
                note.id = data.GetValue(1).GetString();
                if (session != null) session.CreateNote(note);
            });
            SocketManager.Socket.On("modify-note-text", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var text = data.GetValue(1).GetString();
                if (session != null) session.ModifyNoteText(id, text);
            });
            SocketManager.Socket.On("modify-note-image", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var image = data.GetValue(1).GetString();
                if (session != null) session.ModifyNoteImage(id, image);
            });
            SocketManager.Socket.On("set-note-state", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                var state = data.GetValue(1).GetBoolean();
                if (session != null) session.SetNotePublic(id, state);
            });
            SocketManager.Socket.On("remove-note", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var id = data.GetValue().GetString();
                if (session != null) session.RemoveNote(id);
            });
        }
        private void Update()
        {
            if (session == null) session = FindObjectOfType<Session>();
        }

        [System.Obsolete]
        public static void JoinSession(JoinData data)
        {
            WebManager.Download(data.background, true, async (bytes) =>
            {
                await UniTask.SwitchToMainThread();

                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                BackgroundSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                var Loader = SceneManager.LoadSceneAsync("Session");
                await UniTask.WaitUntil(() => Loader.isDone);

                IsMaster = data.master;
                Synced = data.synced;
                Scene = data.scene;
                Users = data.users;

                if (session == null) session = FindObjectOfType<Session>();
                if (!IsMaster && !Synced) return;

                if (!string.IsNullOrEmpty(data.scene))
                {
                    SocketManager.Socket.Emit("update-scene", Scene);
                    session.LoadScene(data.scene);
                }
            });
        }
    }

    [System.Serializable]
    public struct JoinData
    {
        public string id;
        public List<string> users;
        public bool master;
        public bool synced;
        public string scene;
        public string background;
    }
}