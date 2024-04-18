using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LogicUI.FancyTextRendering;
using Networking;
using Nobi.UiRoundedCorners;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class NoteUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RectTransform resize;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private ImageWithIndependentRoundedCorners corners;
        [SerializeField] private ImageWithIndependentRoundedCorners topCorners;

        [Header("Top Panel")]

        [SerializeField] private TMP_InputField headerInput;
        [SerializeField] private TMP_Text header;
        [SerializeField] private GameObject options;
        [SerializeField] private GameObject setPublic;
        [SerializeField] private Image globalIcon;
        [SerializeField] private Sprite globalSprite;
        [SerializeField] private Sprite privateSprite;
        [SerializeField] private GameObject showOthers;

        [Header("Content")]
        [SerializeField] private TMP_InputField textInput;
        [SerializeField] private MarkdownRenderer markdown;
        [SerializeField] private Image image;
        [SerializeField] private GameObject content;
        [SerializeField] private GameObject textContent;
        [SerializeField] private GameObject imageContent;
        [SerializeField] private GameObject chooseImage;

        private RectTransform rect;
        private NoteInfo info;
        private NoteData data;
        private NoteSelection selection;

        private void Awake()
        {
            if (rect == null) rect = (RectTransform)transform;
        }

        public void LoadData(NoteInfo _info, NoteData _data)
        {
            info = _info;
            data = _data;

            textInput.text = data.text;
            headerInput.text = data.header;
            header.text = data.header;
            markdown.Source = data.text;
            image.raycastTarget = info.IsOwner;
            textInput.readOnly = !info.IsOwner;
            header.raycastTarget = info.IsOwner;
            chooseImage.SetActive(info.IsOwner && selection == NoteSelection.Image);
            setPublic.SetActive(info.owner == GameData.User.id);
            showOthers.SetActive(info.IsOwner);
            globalIcon.sprite = info.global ? globalSprite : privateSprite;

            if (string.IsNullOrEmpty(data.image))
            {
                ViewText();
                return;
            }

            WebManager.Download(data.image, true, async (bytes) =>
            {
                // Return if image was not found
                if (bytes == null) return;

                await UniTask.SwitchToMainThread();

                // Generate texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.color = Color.white;
            });

            ViewImage();
        }
        public void ReloadText(string _text, bool reloadRequired)
        {
            data.text = _text;
            markdown.Source = _text;
            if (string.IsNullOrEmpty(_text)) markdown.Source = "*Click here to modify*";
            if (reloadRequired) textInput.text = _text;
        }
        public void ReloadHeader(string _header, bool reloadRequired)
        {
            data.header = _header;
            if (reloadRequired) headerInput.text = _header;
            header.text = _header;
        }
        public void ReloadImage(string _image)
        {
            data.image = _image;
            if (string.IsNullOrEmpty(data.image))
            {
                image.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
                return;
            }

            WebManager.Download(data.image, true, async (bytes) =>
            {
                // Return if image was not found
                if (bytes == null) return;

                await UniTask.SwitchToMainThread();

                // Generate texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                image.color = Color.white;
            });
        }
        public void SetGlobal(bool _isGlobal)
        {
            info.global = _isGlobal;
            image.raycastTarget = info.IsOwner;
            textInput.readOnly = !info.IsOwner;
            header.raycastTarget = info.IsOwner;
            chooseImage.SetActive(info.IsOwner && selection == NoteSelection.Image);
            setPublic.SetActive(info.owner == GameData.User.id);
            showOthers.SetActive(info.IsOwner);
            globalIcon.sprite = info.global ? globalSprite : privateSprite;
            if (!info.IsOwner) Close();
        }

        public void Close()
        {
            Session.Instance.NoteManager.CloseNote(data.id);
            Destroy(gameObject);
        }
        public void ToggleOptions()
        {
            options.SetActive(!options.activeInHierarchy);
        }
        public void ViewImage()
        {
            imageContent.SetActive(true);
            chooseImage.SetActive(info.IsOwner);
            textContent.SetActive(false);
            selection = NoteSelection.Image;
        }
        public void ViewText()
        {
            textContent.SetActive(true);
            imageContent.SetActive(false);
            chooseImage.SetActive(false);
            selection = NoteSelection.Text;
        }
        public async void SelectImage()
        {
            await ImageTask((bytes) =>
            {
                if (bytes == null) return;

                SocketManager.EmitAsync("modify-note-image", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, data.id, Convert.ToBase64String(bytes));
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

        public void UpdateGlobal()
        {
            SocketManager.EmitAsync("set-note-global", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, data.id, !info.global);
        }
        public void ShowNote()
        {
            SocketManager.EmitAsync("show-note", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Note sent to others");
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, data.id);
        }
        public void SaveNote()
        {
            SocketManager.EmitAsync("save-note", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Note saved");
                    FindObjectOfType<JournalsPanel>().LoadJournal(callback.GetValue(1).GetString(), "");
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, data.id);
        }
        public void UpdateText(string text)
        {
            if (text == data.text) return;

            SocketManager.EmitAsync("modify-note-text", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());

                await UniTask.SwitchToMainThread();
                textInput.text = data.text;
                markdown.Source = data.text;
            }, data.id, text);
        }
        public void UpdateHeader(string text)
        {
            if (text == data.header) return;

            SocketManager.EmitAsync("modify-note-header", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());

                header.text = data.header;
                headerInput.text = data.header;
            }, data.id, text);
        }
        public void FinishHeader()
        {
            header.transform.parent.gameObject.SetActive(true);
            headerInput.gameObject.SetActive(false);
        }
        public void FinishText()
        {
            markdown.gameObject.SetActive(true);
            textInput.gameObject.SetActive(false);
        }
        public void ModifyHeader()
        {
            header.transform.parent.gameObject.SetActive(false);
            headerInput.gameObject.SetActive(true);
            headerInput.Select();
        }
        public void ModifyText()
        {
            markdown.gameObject.SetActive(false);
            textInput.gameObject.SetActive(true);
            textInput.Select();
        }

        public void ResizePanel(BaseEventData eventData)
        {
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.position.x < rect.position.x || pointerData.position.y > rect.position.y) return;

            resize.position = new Vector3(pointerData.position.x, pointerData.position.y, 0.0f);
            float height = Mathf.Abs(resize.localPosition.y) + 30.0f;
            float width = Mathf.Abs(resize.localPosition.x) + 5.0f;
            height = Mathf.Clamp(height, 55.0f, Screen.height);
            width = Mathf.Clamp(width, 100.0f, Screen.width);

            rect.sizeDelta = new Vector2(width, height);

            resize.anchoredPosition = new Vector2(-5.0f, 5.0f);
            layoutElement.minHeight = height - 65.0f;

            content.SetActive(height != 55.0f);

            // Refresh corners
            corners.Validate();
            corners.Refresh();
            topCorners.Validate();
            topCorners.Refresh();
        }
    }

    public enum NoteSelection
    {
        Text,
        Image
    }
}