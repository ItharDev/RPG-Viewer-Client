using System.Collections.Generic;
using System;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class BlueprintHolder : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text text;

        [SerializeField] private GameObject panel;
        [SerializeField] private PermissionPanel permissionPanel;

        private TokenData data;
        private string id;
        private string path;

        private MasterPanel masterPanel;
        private GameObject dragObject;

        public string Id { get { return id; } }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition))
                {
                    panel.SetActive(false);
                }
            }
        }

        public void LoadData(TokenData _data, string _id, string _folder, byte[] _image, MasterPanel _masterPanel)
        {
            data = _data;
            id = _id;
            path = _folder;
            text.text = _data.name;
            masterPanel = _masterPanel;

            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(_image);

            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            image.color = Color.white;
        }
        public void UpdatePath(string newPath)
        {
            path = newPath;
        }


        public void OpenConfig()
        {
            masterPanel.OpenBlueprintConfig(data, this, path, image.sprite.texture.GetRawTextureData());
        }
        public void DeleteBlueprint()
        {
            masterPanel.ConfirmDeletion((value) =>
            {
                if (value) masterPanel.DeleteBlueprint(this, path);
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
        public void OpenPermissions()
        {
            permissionPanel.gameObject.SetActive(true);
            permissionPanel.transform.SetParent(GameObject.Find("Main Canvas").transform);
            permissionPanel.transform.SetAsLastSibling();
            permissionPanel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            permissionPanel.LoadPermissions(data.permissions, this);
        }
        public async void ClosePermissions(List<Permission> permissions)
        {
            var jsonList = new List<string>();
            for (int i = 0; i < permissions.Count; i++)
            {
                jsonList.Add(JsonUtility.ToJson(permissions[i]));
                Debug.Log(jsonList[i]);
            }

            Debug.Log("Sending permissions");
            await SocketManager.Socket.EmitAsync("set-permissions", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    data.permissions = permissions;
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Id, jsonList);
        }

        public async void ModifyBlueprint(TokenData _data, byte[] _bytes, bool _imageChanged)
        {
            await SocketManager.Socket.EmitAsync("modify-blueprint", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    data = _data;

                    if (_imageChanged)
                    {
                        data.image = callback.GetValue(1).GetString();

                        Texture2D texture = new Texture2D(1, 1);
                        texture.LoadImage(_bytes);

                        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    }

                    text.text = _data.name;
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }
                
            }, id, JsonUtility.ToJson(_data), _imageChanged ? Convert.ToBase64String(_bytes) : null);
        }

        public void BeginDrag()
        {
            dragObject = Instantiate(image.gameObject);
            dragObject.transform.SetParent(GameObject.Find("Main Canvas").transform);
            dragObject.transform.SetAsLastSibling();
            dragObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        }
        public async void EndDrag()
        {
            Destroy(dragObject);
            bool moved = false;
            var dictionaries = FindObjectsOfType<FolderBlueprint>();

            for (int i = 0; i < dictionaries.Length; i++)
            {
                if (dictionaries[i].Path != path)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(dictionaries[i].BackgroundRect, Input.mousePosition))
                    {
                        moved = true;
                        string newPath = dictionaries[i].Path;

                        if (path != newPath) await SocketManager.Socket.EmitAsync("move-blueprint", async (callback) =>
                        {
                            await UniTask.SwitchToMainThread();

                            if (callback.GetValue().GetBoolean())
                            {
                                FindObjectOfType<MasterPanel>().MoveBlueprint(this, newPath);
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

            if (!moved && RectTransformUtility.RectangleContainsScreenPoint(FindObjectOfType<MasterPanel>().BlueprintPanel.GetComponent<RectTransform>(), Input.mousePosition))
            {
                if (path != "") await SocketManager.Socket.EmitAsync("move-blueprint", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();

                    if (callback.GetValue().GetBoolean())
                    {
                        FindObjectOfType<MasterPanel>().MoveBlueprint(this, "");
                        path = "";
                    }
                    else
                    {
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    }
                }, id, path, "");
            }
            else if (!moved && !RectTransformUtility.RectangleContainsScreenPoint(FindObjectOfType<MasterPanel>().BlueprintPanel.GetComponent<RectTransform>(), Input.mousePosition))
            {
                Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                TokenData newData = data;
                newData.position = pos;
                newData.enabled = false;

                await SocketManager.Socket.EmitAsync("create-token", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, JsonUtility.ToJson(newData));
            }
        }
        public void Drag()
        {
            dragObject.transform.position = Input.mousePosition;
        }
    }
}