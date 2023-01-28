using System;
using System.IO;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LogicUI.FancyTextRendering;
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
        public GameObject confirmPanel;

        [SerializeField] private TMP_InputField headerInput;
        [SerializeField] private GameObject publicButton;
        [SerializeField] private GameObject showButton;
        [SerializeField] private GameObject deleteButton;
        [SerializeField] private GameObject selectButton;

        [SerializeField] private GameObject viewButton;
        [SerializeField] private Image viewImage;
        [SerializeField] private Sprite imageSprite;
        [SerializeField] private Sprite textSprite;


        [SerializeField] private TMP_Text hoverText;
        [SerializeField] private MarkdownRenderer markdown;
        [SerializeField] private TMP_InputField markdownInput;
        [SerializeField] private Image image;

        [SerializeField] private GameObject buttonParent;
        [SerializeField] private GameObject content;
        [SerializeField] private RectTransform topPanel;
        [SerializeField] private GameObject resizeIcon;

        public NoteData Data;

        private NoteManager manager;
        private bool dragging;
        private Vector2 startPos;
        private Vector2 openSize;
        private byte[] bytes;
        private bool minimised;

        private void Start()
        {
            markdownInput.onValueChanged.AddListener(UpdateMarkdown);
        }
        private void Update()
        {

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Data.owner != SocketManager.UserId) return;
                if (Input.GetKeyDown(KeyCode.S))
                {
                    DeselectTextBox("");
                    ModifyText();
                }

                if (Input.GetKeyDown(KeyCode.M))
                {
                    SelectTextBox("");
                }
            }
        }

        public void SelectTextBox(string text)
        {
            markdown.gameObject.SetActive(false);
            markdownInput.gameObject.SetActive(true);
            markdownInput.ActivateInputField();
        }
        public void DeselectTextBox(string text)
        {
            markdown.gameObject.SetActive(true);
            markdownInput.gameObject.SetActive(false);
        }
        public void UpdateMarkdown(string text)
        {
            markdown.Source = text;
        }

        public void Select(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button != PointerEventData.InputButton.Left || pointerData.clickCount < 2) return;

            if (minimised) Minimise();
            else
            {
                Panel.SetActive(true);
                Panel.transform.SetParent(GameObject.Find("Main Canvas").transform);
                Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(600.0f, 750.0f);
                Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(660.0f, -165.0f);
                Panel.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                Panel.transform.SetAsLastSibling();
            }
        }

        public void LoadData(NoteData data, NoteManager noteManager)
        {
            Data = data;
            manager = noteManager;
            Data.type = NoteType.Text;

            transform.localPosition = new Vector3(data.position.x, data.position.y, -1);
            if (Data.owner == SocketManager.UserId)
            {
                gameObject.SetActive(true);
                publicButton.SetActive(true);
                deleteButton.SetActive(true);
                showButton.SetActive(true);
            }
            else
            {
                gameObject.SetActive(Data.isPublic);
                publicButton.SetActive(false);
                deleteButton.SetActive(false);
                showButton.SetActive(false);
            }

            headerInput.text = data.header;
            hoverText.text = data.header;

            markdownInput.gameObject.SetActive(false);
            markdown.gameObject.SetActive(true);
            image.gameObject.SetActive(false);
            selectButton.SetActive(false);

            UpdateImage(data.image);

            markdownInput.text = data.text;
            markdown.Source = data.text;

            viewButton.GetComponentInChildren<TMP_Text>(true).text = "View image";
            viewImage.sprite = imageSprite;

            GetComponentInChildren<Canvas>(true).GetComponent<RectTransform>().localScale = new Vector3(SessionManager.session.Settings.grid.cellSize * 0.4f, SessionManager.session.Settings.grid.cellSize * 0.4f, 1.0f);
        }

        public void ChangeView()
        {
            if (Data.type == NoteType.Text)
            {
                Data.type = NoteType.Image;
                viewButton.GetComponentInChildren<TMP_Text>(true).text = "View text";
                viewImage.sprite = textSprite;

                markdownInput.gameObject.SetActive(false);
                markdown.gameObject.SetActive(false);
                if (!string.IsNullOrEmpty(Data.image)) image.gameObject.SetActive(true);
                if (Data.owner == SocketManager.UserId) selectButton.SetActive(true);
            }
            else if (Data.type == NoteType.Image)
            {
                Data.type = NoteType.Text;
                viewButton.GetComponentInChildren<TMP_Text>(true).text = "View image";
                viewImage.sprite = imageSprite;

                markdownInput.gameObject.SetActive(false);
                markdown.gameObject.SetActive(true);
                image.gameObject.SetActive(false);
                selectButton.SetActive(false);
            }
        }

        public void UpdateImage(string id)
        {
            Data.image = id;

            if (string.IsNullOrEmpty(id))
            {
                image.gameObject.SetActive(false);
                return;
            }

            WebManager.Download(id, true, async (bytes) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (bytes != null)
                    {
                        Texture2D texture = new Texture2D(1, 1);
                        texture.LoadImage(bytes);
                        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        image.sprite = sprite;
                        if (Data.type == NoteType.Image) image.gameObject.SetActive(true);
                    }
                });
        }
        public void UpdateText(string text)
        {
            markdownInput.text = text;
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
        public void Show()
        {
            if (minimised) Minimise();
            else
            {
                Panel.SetActive(true);
                Panel.transform.SetParent(GameObject.Find("Main Canvas").transform);
                Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(600.0f, 750.0f);
                Panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(660.0f, -165.0f);
                Panel.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                Panel.transform.SetAsLastSibling();

                if (Data.type == NoteType.Text)
                {
                    markdownInput.gameObject.SetActive(false);
                    markdown.gameObject.SetActive(true);
                }
                else
                {
                    markdown.gameObject.SetActive(false);
                    image.gameObject.SetActive(true);
                }
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
                headerInput.GetComponent<Image>().raycastTarget = true;
                headerInput.interactable = true;
                Panel.GetComponent<RectTransform>().sizeDelta = openSize;
                resizeIcon.SetActive(true);

                if (Data.type == NoteType.Text)
                {
                    markdownInput.gameObject.SetActive(false);
                    markdown.gameObject.SetActive(true);
                }
                else
                {
                    markdown.gameObject.SetActive(false);
                    image.gameObject.SetActive(true);
                }
            }
            else
            {
                openSize = Panel.GetComponent<RectTransform>().sizeDelta;
                content.SetActive(false);
                buttonParent.SetActive(false);
                Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
                headerInput.GetComponent<Image>().raycastTarget = false;
                headerInput.interactable = false;
                resizeIcon.SetActive(false);
            }

            Panel.transform.SetAsLastSibling();
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
            }, Data.id, markdownInput.text);
        }
        public async void SetState()
        {
            await SocketManager.Socket.EmitAsync("set-note-state", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, !Data.isPublic);
        }
        public void Remove()
        {
            confirmPanel.SetActive(true);
            confirmPanel.transform.SetParent(GameObject.Find("Main Canvas").transform);
            confirmPanel.transform.SetAsLastSibling();
            confirmPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            confirmPanel.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
        }
        public async void ConfirmDeletion()
        {
            Destroy(confirmPanel);
            await SocketManager.Socket.EmitAsync("remove-note", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id);
        }
        public async void ShowToPlayers()
        {
            await SocketManager.Socket.EmitAsync("show-note", (callback) =>
            {
                if (callback.GetValue().GetBoolean()) MessageManager.QueueMessage("Note showed to others");
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
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
        public NoteType type;
    }

    [Serializable]
    public enum NoteType
    {
        Text,
        Image
    }
}