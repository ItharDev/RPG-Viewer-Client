using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPG
{
    public class SceneHolder : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text text;

        [SerializeField] private GameObject panel;

        public SceneSettings Data;
        private string id;
        private string path;

        private MasterPanel masterPanel;
        private GameObject dragObject;
        public string Id { get { return id; } }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition) && !RectTransformUtility.RectangleContainsScreenPoint(panel.GetComponent<RectTransform>(), Input.mousePosition))
                {
                    panel.SetActive(false);
                }
            }
        }

        public void LoadData(SceneSettings _data, string _id, string _path, byte[] _image, MasterPanel _masterPanel)
        {
            Data = _data;
            id = _id;
            Data.id = _id;
            Data.bytes = _image;
            path = _path;
            Data.path = _path;
            text.text = _data.data.name;
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

        public async void ModifyScene()
        {
            await SocketManager.Socket.EmitAsync("set-scene", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, "");
            SocketManager.SceneSettings = Data;
            SceneManager.LoadScene("Scene");
        }
        public void DeleteScene()
        {
            masterPanel.ConfirmDeletion((value) =>
            {
                if (value) masterPanel.DeleteScene(this, path);
            });
        }
        public void TogglePanel()
        {
            panel.transform.SetParent(transform);
            panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            panel.transform.SetParent(GameObject.Find("Main Canvas").transform);
            panel.transform.SetAsLastSibling();
            panel.SetActive(!panel.gameObject.activeInHierarchy);
        }

        public void BeginDrag()
        {
            dragObject = Instantiate(image.transform.parent.gameObject);
            dragObject.transform.SetParent(GameObject.Find("Main Canvas").transform);
            dragObject.transform.SetAsLastSibling();
            dragObject.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            dragObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
            dragObject.GetComponentInChildren<Image>(true).enabled = true;
        }
        public async void EndDrag()
        {
            bool moved = false;
            var dictionaries = FindObjectsOfType<FolderScene>();
            Destroy(dragObject);

            for (int i = 0; i < dictionaries.Length; i++)
            {
                if (dictionaries[i].Path != path)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(dictionaries[i].BackgroundRect, Input.mousePosition))
                    {
                        moved = true;
                        string newPath = dictionaries[i].Path;

                        if (path != newPath) await SocketManager.Socket.EmitAsync("move-scene", async (callback) =>
                        {
                            await UniTask.SwitchToMainThread();

                            if (callback.GetValue().GetBoolean())
                            {
                                FindObjectOfType<MasterPanel>().MoveScene(this, newPath);
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

            if (!moved && RectTransformUtility.RectangleContainsScreenPoint(FindObjectOfType<MasterPanel>().ScenePanel.GetComponent<RectTransform>(), Input.mousePosition))
            {
                if (path != "") await SocketManager.Socket.EmitAsync("move-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();

                    if (callback.GetValue().GetBoolean())
                    {
                        FindObjectOfType<MasterPanel>().MoveScene(this, "");
                        path = "";
                    }
                    else
                    {
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    }
                }, id, path, "");
            }
            else if (!moved && !RectTransformUtility.RectangleContainsScreenPoint(FindObjectOfType<MasterPanel>().ScenePanel.GetComponent<RectTransform>(), Input.mousePosition))
            {
                await SocketManager.Socket.EmitAsync("set-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Data.id);
            }
        }
        public void Drag()
        {
            dragObject.transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.0f);
        }
    }
}