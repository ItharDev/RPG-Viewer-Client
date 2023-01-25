using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RPG;
using SocketIOClient;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Networking
{
    public class SocketManager : MonoBehaviour
    {
        public static SocketManager Instance { get; private set; }

        public static SocketIOUnity Socket;

        public static SceneSettings SceneSettings;
        public static string UserId;

        public UnityEvent OnConnectedEvent = new UnityEvent();
        public UnityEvent OnDisconnectedEvent = new UnityEvent();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }

        private void OnDisable()
        {
            OnConnectedEvent.RemoveAllListeners();
            OnDisconnectedEvent.RemoveAllListeners();
            if (Socket == null) return;
            Socket.OnDisconnected -= OnDisconnected;
            Socket.OnConnected -= OnConnected;
            Socket.OnReconnectError -= OnReconnect;

            if (Application.isPlaying) Socket.Disconnect();
        }

        private static void OnReconnect(object sender, Exception e)
        {
            MessageManager.QueueMessage("Failed to establish connection to the server. Reconnecting after 30 seconds");
        }

        private static async void OnConnected(object sender, EventArgs e)
        {
            await UniTask.SwitchToMainThread();

            MessageManager.QueueMessage("Connection established to the server");
            Instance.OnConnectedEvent.Invoke();
        }

        private static async void OnDisconnected(object sender, string e)
        {
            await UniTask.SwitchToMainThread();

            MessageManager.QueueMessage("Disconnected from the server. Returning to menu");
            SceneManager.LoadScene("Menu");
            Instance.OnDisconnectedEvent.Invoke();
        }



        public static void Connect(string address)
        {
            if (Socket != null)
            {
                Socket.Disconnect();

                Socket.OnDisconnected -= OnDisconnected;
                Socket.OnConnected -= OnConnected;
                Socket.OnReconnectError -= OnReconnect;
            }

            var uri = new Uri($"http://{address}");
            Socket = new SocketIOUnity(uri, new SocketIOOptions
            {
                Query = new Dictionary<string, string>
                {
                    { "token", "UNITY" }
                },
                EIO = 4,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                ReconnectionDelay = 30000,
                ReconnectionAttempts = 10
            });

            Socket.OnDisconnected += OnDisconnected;
            Socket.OnConnected += OnConnected;
            Socket.OnReconnectError += OnReconnect;

            Socket.Connect();
        }
    }
}
