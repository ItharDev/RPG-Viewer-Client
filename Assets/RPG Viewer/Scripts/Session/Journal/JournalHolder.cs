using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class JournalHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        [SerializeField] private GameObject panel;
        [SerializeField] private CollaboratorPanel collaboratorPanel;
        [SerializeField] private GameObject dragPanel;

        [SerializeField] private Color normalColor;
        [SerializeField] private Color activeColor;

        public JournalData Data;
        private string id;
        private string path;

        private MasterPanel masterPanel;
        private GameObject dragObject;
        private JournalManager manager;
        private Transform startTransform;

        public string Id { get { return id; } }

        private void Update()
        {
            text.color = RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition) ? activeColor : normalColor;

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition))
                {
                    panel.SetActive(false);
                }
            }
        }

        public async void LoadData(JournalData _data, string _id, string _folder, MasterPanel _masterPanel, JournalManager manager)
        {
            Data = _data;
            this.manager = manager;
            id = _id;
            path = _folder;
            text.text = _data.header;
            masterPanel = _masterPanel;

            if (Data.owner != SocketManager.UserId) await SocketManager.Socket.EmitAsync("get-user", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean()) text.text += $" ({callback.GetValue(1).GetProperty("name").GetString()})";
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.owner);
        }
        public void UpdatePath(string newPath)
        {
            path = newPath;
        }

        public void DeleteJournal()
        {
            masterPanel.ConfirmDeletion((value) =>
            {
                if (value) masterPanel.DeleteJournal(this, path);
            });
        }
        public void TogglePanel(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button == PointerEventData.InputButton.Left)
            {
                OpenPanel();
            }
            else if (pointerData.button == PointerEventData.InputButton.Right)
            {
                if (Data.owner != SocketManager.UserId) return;

                panel.transform.SetParent(transform);
                panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                panel.transform.SetParent(GameObject.Find("Main Canvas").transform);
                panel.transform.SetAsLastSibling();
                panel.SetActive(!panel.gameObject.activeInHierarchy);
            }
        }
        public void OpenSharing()
        {
            collaboratorPanel.gameObject.SetActive(true);
            collaboratorPanel.transform.SetParent(GameObject.Find("Main Canvas").transform);
            collaboratorPanel.transform.SetAsLastSibling();
            collaboratorPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            collaboratorPanel.LoadCollaborators(Data.collaborators, this);
        }
        public async void CloseSharing(List<Collaborator> collaboratos)
        {
            var jsonList = new List<string>();
            for (int i = 0; i < collaboratos.Count; i++)
            {
                jsonList.Add(JsonUtility.ToJson(collaboratos[i]));
            }

            await SocketManager.Socket.EmitAsync("set-collaborators", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    Data.collaborators = collaboratos;
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Id, jsonList);
        }
        public void OpenPanel()
        {
            manager.ShowJournal(Data);
        }

        public void UpdateHeader(string header)
        {
            Data.header = header;
            text.text = header;
        }
        public void UpdateText(string text)
        {
            Data.text = text;
        }
        public void UpdateImage(string image)
        {
            Data.image = image;
        }

        public void BeginDrag()
        {
            if (startTransform != null || path == "shared") return;

            startTransform = transform.parent;
            dragObject = Instantiate(dragPanel);
            dragObject.transform.SetParent(GameObject.Find("Main Canvas").transform);
            dragObject.transform.SetAsLastSibling();
            dragObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            dragObject.GetComponentInChildren<TMP_Text>(true).text = text.text;
            dragObject.SetActive(true);
        }
        public async void EndDrag()
        {
            if (path == "shared") return;

            Destroy(dragObject);
            bool moved = false;
            var dictionaries = FindObjectsOfType<FolderJournal>();

            for (int i = 0; i < dictionaries.Length; i++)
            {
                if (dictionaries[i].Path != path && dictionaries[i].Path != "shared")
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(dictionaries[i].BackgroundRect, Input.mousePosition))
                    {
                        moved = true;
                        string newPath = dictionaries[i].Path;

                        if (path != newPath) await SocketManager.Socket.EmitAsync("move-journal", async (callback) =>
                        {
                            await UniTask.SwitchToMainThread();

                            if (callback.GetValue().GetBoolean())
                            {
                                masterPanel.MoveJournal(this, newPath);
                                path = newPath;
                            }
                            else
                            {
                                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                                transform.SetParent(startTransform);
                                startTransform = null;
                            }
                        }, id, path, newPath);
                    }
                }
                else
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(dictionaries[i].BackgroundRect, Input.mousePosition))
                    {
                        moved = true;
                        transform.SetParent(startTransform);
                        startTransform = null;
                    }
                }
            }

            if (!moved && RectTransformUtility.RectangleContainsScreenPoint(masterPanel.JournalPanel.GetComponent<RectTransform>(), Input.mousePosition))
            {
                if (path != "") await SocketManager.Socket.EmitAsync("move-journal", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();

                    if (callback.GetValue().GetBoolean())
                    {
                        masterPanel.MoveJournal(this, "");
                        path = "";
                    }
                    else
                    {
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                        transform.SetParent(startTransform);
                        startTransform = null;
                    }
                }, id, path, "");
            }
            else if (!moved)
            {
                transform.SetParent(startTransform);
                startTransform = null;
            }
        }
        public void Drag()
        {
            if (path == "shared") return;

            dragObject.transform.position = Input.mousePosition;
        }
    }

    [Serializable]
    public struct JournalData
    {
        public string id;
        public string owner;
        public string header;
        public string text;
        public string image;
        public List<Collaborator> collaborators;
        public NoteType type;
    }

    [Serializable]
    public struct Collaborator
    {
        public string user;
        public bool isCollaborator;
    }
}