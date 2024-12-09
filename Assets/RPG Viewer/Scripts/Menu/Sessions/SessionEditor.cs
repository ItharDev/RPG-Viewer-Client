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

        public void OpenPanel()
        {
            UICanvas.Instance.OpenPanel(transform);
        }

        public void ClosePanel()
        {
            UICanvas.Instance.ClosePanel(transform);
        }

        public void LoadData(string id, Sprite landingPageSprite)
        {
            // Fetch session data
            SocketManager.EmitAsync("load-session", async (callback) =>
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
                        string name = invites[i].GetProperty("name").GetString();
                        string id = invites[i].GetProperty("id").GetString();
                        string status = invites[i].GetProperty("status").GetString();

                        InviteHolder inviteHolder = Instantiate(invitePrefab, inviteList);
                        inviteHolder.SetData(id, name, status, true, this);
                        inviteHolders.Add(inviteHolder);
                    }

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, id);
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

            // Reset Input
            landingPageBytes = null;
            landingPage.sprite = defaultLandingPage;
            nameInput.text = "";
            inviteInput.text = "";
            title.text = "New Session";
            inviteHolders.Clear();

            foreach (Transform child in inviteList)
            {
                Destroy(child.gameObject);
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