using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Networking;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class SessionEditor : MonoBehaviour
    {
        [Header("Top Panel")]
        [SerializeField] private Image landingPage;
        [SerializeField] private Sprite defaultLandingPage;
        [SerializeField] private TMP_Text title;

        [Header("Input")]
        [SerializeField] private TMP_InputField nameInput;

        [Space]
        [SerializeField] private Transform inviteList;
        [SerializeField] private TMP_InputField inviteInput;
        [SerializeField] private InviteHolder invitePrefab;

        [Space]
        [SerializeField] private TMP_Text acceptText;

        private List<InviteHolder> inviteHolders = new List<InviteHolder>();
        private byte[] landingPageBytes;
        private string id;

        public void OpenPanel()
        {
            Reset();
            UICanvas.Instance.OpenPanel(transform);
        }

        public void ClosePanel()
        {
            UICanvas.Instance.ClosePanel(transform);
            Reset();
        }

        public void LoadData(string _id, Sprite landingPageSprite)
        {
            id = _id;
            acceptText.text = "Save Changes";

            // Fetch session data
            SocketManager.EmitAsync("get-session", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Set session data
                    nameInput.text = callback.GetValue(1).GetProperty("name").GetString();
                    title.text = callback.GetValue(1).GetProperty("name").GetString();

                    landingPage.sprite = landingPageSprite;
                    landingPageBytes = landingPageSprite.texture.GetRawTextureData();

                    // Set invite list
                    var invites = callback.GetValue(1).GetProperty("invites").EnumerateArray().ToArray();
                    for (int i = 0; i < invites.Length; i++)
                    {
                        LoadInvite(invites[i].GetString());
                    }

                    // Set players list
                    var players = callback.GetValue(1).GetProperty("users").EnumerateArray().ToArray();
                    for (int i = 0; i < players.Length; i++)
                    {
                        if (players[i].GetString() == GameData.User.id) continue;
                        LoadInvite(players[i].GetString(), true);
                    }

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, id);
        }

        private void Reset()
        {
            // Reset Input
            id = null;
            landingPageBytes = null;

            landingPage.sprite = defaultLandingPage;
            nameInput.text = "";
            inviteInput.text = "";
            title.text = "New Session";
            acceptText.text = "Create Session";
            inviteHolders.Clear();

            foreach (Transform child in inviteList)
            {
                Destroy(child.gameObject);
            }
        }

        private void LoadInvite(string userId, bool hasAccepted = false)
        {
            SocketManager.EmitAsync("get-profile", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    string name = callback.GetValue(1).GetString();
                    string email = callback.GetValue(2).GetString();

                    InviteHolder inviteHolder = Instantiate(invitePrefab, inviteList);
                    inviteHolder.SetData(userId, email, name, hasAccepted, false, this);
                    inviteHolders.Add(inviteHolder);

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, userId);
        }

        private void CreateInvite(string userId)
        {
            if (userId == GameData.User.id)
            {
                MessageManager.QueueMessage("You cannot invite yourself", MessageType.Error);
                return;
            }

            SocketManager.EmitAsync("get-profile", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    string name = callback.GetValue(1).GetString();
                    string email = callback.GetValue(2).GetString();

                    InviteHolder inviteHolder = Instantiate(invitePrefab, inviteList);
                    inviteHolder.SetData(userId, email, name, false, true, this);
                    inviteHolders.Add(inviteHolder);

                    MessageManager.QueueMessage("Invite added");
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, userId);
        }

        public void AddInvite()
        {
            // Check if invite input is empty
            if (string.IsNullOrEmpty(inviteInput.text))
            {
                MessageManager.QueueMessage("Invalid email address", MessageType.Error);
                return;
            }

            // Check if invite email is already in the list
            if (inviteHolders.Any(x => x.GetEmail == inviteInput.text))
            {
                MessageManager.QueueMessage("User already added", MessageType.Error);
                return;
            }

            SocketManager.EmitAsync("get-user-id", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    string userId = callback.GetValue(1).GetString();
                    CreateInvite(userId);
                    inviteInput.text = "";
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, inviteInput.text);
        }

        public void CancelInvite(InviteHolder inviteHolder)
        {
            inviteHolders.Remove(inviteHolder);
            Destroy(inviteHolder.gameObject);
        }

        public void CreateSession()
        {
            // Check if session name is valid
            if (string.IsNullOrEmpty(nameInput.text))
            {
                MessageManager.QueueMessage("Invalid session name", MessageType.Error);
                return;
            }

            // Check if langing page has been selected
            if (landingPage == null)
            {
                MessageManager.QueueMessage("No landing page selected", MessageType.Error);
                return;
            }
        }

        public async void SelectImage()
        {
            await ImageTask(async (bytes) =>
            {
                if (bytes == null) return;

                // Set landing page image
                await UniTask.SwitchToMainThread();

                // Create landing page texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                landingPage.sprite = sprite;
                landingPageBytes = bytes;
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
    }
}