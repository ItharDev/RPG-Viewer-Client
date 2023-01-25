using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;

namespace RPG
{
    public class MenuPanel : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] private TMP_Text statusText;

        [Header("Address")]
        [SerializeField] private TMP_InputField ipInput;

        [Header("Account")]
        [SerializeField] private GameObject accountHeader;
        [SerializeField] private GameObject accountInfo;
        [SerializeField] private GameObject accountPanel;

        [Header("Sign in")]
        [SerializeField] private TMP_InputField signInEmail;
        [SerializeField] private TMP_InputField signInPassword;

        [Header("Register")]
        [SerializeField] private TMP_InputField RegisterEmail;
        [SerializeField] private TMP_InputField RegisterPassword;
        [SerializeField] private TMP_InputField RegisterVerification;
        [SerializeField] private TMP_InputField RegisterName;

        [Header("Sessions")]
        [SerializeField] private GameObject sessionPanel;
        [SerializeField] private TMP_InputField createName;
        [SerializeField] private TMP_InputField licenceKey;
        [SerializeField] private TMP_Dropdown joinDropdown;

        private readonly List<string> sessions = new List<string>();

        private void Start()
        {
            if (PlayerPrefs.HasKey("Address"))
            {
                var address = PlayerPrefs.GetString("Address");
                SocketManager.Connect(address);
            }
        }
        private void Update()
        {
            if (SocketManager.Socket != null) statusText.text = SocketManager.Socket.Connected ? "Online" : "Offline";
        }

        #region Buttons
        public void Connect()
        {
            var address = ipInput.text;
            SocketManager.Connect(address);
        }

        public async void CreateSession()
        {
            if (!SocketManager.Socket.Connected) return;

            if (string.IsNullOrEmpty(createName.text) || name.StartsWith(" ") || name.EndsWith(" "))
            {
                MessageManager.QueueMessage("Invalid session name");
                return;
            }

            await SocketManager.Socket.EmitAsync("create-session", (callback) =>
            {
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Session created successfully");
                    LoadLicences();
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }

            }, createName.text);
        }
        public void CopyId()
        {
            var id = sessions[joinDropdown.value];
            TextEditor editor = new TextEditor
            {
                text = id
            };
            editor.SelectAll();
            editor.Copy();
            MessageManager.QueueMessage($"Session ID ({id}) copied to clipboard");
        }
        public async void ApplyLicence()
        {
            if (!SocketManager.Socket.Connected) return;

            await SocketManager.Socket.EmitAsync("validate-licence", (callback) =>
            {
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Session key validated");
                    LoadLicences();
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }
            }, licenceKey.text);
        }
        public async void RemoveLicences()
        {
            if (!SocketManager.Socket.Connected) return;

            await SocketManager.Socket.EmitAsync("remove-licences", (callback) =>
            {
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Licences removed successfully");
                    LoadLicences();
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }
            });
        }

        [System.Obsolete]
        public async void JoinSession()
        {
            if (!SocketManager.Socket.Connected) return;

            var id = sessions[joinDropdown.value];

            await SocketManager.Socket.EmitAsync("join-session", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage($"Joining game session {joinDropdown.captionText.text}");

                    var data = JsonUtility.FromJson<JoinData>(callback.GetValue(1).ToString());
                    SessionManager.JoinSession(data);
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());

            }, id);
        }

        public async void Register()
        {
            if (!SocketManager.Socket.Connected) return;
            if (RegisterPassword.text != RegisterVerification.text)
            {
                MessageManager.QueueMessage("Passwords do not match");
                return;
            }

            await SocketManager.Socket.EmitAsync("register", async (callback) =>
            {
                if (callback.GetValue().GetBoolean())
                {
                    await SocketManager.Socket.EmitAsync("sign-in", async (callback) =>
                    {
                        if (callback.GetValue().GetBoolean())
                        {
                            await UniTask.SwitchToMainThread();

                            accountHeader.SetActive(true);
                            accountPanel.SetActive(false);
                            sessionPanel.SetActive(true);

                            RegisterEmail.text = "";
                            RegisterName.text = "";
                            RegisterPassword.text = "";
                            RegisterVerification.text = "";

                            signInEmail.text = "";
                            signInPassword.text = "";

                            accountInfo.GetComponentInChildren<TMP_Text>().text = callback.GetValue(1).GetString();
                            SocketManager.UserId = callback.GetValue(2).GetString();
                            PlayerPrefs.SetString("user id", SocketManager.UserId);

                            MessageManager.QueueMessage($"Registered successfully as {callback.GetValue(1).GetString()}");
                        }
                        else
                        {
                            MessageManager.QueueMessage(callback.GetValue(1).GetString());
                        }
                    }, RegisterEmail.text, RegisterPassword.text, "");
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }
            }, RegisterEmail.text, RegisterName.text, RegisterPassword.text);
        }
        public async void SignIn()
        {
            if (!SocketManager.Socket.Connected) return;

            await SocketManager.Socket.EmitAsync("sign-in", async (callback) =>
            {
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    accountHeader.SetActive(true);
                    accountPanel.SetActive(false);
                    sessionPanel.SetActive(true);

                    RegisterEmail.text = "";
                    RegisterName.text = "";
                    RegisterPassword.text = "";
                    RegisterVerification.text = "";

                    signInEmail.text = "";
                    signInPassword.text = "";

                    accountInfo.GetComponentInChildren<TMP_Text>().text = callback.GetValue(1).GetString();
                    SocketManager.UserId = callback.GetValue(2).GetString();
                    PlayerPrefs.SetString("user id", SocketManager.UserId);

                    MessageManager.QueueMessage($"Signed in as {callback.GetValue(1).GetString()}");

                    LoadLicences();
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }
            }, signInEmail.text, signInPassword.text, "");
        }
        public async void SignOut()
        {
            if (!SocketManager.Socket.Connected) return;

            await SocketManager.Socket.EmitAsync("sign-out", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                MessageManager.QueueMessage($"Signed out");

                accountHeader.SetActive(false);
                accountPanel.SetActive(true);
                sessionPanel.SetActive(false);

                PlayerPrefs.DeleteKey("user id");
                SocketManager.UserId = null;
                accountInfo.GetComponentInChildren<TMP_Text>().text = "";
                licenceKey.text = "";
            });
        }
        #endregion

        public async void LoadLicences()
        {
            await SocketManager.Socket.EmitAsync("load-licences", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    licenceKey.text = "";

                    var list = callback.GetValue(1).EnumerateArray();
                    joinDropdown.ClearOptions();
                    sessions.Clear();

                    for (int i = 0; i < callback.GetValue(1).GetArrayLength(); i++)
                    {
                        list.MoveNext();
                        string id = list.Current.GetProperty("id").GetString();
                        string name = list.Current.GetProperty("name").GetString();
                        sessions.Add(id);
                        joinDropdown.AddOptions(new List<string>() { name });
                        joinDropdown.RefreshShownValue();
                    }
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }
            });
        }

        public async void AutomaticSignIn()
        {
            if (ipInput.text != "") PlayerPrefs.SetString("Address", ipInput.text);
            ipInput.text = "";
            if (PlayerPrefs.HasKey("user id"))
            {
                await SocketManager.Socket.EmitAsync("sign-in", async (callback) =>
                {
                    if (callback.GetValue().GetBoolean())
                    {
                        await UniTask.SwitchToMainThread();

                        accountHeader.SetActive(true);
                        accountPanel.SetActive(false);
                        sessionPanel.SetActive(true);

                        accountInfo.GetComponentInChildren<TMP_Text>().text = callback.GetValue(1).GetString();
                        SocketManager.UserId = callback.GetValue(2).GetString();

                        LoadLicences();
                        MessageManager.QueueMessage($"Signed in as {callback.GetValue(1).GetString()}");
                    }
                    else
                    {
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    }
                }, signInEmail.text, signInPassword.text, PlayerPrefs.GetString("user id"));
            }
        }
    }
}