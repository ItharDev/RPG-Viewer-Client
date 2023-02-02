using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;

namespace RPG
{
    public class JournalHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        [SerializeField] private GameObject panel;
        [SerializeField] private CollaboratorPanel collaboratorPanel;

        [SerializeField] private Color normalColor;
        [SerializeField] private Color activeColor;

        private JournalData data;
        private string id;
        private string path;

        private MasterPanel masterPanel;
        private GameObject dragObject;

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

        public void LoadData(JournalData _data, string _id, string _folder, MasterPanel _masterPanel)
        {
            data = _data;
            id = _id;
            path = _folder;
            text.text = _data.header;
            masterPanel = _masterPanel;
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
        public void TogglePanel()
        {
            panel.transform.SetParent(transform);
            panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            panel.transform.SetParent(GameObject.Find("Main Canvas").transform);
            panel.transform.SetAsLastSibling();
            panel.SetActive(!panel.gameObject.activeInHierarchy);
        }
        public void OpenSharing()
        {
            collaboratorPanel.gameObject.SetActive(true);
            collaboratorPanel.transform.SetParent(GameObject.Find("Main Canvas").transform);
            collaboratorPanel.transform.SetAsLastSibling();
            collaboratorPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            collaboratorPanel.LoadCollaborators(data.collaborators, this);
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
                    data.collaborators = collaboratos;
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Id, jsonList);
        }

        public async void ModifyHeader(string header)
        {
            await SocketManager.Socket.EmitAsync("modify-journal-header", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    data.header = header;
                    text.text = header;
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }

            }, id, header);
        }

        public void BeginDrag()
        {
            dragObject = Instantiate(text.gameObject);
            dragObject.transform.SetParent(GameObject.Find("Main Canvas").transform);
            dragObject.transform.SetAsLastSibling();
            dragObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }
        public async void EndDrag()
        {
            Destroy(dragObject);
            bool moved = false;
            var dictionaries = FindObjectsOfType<FolderJournal>();

            for (int i = 0; i < dictionaries.Length; i++)
            {
                if (dictionaries[i].Path != path)
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
                            }
                        }, id, path, newPath);
                    }
                }
                else
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(dictionaries[i].BackgroundRect, Input.mousePosition))
                    {
                        moved = true;
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
                    }
                }, id, path, "");
            }
        }
        public void Drag()
        {
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