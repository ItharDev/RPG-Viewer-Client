using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using LogicUI.FancyTextRendering;
using Networking;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class JournalPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField headerInput;
        [SerializeField] private GameObject showButton;
        [SerializeField] private GameObject selectButton;

        [SerializeField] private GameObject viewButton;
        [SerializeField] private Image viewImage;
        [SerializeField] private Sprite imageSprite;
        [SerializeField] private Sprite textSprite;


        [SerializeField] private MarkdownRenderer markdown;
        [SerializeField] private TMP_InputField markdownInput;
        [SerializeField] private Image image;

        [SerializeField] private GameObject buttonParent;
        [SerializeField] private GameObject content;
        [SerializeField] private RectTransform topPanel;
        [SerializeField] private GameObject resizeIcon;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private JournalManager manager;

        public JournalData Data;
        public Collaborator Collaborator;

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
                if (Data.owner != SocketManager.UserId && !Collaborator.isCollaborator) return;
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

        public void LoadData(JournalData data, JournalManager manager)
        {
            Data = data;
            this.manager = manager;

            var collaborator = Data.collaborators.FirstOrDefault(x => x.user == SocketManager.UserId);
            if (collaborator.user != SocketManager.UserId)
            {
                Collaborator = new Collaborator()
                {
                    user = SocketManager.UserId,
                    isCollaborator = false
                };
            }
            else Collaborator = collaborator;

            if (string.IsNullOrEmpty(Data.text))
            {
                Data.type = string.IsNullOrEmpty(Data.image) ? NoteType.Image : NoteType.Text;
            }
            else Data.type = NoteType.Image;

            if (Data.owner == SocketManager.UserId || Collaborator.isCollaborator)
            {
                showButton.SetActive(true);
            }
            else
            {
                showButton.SetActive(false);
            }

            headerInput.text = data.header;
            if (!collaborator.isCollaborator) headerInput.interactable = false;

            markdownInput.gameObject.SetActive(false);
            markdown.gameObject.SetActive(true);
            image.gameObject.SetActive(false);
            selectButton.SetActive(false);

            UpdateImage(data.image);

            markdownInput.text = data.text;
            markdown.Source = data.text;

            viewButton.GetComponentInChildren<TMP_Text>(true).text = "View image";
            viewImage.sprite = imageSprite;

            ChangeView();

            layoutElement.minHeight = GetComponent<RectTransform>().sizeDelta.y - 60.0f;
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
                if (Data.owner == SocketManager.UserId || Collaborator.isCollaborator) selectButton.SetActive(true);
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

            if (string.IsNullOrEmpty(id)) return;

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
            Data.text = text;
            markdownInput.text = text;
        }
        public void UpdateHeader(string text)
        {
            headerInput.text = text;
        }
        public void Show()
        {
            if (minimised) Minimise();
        }
        public void Close()
        {
            manager.CloseJournal(this);
        }

        public void Minimise()
        {
            if (minimised)
            {
                content.SetActive(true);
                buttonParent.SetActive(true);
                headerInput.GetComponent<Image>().raycastTarget = true;
                headerInput.interactable = true;
                GetComponent<RectTransform>().sizeDelta = openSize;
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
                openSize = GetComponent<RectTransform>().sizeDelta;
                content.SetActive(false);
                buttonParent.SetActive(false);
                GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
                headerInput.GetComponent<Image>().raycastTarget = false;
                headerInput.interactable = false;
                resizeIcon.SetActive(false);
            }

            transform.SetAsLastSibling();
            minimised = !minimised;

            layoutElement.minHeight = GetComponent<RectTransform>().sizeDelta.y - 60.0f;
        }
        public void Resize()
        {
            layoutElement.minHeight = GetComponent<RectTransform>().sizeDelta.y - 60.0f;
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
                await SocketManager.Socket.EmitAsync("modify-journal-image", (callback) =>
                {
                    if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Data.id, Convert.ToBase64String(bytes));

                bytes = null;
            });
            await Task.Yield();
        }

        public async void ModifyHeader()
        {
            await SocketManager.Socket.EmitAsync("modify-journal-header", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, headerInput.text);
        }
        public async void ModifyText()
        {
            await SocketManager.Socket.EmitAsync("modify-journal-text", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, markdownInput.text);
        }
        public async void ShowToPlayers()
        {
            await SocketManager.Socket.EmitAsync("show-journal", (callback) =>
            {
                if (callback.GetValue().GetBoolean()) MessageManager.QueueMessage("Journal page showed to others");
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(Data));
        }
    }
}