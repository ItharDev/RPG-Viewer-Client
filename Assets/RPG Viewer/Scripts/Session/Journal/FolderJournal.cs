using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class FolderJournal : MonoBehaviour
    {
        [Header("Image")]
        [SerializeField] private Image image;
        [SerializeField] private Sprite folderClose;
        [SerializeField] private Sprite folderOpen;

        [Header("Buttons")]
        [SerializeField] private GameObject folderButton;
        [SerializeField] private GameObject journalButton;
        [SerializeField] private GameObject deleteButton;


        [SerializeField] private TMP_InputField input;

        private MasterPanel masterPanel;

        public Transform Content;
        private Transform startTransform;

        public RectTransform BackgroundRect;

        private string path;
        private string folderName;

        public string Path { get { return path; } }

        private void Start()
        {
            input.GetComponentInChildren<TMP_SelectionCaret>().raycastTarget = false;
        }

        private void Update()
        {
            GetComponent<RectTransform>().sizeDelta = new Vector2(300, (Content.gameObject.activeInHierarchy ? Content.GetComponent<RectTransform>().sizeDelta.y : 0) + 50);

            if (Path == "shared")
            {
                folderButton.SetActive(false);
                journalButton.SetActive(false);
                deleteButton.SetActive(false);

                return;
            }

            folderButton.SetActive(RectTransformUtility.RectangleContainsScreenPoint(BackgroundRect, Input.mousePosition));
            journalButton.SetActive(RectTransformUtility.RectangleContainsScreenPoint(BackgroundRect, Input.mousePosition));
            deleteButton.SetActive(RectTransformUtility.RectangleContainsScreenPoint(BackgroundRect, Input.mousePosition));
        }

        public void LoadData(string _name, string _path, MasterPanel _masterPanel)
        {
            input.text = _name;
            folderName = _name;
            path = _path;
            masterPanel = _masterPanel;
        }
        public void UpdateChildren()
        {
            var folders = GetComponentsInChildren<FolderJournal>(true);
            var journals = GetComponentsInChildren<JournalHolder>(true);

            for (int i = 0; i < journals.Length; i++)
            {
                if (journals[i].transform.parent == Content) journals[i].UpdatePath(Path);
            }
            for (int i = 0; i < folders.Length; i++)
            {
                if (folders[i].transform.parent == Content) folders[i].UpdatePath(Path);
            }
        }
        public void UpdatePath(string newPath)
        {
            var id = Path.Split("/").Last();
            path = $"{newPath}/{id}";
            UpdateChildren();
        }

        public void PointerClick(BaseEventData data)
        {
            var pointerData = data as PointerEventData;
            if (pointerData.dragging) return;

            if (pointerData.clickCount == 2)
            {
                if (Path == "shared") return;

                input.Select();
                return;
            }

            Content.gameObject.SetActive(!Content.gameObject.activeInHierarchy);
            image.sprite = Content.gameObject.activeInHierarchy ? folderOpen : folderClose;
        }
        public void BeginDrag()
        {
            if (startTransform != null || Path == "shared") return;

            if (Content.gameObject.activeInHierarchy)
            {
                Content.gameObject.SetActive(!Content.gameObject.activeInHierarchy);
                image.sprite = Content.gameObject.activeInHierarchy ? folderOpen : folderClose;
            }

            startTransform = transform.parent;
            transform.SetParent(GameObject.Find("Main Canvas").transform);
            transform.SetAsLastSibling();
            GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }
        public async void EndDrag()
        {
            if (Path == "shared") return;

            bool moved = false;
            var dictionaries = FindObjectsOfType<FolderJournal>();

            for (int i = 0; i < dictionaries.Length; i++)
            {
                if (dictionaries[i] != this && dictionaries[i].Path != "shared")
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(dictionaries[i].BackgroundRect, Input.mousePosition))
                    {
                        moved = true;
                        var newPath = dictionaries[i].Path;
                        var content = dictionaries[i].Content;

                        await SocketManager.Socket.EmitAsync("move-journal-folder", async (callback) =>
                        {
                            await UniTask.SwitchToMainThread();
                            startTransform = null;

                            if (callback.GetValue().GetBoolean())
                            {
                                var id = Path.Split("/").Last();

                                transform.SetParent(content);
                                transform.SetAsFirstSibling();

                                path = $"{newPath}/{id}";

                                UpdateChildren();
                                startTransform = null;
                            }
                            else
                            {
                                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                            }
                        }, path, newPath);
                    }
                }
            }

            if (!moved && RectTransformUtility.RectangleContainsScreenPoint(masterPanel.JournalPanel.GetComponent<RectTransform>(), Input.mousePosition) && Path.Split("/").Length != 1)
            {
                await SocketManager.Socket.EmitAsync("move-journal-folder", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    startTransform = null;

                    if (callback.GetValue().GetBoolean())
                    {
                        var id = Path.Split("/").Last();

                        transform.SetParent(masterPanel.JournalList);
                        transform.SetAsFirstSibling();

                        path = id;
                        UpdateChildren();
                    }
                    else
                    {
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    }
                }, path, "");
            }
            else if (!moved)
            {
                transform.SetParent(startTransform);
                startTransform = null;
            }
        }
        public void Drag()
        {
            if (Path == "shared") return;
            transform.position = Input.mousePosition;
        }

        public void CreateFolder()
        {
            masterPanel.OpenJournalFolder(Path);
        }
        public void CreateJournal()
        {
            masterPanel.OpenJournalCreation(Path);
        }
        public async void RenameFolder()
        {
            if (string.IsNullOrEmpty(input.text)) input.text = "New folder";

            await SocketManager.Socket.EmitAsync("rename-journal-folder", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean()) folderName = input.text;
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    input.text = folderName;
                }
            }, Path, input.text);
        }
        public void DeleteFolder()
        {
            masterPanel.ConfirmDeletion((value) =>
            {
                if (value) masterPanel.RemoveJournalFolder(path);
            });
        }
    }
}