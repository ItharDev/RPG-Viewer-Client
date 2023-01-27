using System;
using System.IO;
using System.Resources;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Networking;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class NoteHolder : MonoBehaviour
    {
        public GameObject Panel;
        [SerializeField] private TMP_InputField headerInput;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_InputField textInput;
        [SerializeField] private GameObject publicButton;
        [SerializeField] private GameObject removeButton;
        [SerializeField] private GameObject deleteButton;
        [SerializeField] private TMP_Text hoverText;
        [SerializeField] private Image image;

        [SerializeField] private GameObject buttonParent;
        [SerializeField] private GameObject content;
        [SerializeField] private RectTransform topPanel;

        public bool Selected;
        public NoteData Data;

        private NoteManager manager;
        private bool dragging;
        private Vector2 startPos;
        private byte[] bytes;
        private bool minimised;

        public void Select(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button != PointerEventData.InputButton.Left || pointerData.clickCount < 2) return;
            manager.SelectNote(this);

            Panel.SetActive(true);
            Panel.transform.SetParent(GameObject.Find("Main Canvas").transform);
            Panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            Panel.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            Panel.transform.SetAsLastSibling();

            Selected = true;
        }

        public void Deselect()
        {
            Selected = false;
            Panel.SetActive(false);
        }

        public void LoadData(NoteData data, NoteManager noteManager)
        {
            Data = data;
            manager = noteManager;

            transform.localPosition = new Vector3(data.position.x, data.position.y, -1);
            if (Data.owner == SocketManager.UserId)
            {
                gameObject.SetActive(true);
                publicButton.SetActive(true);
                deleteButton.SetActive(true);
            }
            else
            {
                gameObject.SetActive(Data.isPublic);
                publicButton.SetActive(false);
                deleteButton.SetActive(false);
            }

            UpdateImage(data.image);

            textInput.text = data.text;
            headerInput.text = data.header;
            hoverText.text = data.header;
        }

        public void UpdateImage(string id)
        {
            Data.image = id;
            if (string.IsNullOrEmpty(id))
            {
                image.enabled = false;
                Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(600.0f, 400.0f);
                removeButton.SetActive(false);
            }
            else
            {
                WebManager.Download(id, true, async (bytes) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (bytes != null)
                    {
                        Texture2D texture = new Texture2D(1, 1);
                        texture.LoadImage(bytes);
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        image.sprite = sprite;
                        image.enabled = true;
                        Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(600.0f, 730.0f);
                        removeButton.SetActive(true);
                    }
                });
            }
        }
        public void UpdateText(string text)
        {
            textInput.text = text;
        }
        public void UpdateHeader(string text)
        {
            headerInput.text = text;
            hoverText.text = text;
        }
        public void SetPublic(bool isPublic)
        {
            Data.isPublic = isPublic;
            publicButton.GetComponentInChildren<TMP_Text>(true).text = isPublic ? "Set as private" : "Set as public";

            if (Data.owner == SocketManager.UserId)
            {
                gameObject.SetActive(true);
                publicButton.SetActive(true);
            }
            else
            {
                gameObject.SetActive(Data.isPublic);
                publicButton.SetActive(false);
            }
        }

        public void BeginDrag(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;
            startPos = transform.position;
            dragging = true;
        }
        public void Drag(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (!dragging) return;


            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
            Data.position = transform.position;
        }
        public async void EndDrag(BaseEventData eventData)
        {
            dragging = false;
            Data.position = transform.position;

            await SocketManager.Socket.EmitAsync("move-note", (callback) =>
            {
                if (!callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    Data.position = startPos;
                    transform.localPosition = new Vector3(Data.position.x, Data.position.y, -1);
                }
            }, Data.id, JsonUtility.ToJson(Data.position));
        }

        public void Minimise()
        {
            if (minimised)
            {
                content.SetActive(true);
                buttonParent.SetActive(true);
                topPanel.sizeDelta = new Vector2(0, 30);
                headerInput.GetComponent<Image>().raycastTarget = true;
                headerInput.interactable = true;
                headerText.raycastTarget = true;
                headerText.horizontalAlignment = HorizontalAlignmentOptions.Left;
                Panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                content.SetActive(false);
                buttonParent.SetActive(false);
                topPanel.sizeDelta = new Vector2(-450, 30);
                headerInput.GetComponent<Image>().raycastTarget = false;
                headerInput.interactable = false;
                headerText.raycastTarget = false;
                headerText.horizontalAlignment = HorizontalAlignmentOptions.Center;
            }

            minimised = !minimised;
        }

        public async void SelectImage() => await ImageTask();
        private async Task ImageTask()
        {
            var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
            StandaloneFileBrowser.OpenFilePanelAsync("Select file", "", extensions, false, async (string[] paths) =>
            {
                if (paths.Length == 0) return;
                bytes = File.ReadAllBytes(paths[0]);

                MessageManager.QueueMessage("Uploading image");
                await SocketManager.Socket.EmitAsync("modify-note-image", (callback) =>
                {
                    if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Data.id, Convert.ToBase64String(bytes));

                bytes = null;
            });
            await Task.Yield();
        }

        public async void RemoveImage()
        {
            await SocketManager.Socket.EmitAsync("modify-note-image", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, null);
        }
        public async void ModifyHeader()
        {
            await SocketManager.Socket.EmitAsync("modify-note-header", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, headerInput.text);
        }
        public async void ModifyText()
        {
            await SocketManager.Socket.EmitAsync("modify-note-text", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, textInput.text);
        }
        public async void SetState()
        {
            await SocketManager.Socket.EmitAsync("set-note-state", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, !Data.isPublic);
        }
        public async void Remove()
        {
            await SocketManager.Socket.EmitAsync("remove-note", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id);
        }
    }

    [Serializable]
    public struct NoteData
    {
        public string id;
        public string owner;
        public string header;
        public string text;
        public string image;
        public bool isPublic;
        public Vector2 position;
    }
}