using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionInfo Info;
        public static SessionState State;

        public static void JoinSession(JoinData data)
        {
            WebManager.Download(data.background, true, async (bytes) =>
            {
                // Check if landing page exists
                if (bytes == null)
                {
                    MessageManager.QueueMessage("Failed to load landing page, try again");
                    return;
                }

                await UniTask.SwitchToMainThread();

                // Create landing page texture
                Texture2D texture = new Texture2D(1, 1);
                texture.LoadImage(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                // Generate list of users
                List<UserInfo> users = new List<UserInfo>();
                foreach (string id in data.users)
                {
                    UserInfo user = new UserInfo(id, "");
                    users.Add(user);
                }

                // Generate session info and state
                Info = new SessionInfo(data.id, data.master, data.master == GameData.User.id, users, sprite);
                State = new SessionState(data.synced, data.scene);
                SessionState oldState = new SessionState(false, "");

                // Load lighting presets
                LoadPresets(data.lightingPresets);

                // Load session in background
                AsyncOperation Loader = SceneManager.LoadSceneAsync("Session");
                await UniTask.WaitUntil(() => Loader.isDone);

                Events.OnStateChanged?.Invoke(oldState, State);
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
                    // Check if event was successful
                    if (callback.GetValue().GetBoolean())
                    {
                        // TODO: Load preset data and add it to the list
                        // var data = JsonUtility.FromJson<LightPreset>(callback.GetValue(1).ToString());
                        // LightingPresets.AddPreset(id, data);
                    }
                    else
                    {
                        // Send error message
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    }
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
}