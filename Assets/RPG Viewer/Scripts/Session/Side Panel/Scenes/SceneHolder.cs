using System;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class SceneHolder : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private GameObject headerBackground;
        [SerializeField] private GameObject header;
        [SerializeField] private TMP_InputField headerInput;

        [Header("Options")]
        [SerializeField] private RectTransform optionsPanel;
        [SerializeField] private GameObject selectButton;
        [SerializeField] private GameObject rootButton;
        [SerializeField] private GameObject deselectButton;

        [Header("Colors")]
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor;

        public string Path { get { return Data.path; } }
        public string Id { get { return Data.id; } }

        public SceneData Data;
        private ScenesPanel scenesPanel;
        private bool optionsOpen;
        private Color selectedColor;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnSceneClicked.AddListener(HandleClick);
            Events.OnSceneFolderClicked.AddListener(HandleClick);
            Events.OnSceneSelected.AddListener(HandleSelect);
            Events.OnSceneDeselected.AddListener(HandleDeselect);
            Events.OnSceneMoved.AddListener(HandleMoved);
            Events.OnSidePanelChanged.AddListener(CloseOptions);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSceneClicked.RemoveListener(HandleClick);
            Events.OnSceneFolderClicked.RemoveListener(HandleClick);
            Events.OnSceneSelected.RemoveListener(HandleSelect);
            Events.OnSceneDeselected.RemoveListener(HandleDeselect);
            Events.OnSceneMoved.RemoveListener(HandleMoved);
            Events.OnSidePanelChanged.RemoveListener(CloseOptions);
        }

        private void HandleClick(SceneHolder scene)
        {
            // Close options panel if it's open and not ours
            if (optionsOpen && scene != this) ToggleOptions();
        }
        private void HandleClick(SceneFolder folder)
        {
            // Close options panel if it's open
            if (optionsOpen) ToggleOptions();
        }
        private void HandleSelect(SceneHolder scene)
        {
            // This scene was selected
            if (scene == this)
            {
                selectButton.SetActive(false);
                rootButton.SetActive(true);
                deselectButton.SetActive(true);

                background.color = selectedColor;

                return;
            }

            // Allow to select another scene
            rootButton.SetActive(false);
            selectButton.SetActive(true);
            deselectButton.SetActive(false);
            background.color = normalColor;
        }
        private void HandleDeselect()
        {
            // Allow scene selection
            selectButton.SetActive(true);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

            // Reset background color
            background.color = normalColor;
        }
        private void HandleMoved()
        {
            // Allow scene selection
            selectButton.SetActive(true);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

            // Reset background color
            background.color = normalColor;
        }

        public void ClickScene(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;

            ToggleOptions();

            // Send scene toggled event
            Events.OnSceneClicked?.Invoke(this);
        }
        private void ToggleOptions()
        {
            // Toggle open state
            optionsOpen = !optionsOpen;

            // Set panel on top before opening it
            if (optionsOpen)
            {
                optionsPanel.transform.SetParent(UICanvas.Instance.transform, true);
                optionsPanel.SetAsLastSibling();
            }

            // Calculate panel's target height
            float targetSize = 90.0f;
            if (selectButton.activeInHierarchy) targetSize += 30.0f;
            if (deselectButton.activeInHierarchy) targetSize += 30.0f;
            if (rootButton.activeInHierarchy) targetSize += 30.0f;

            LeanTween.size(optionsPanel, new Vector2(110.0f, optionsOpen ? targetSize : 0.0f), 0.2f).setOnComplete(() =>
            {
                // Set panel's transform to this after closing the panel
                if (!optionsOpen)
                {
                    optionsPanel.transform.SetParent(transform, true);
                    optionsPanel.anchoredPosition = new Vector2(15.0f, -100.0f);
                    optionsPanel.SetAsLastSibling();
                }

                // Enable / disable content size fitter
                optionsPanel.GetComponent<ContentSizeFitter>().enabled = optionsOpen;
            });
        }
        private void CloseOptions()
        {
            if (!optionsOpen) return;

            optionsOpen = false;

            LeanTween.size(optionsPanel, new Vector2(115.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                optionsPanel.transform.SetParent(transform, true);
                optionsPanel.anchoredPosition = new Vector2(15.0f, -45.0f);
                optionsPanel.SetAsLastSibling();

                // Enable / disable content size fitter
                optionsPanel.GetComponent<ContentSizeFitter>().enabled = false;
            });
        }

        public void Play()
        {
            ToggleOptions();
            SocketManager.EmitAsync("set-state", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, ConnectionManager.State.synced);
        }
        public void Rename()
        {
            ToggleOptions();
            headerInput.gameObject.SetActive(true);
            headerText.gameObject.SetActive(false);
            header.SetActive(true);
            headerInput.DeactivateInputField();
            headerInput.ActivateInputField();
            headerInput.Select();
        }
        public void ConfirmRename()
        {
            header.SetActive(false);
            headerInput.gameObject.SetActive(false);
            headerText.gameObject.SetActive(true);
            string newName = string.IsNullOrEmpty(headerInput.text) ? Data.info.name : headerInput.text;

            SocketManager.EmitAsync("rename-scene", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    headerText.text = newName;
                    Data.info.name = newName;
                    GetComponentInParent<SceneFolder>(true).SortContent();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id, newName);
        }
        public void Delete()
        {
            ToggleOptions();
            MessageManager.AskConfirmation(new Confirmation("Delete scene", "Delete", "Cancel", (result) =>
            {
                if (result) SocketManager.EmitAsync("remove-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (callback.GetValue().GetBoolean())
                    {
                        scenesPanel.RemoveScene(this);
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Path, Id);

            }));
        }
        public void Select()
        {
            ToggleOptions();
            scenesPanel.SelectScene(this);
        }
        public void MoveRoot()
        {
            ToggleOptions();
            scenesPanel.MoveSceneRoot();
        }
        public void Deselect()
        {
            ToggleOptions();
            scenesPanel.DeselectScene();
        }

        public void LoadData(string id, string path, ScenesPanel panel, Action onComplete)
        {
            SocketManager.EmitAsync("get-scene", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    // Load Data
                    SceneData data = JsonUtility.FromJson<SceneData>(callback.GetValue(1).ToString());
                    data.id = id;
                    scenesPanel = panel;
                    selectedColor = string.IsNullOrEmpty(path) ? scenesPanel.GetColor() : scenesPanel.GetDirectoryByPath(path).Data.color;
                    selectedColor.a = 0.5f;
                    LoadData(data);
                    onComplete?.Invoke();
                    return;
                }

                Data = new SceneData(new SceneInfo(), new GridData(), new LightingSettings());
                Data.id = id;
                panel.RemoveScene(this);
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
        public void UpdatePath(string newPath)
        {
            Data.path = newPath;
            selectedColor = string.IsNullOrEmpty(newPath) ? scenesPanel.GetColor() : scenesPanel.GetDirectoryByPath(newPath).Data.color;
        }

        private void LoadData(SceneData data)
        {
            Data = data;
            headerText.text = data.info.name;
            headerBackground.SetActive(!string.IsNullOrEmpty(data.info.name));
            WebManager.Download(data.info.image, true, async (bytes) =>
            {
                // Return if image couldn't be loaded
                if (bytes == null) return;

                await UniTask.SwitchToMainThread();

                // Generate texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                icon.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                icon.color = Color.white;
                icon.GetComponent<RectTransform>().sizeDelta = new Vector2(50.0f, 50.0f);
            });
        }
    }
}