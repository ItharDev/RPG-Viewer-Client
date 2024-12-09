using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class SessionPanel : MonoBehaviour
    {
        [SerializeField] private Transform sessionList;
        [SerializeField] private SessionCard sessionPrefab;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnDisconnected.AddListener(SignOut);
            Events.OnSignIn.AddListener(SignIn);
            Events.OnSignOut.AddListener(SignOut);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnDisconnected.RemoveListener(SignOut);
            Events.OnSignIn.RemoveListener(SignIn);
            Events.OnSignOut.RemoveListener(SignOut);
        }

        private void SignIn(string id, string name)
        {
            // Fetch licences
            ClearSessions();
            FetchSessions();
        }
        private void SignOut()
        {
            ClearSessions();
        }

        private void FetchSessions()
        {
            SocketManager.EmitAsync("load-licences", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Enumerate session array
                    var list = callback.GetValue(1).EnumerateArray().ToArray();

                    for (int i = 0; i < list.Length; i++)
                    {
                        // Read session data
                        string id = list[i].GetProperty("id").GetString();
                        string name = list[i].GetProperty("name").GetString();
                        string landingPage = list[i].GetProperty("landingPage").GetString();

                        LoadSesionCard(id, name, landingPage);
                    }

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            });
        }

        private void ClearSessions()
        {
            // Clear all session cards
            foreach (Transform child in sessionList)
            {
                Destroy(child.gameObject);
            }
        }

        private void LoadSesionCard(string id, string name, string landingPage)
        {
            // Create session card
            SessionCard card = Instantiate(sessionPrefab, sessionList);
            card.SetData(id, name, landingPage);
        }

        public void JoinSession(string id)
        {
            SocketManager.EmitAsync("join-session", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage($"Joining Game Session");

                    // Read join data
                    JoinData data = JsonUtility.FromJson<JoinData>(callback.GetValue(1).ToString());
                    ConnectionManager.JoinSession(data);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, id);
        }
    }
}