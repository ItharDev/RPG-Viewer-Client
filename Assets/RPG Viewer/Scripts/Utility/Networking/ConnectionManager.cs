using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG
{
    public class ConnectionManager : MonoBehaviour
    {
        public static SessionInfo Info;
        public static SessionState State;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        private void OnEnable()
        {
            Events.OnStateChanged.AddListener(SetState);
        }
        private void OnDisable()
        {
            Events.OnStateChanged.RemoveListener(SetState);
        }

        private void SetState(SessionState oldState, SessionState newState)
        {
            State = newState;
        }

        public static void JoinSession(JoinData data)
        {
            WebManager.Download(data.background, true, async (bytes) =>
            {
                // Check if landing page exists
                if (bytes == null)
                {
                    MessageManager.QueueMessage("Failed to load landing page, please try again");
                    return;
                }

                await UniTask.SwitchToMainThread();

                // Create landing page texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                // Generate list of users
                List<UserInfo> users = new List<UserInfo>();
                foreach (string id in data.users)
                {
                    UserInfo user = new UserInfo(id, "");
                    users.Add(user);
                }

                PresetManager.Instance.LoadPresets(data.presets);

                // Generate session info and state
                Info = new SessionInfo(data.id, data.master, data.isMaster, users, sprite);
                State = new SessionState(data.synced, "");
                SessionState oldState = new SessionState(false, "");

                // Load session in background
                AsyncOperation Loader = SceneManager.LoadSceneAsync("Session");
                await UniTask.WaitUntil(() => Loader.isDone);

                Events.OnStateChanged?.Invoke(oldState, State);
                PlayerPrefs.SetString($"{GameData.User.id}_last_session", data.id);
            });
        }

        private static void LoadPresets(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                // Store current iterations value
                string id = list[i];
                SocketManager.EmitAsync("load-preset", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean())
                    {
                        // TODO: Load preset data and add it to the list
                        // var data = JsonUtility.FromJson<LightPreset>(callback.GetValue(1).ToString());
                        // LightingPresets.AddPreset(id, data);
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, list[i]);
            }
        }
    }

    public struct SessionInfo
    {
        public string id;
        public string master;
        public bool isMaster;
        public List<UserInfo> users;
        public Sprite background;

        public SessionInfo(string _id, string _master, bool _isMaster, List<UserInfo> _users, Sprite _background)
        {
            id = _id;
            master = _master;
            isMaster = _isMaster;
            users = _users;
            background = _background;
        }
    }

    public struct SessionState
    {
        public bool synced;
        public string scene;

        public SessionState(bool _synced, string _scene)
        {
            synced = _synced;
            scene = _scene;
        }
    }

    [System.Serializable]
    public struct JoinData
    {
        public string id;
        public string master;
        public bool isMaster;
        public bool synced;
        public string scene;
        public List<string> users;
        public List<string> presets;
        public string background;
    }
}