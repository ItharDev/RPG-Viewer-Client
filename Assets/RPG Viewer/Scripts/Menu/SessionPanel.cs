using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Networking;
using SFB;
using TMPro;
using UnityEngine;

namespace RPG
{
    public class SessionPanel : MonoBehaviour
    {
        [SerializeField] private MenuHandler menu;
        [SerializeField] private GameObject button;

        [SerializeField] private TMP_InputField createInput;
        [SerializeField] private TMP_InputField licenceInput;
        [SerializeField] private TMP_Dropdown joinDropdown;

        private RectTransform rect;
        private List<string> sessions = new List<string>();
        private byte[] landingPage;

        private void OnValidate()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();
        }
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
        }

        private void FetchLicences()
        {
            SocketManager.EmitAsync("load-licences", async (callback) =>
            {
                // check if the event was successfull
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    // Enumerate session array
                    var list = callback.GetValue(1).EnumerateArray();
                    int length = callback.GetValue(1).GetArrayLength();

                    for (int i = 0; i < length; i++)
                    {
                        // Move to next element in array
                        list.MoveNext();

                        // Read session data
                        string id = list.Current.GetProperty("id").GetString();
                        string name = list.Current.GetProperty("name").GetString();

                        // Add session and update dropdown
                        sessions.Add(id);
                        joinDropdown.AddOptions(new List<string>() { name });
                        joinDropdown.RefreshShownValue();
                    }

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }

        public async void SelectImage() => await ImageTask();
        private async Task ImageTask()
        {
            // Only allow image files
            ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };

            // Open file explorer
            StandaloneFileBrowser.OpenFilePanelAsync("Select file", "", extensions, false, (string[] paths) =>
            {
                // Return if no items are selected
                if (paths.Length == 0) return;

                // Read bytes from selected file
                landingPage = File.ReadAllBytes(paths[0]);
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
                // check if the event was successful
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
            string id = sessions[joinDropdown.value];

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
                // check if the event was successful
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
            string id = sessions[joinDropdown.value];

            SocketManager.EmitAsync("join-session", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage($"Joining game session {joinDropdown.captionText.text}");

                    // Read join data
                    JoinData data = JsonUtility.FromJson<JoinData>(callback.GetValue(1).ToString());
                    SessionManager.JoinSession(data);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
    }

    [System.Serializable]
    public struct JoinData
    {
        public string id;
        public string master;
        public List<string> users;
        public bool synced;
        public string scene;
        public string background;
        public List<string> lightingPresets;
    }
}