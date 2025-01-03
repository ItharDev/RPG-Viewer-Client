using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Networking;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class SessionPanel : MonoBehaviour
    {
        [SerializeField] private MenuHandler menu;
        [SerializeField] private GameObject button;

        [Space]
        [SerializeField] private TMP_InputField createInput;
        [SerializeField] private TMP_InputField licenceInput;
        [SerializeField] private TMP_Dropdown joinDropdown;

        [Space]
        [SerializeField] private Image continueImage;
        [SerializeField] private GameObject continuePanel;
        [SerializeField] private RectTransform header;

        private RectTransform rect;
        private Dictionary<string, string> sessions = new Dictionary<string, string>();
        private byte[] landingPage;

        private void OnEnable()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();

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

        public void OpenPanel()
        {
            // Close panel if it's open
            if (rect.sizeDelta.x != 0)
            {
                ClosePanel();
                return;
            }

            // Open panel
            menu.OpenSessions?.Invoke();
            LeanTween.size(rect, new Vector2(470.0f, 190.0f), 0.2f);
        }
        public void ClosePanel()
        {
            // Close panel
            LeanTween.size(rect, new Vector2(0.0f, 0.0f), 0.2f);
        }

        private void SignIn(string id, string name)
        {
            // Fetch licences
            FetchLicences();
            button.SetActive(true);
        }
        private void SignOut()
        {
            // Clear possible old sessions
            joinDropdown.ClearOptions();
            sessions.Clear();
            button.SetActive(false);
            continuePanel.SetActive(false);
        }

        private void FetchLicences()
        {
            // Read last session data
            string lastSession = PlayerPrefs.GetString($"{GameData.User.id}_last_session");

            SocketManager.EmitAsync("load-licences", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    joinDropdown.ClearOptions();
                    sessions.Clear();

                    // Enumerate session array
                    var list = callback.GetValue(1).EnumerateArray().ToArray();

                    for (int i = 0; i < list.Length; i++)
                    {
                        // Read session data
                        string id = list[i].GetProperty("id").GetString();
                        string name = list[i].GetProperty("name").GetString();
                        string landingPage = list[i].GetProperty("landingPage").GetString();

                        // Add session and update dropdown
                        sessions.Add(id, name);
                        joinDropdown.AddOptions(new List<string>() { name });
                        joinDropdown.RefreshShownValue();

                        if (id == lastSession) LoadLastSession(landingPage);
                    }

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }

        private void LoadLastSession(string landingPage)
        {
            WebManager.Download(landingPage, true, async (bytes) =>
            {
                await UniTask.SwitchToMainThread();

                header.anchoredPosition = new Vector2(0.0f, bytes == null ? 0.0f : 300.0f);

                // Return if image was not found
                if (bytes == null) return;

                // Create and apply texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                continueImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                continuePanel.SetActive(true);
            });
        }

        public async void SelectImage()
        {
            await ImageTask((bytes) =>
            {
                if (bytes != null) landingPage = bytes;
            });
        }
        private async Task ImageTask(Action<byte[]> callback)
        {
            // Only allow image files
            ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg", "webp") };

            // Open file explorer
            StandaloneFileBrowser.OpenFilePanelAsync("Select file", "", extensions, false, (string[] paths) =>
            {
                // Return if no items are selected
                if (paths.Length == 0) callback(null);

                // Read bytes from selected file
                callback(File.ReadAllBytes(paths[0]));
            });
            await Task.Yield();
        }

        public void CreateSession()
        {
            // Check if session name is valid
            if (string.IsNullOrEmpty(createInput.text))
            {
                MessageManager.QueueMessage("Invalid session name");
                return;
            }

            // Check if langing page has been selected
            if (landingPage == null)
            {
                MessageManager.QueueMessage("No landing page selected");
                return;
            }

            SocketManager.EmitAsync("create-session", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Session created successfully");

                    // Fetch new session data
                    FetchLicences();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, createInput.text, Convert.ToBase64String(landingPage));

            // Reset landing page selection
            landingPage = null;
        }
        public void CopyId()
        {
            // Return if no session is selected
            if (joinDropdown.options.Count == 0) return;

            // Read current session's id
            string id = sessions.Keys.ElementAt(joinDropdown.value);

            // Create new text editor
            TextEditor editor = new TextEditor
            {
                text = id
            };

            // Select all and copy the session id
            editor.SelectAll();
            editor.Copy();

            MessageManager.QueueMessage($"Session ID ({id}) copied to clipboard");
        }
        public void ApplyLicence()
        {
            // Check if given key is valid
            if (string.IsNullOrEmpty(licenceInput.text)) return;

            SocketManager.EmitAsync("validate-licence", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Session key validated");

                    // Reset licence input
                    licenceInput.text = "";

                    // Fetch new session data
                    FetchLicences();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, licenceInput.text);
        }
        public void RemoveLicences()
        {
            SocketManager.EmitAsync("remove-licences", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue(0).GetBoolean())
                {
                    MessageManager.QueueMessage("Licences removed successfully");

                    // Fetch new session data
                    FetchLicences();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        public void JoinSession()
        {
            // Return if no session is selected
            if (joinDropdown.options.Count == 0) return;

            // Read current session's id
            string id = sessions.Keys.ElementAt(joinDropdown.value);

            SocketManager.EmitAsync("join-session", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage($"Connecting to {joinDropdown.captionText.text}");

                    // Read join data
                    JoinData data = JsonUtility.FromJson<JoinData>(callback.GetValue(1).ToString());
                    ConnectionManager.JoinSession(data);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
        public void ContinueSession()
        {
            // Return if last session data was not found
            string id = PlayerPrefs.GetString($"{GameData.User.id}_last_session");
            if (string.IsNullOrEmpty(id)) return;

            // Check if are invited to that session
            if (!sessions.ContainsKey(id)) return;

            SocketManager.EmitAsync("join-session", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage($"Connecting to {sessions[id]}");

                    // Read join data
                    JoinData data = JsonUtility.FromJson<JoinData>(callback.GetValue(1).ToString());
                    ConnectionManager.JoinSession(data);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
    }
}