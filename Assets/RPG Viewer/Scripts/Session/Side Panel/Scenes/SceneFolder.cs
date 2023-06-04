using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using Nobi.UiRoundedCorners;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class SceneFolder : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private RectTransform content;
        [SerializeField] private Image border;

        [Header("Button")]
        [SerializeField] private TMP_Text header;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite openIcon;
        [SerializeField] private Sprite closedIcon;
        [SerializeField] private TMP_InputField headerInput;

        [Header("Options")]
        [SerializeField] private RectTransform optionsPanel;
        [SerializeField] private GameObject selectButton;
        [SerializeField] private GameObject rootButton;
        [SerializeField] private GameObject deselectButton;
        [SerializeField] private GameObject moveHereButton;

        [Header("Colors")]
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor;

        public string Path
        {
            get { return string.IsNullOrEmpty(Data.path) ? Id : $"{Data.path}/{Id}"; }
        }
        public string Id { get { return Data.id; } }
        public Transform Content { get { return content.transform; } }

        public Folder Data;
        private ScenesPanel scenesPanel;
        private RectTransform rect;

        private bool folderOpen;
        private bool optionsOpen;
        private Color selectedColor;
        private bool requireSend;
        private float lastSize;
        private float lastCount;

        private void OnEnable()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();

            // Add event listeners
            Events.OnSceneFolderClicked.AddListener(HandleClick);
            Events.OnSceneFolderSelected.AddListener(HandleSelect);
            Events.OnSceneFolderDeselected.AddListener(HandleDeselect);
            Events.OnSceneFolderMoved.AddListener(HandleMoved);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSceneFolderClicked.RemoveListener(HandleClick);
            Events.OnSceneFolderSelected.RemoveListener(HandleSelect);
            Events.OnSceneFolderDeselected.RemoveListener(HandleDeselect);
            Events.OnSceneFolderMoved.RemoveListener(HandleMoved);
        }
        private void Update()
        {
            if (requireSend)
            {
                // Send folder toggled event
                Events.OnSceneFolderClicked?.Invoke(this);
                requireSend = false;
            }
            if (content.sizeDelta.y != lastSize)
            {
                lastSize = content.sizeDelta.y;
                Resize();
            }
            if (lastCount != content.transform.childCount)
            {
                lastCount = content.transform.childCount;
                SortContent();
            }
        }

        private void Resize()
        {
            rect.sizeDelta = new Vector2(0.0f, folderOpen ? 40.0f + content.sizeDelta.y : 40.0f);

            // Refresh corners
            var corners = background.GetComponent<ImageWithRoundedCorners>();
            corners.Validate();
            corners.Refresh();
        }

        private void HandleClick(SceneFolder folder)
        {
            // Close options panel if it's open and not ours
            if (optionsOpen && folder != this) ToggleOptions();

            // Refresh rounded corners
            background.enabled = false;
            background.enabled = true;

            border.enabled = false;
            border.enabled = true;

            Resize();
        }
        private void HandleSelect(SceneFolder folder)
        {
            // This folder was selected
            if (folder == this)
            {
                selectButton.SetActive(false);
                rootButton.SetActive(true);
                moveHereButton.SetActive(false);
                deselectButton.SetActive(true);

                background.color = selectedColor;

                return;
            }

            // Allow to select another folder
            rootButton.SetActive(false);
            selectButton.SetActive(true);
            moveHereButton.SetActive(true);
            deselectButton.SetActive(false);
            background.color = normalColor;

            // Check if we are children of the selected folder
            if (scenesPanel.IsSubFolderOf(this, folder) && folder != null) moveHereButton.SetActive(false);
        }
        private void HandleDeselect()
        {
            // Allow folder selection
            selectButton.SetActive(true);
            moveHereButton.SetActive(false);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

            // Reset background color
            background.color = normalColor;
        }
        private void HandleMoved()
        {
            // Allow folder selection
            selectButton.SetActive(true);
            moveHereButton.SetActive(false);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

            // Reset background color
            background.color = normalColor;
        }
        private void SortContent()
        {
            List<SceneFolder> folders = scenesPanel.GetFolders(this);
            List<SceneHolder> holders = scenesPanel.GetScenes(this);

            folders.Sort(SortByName);
            holders.Sort(SortByName);

            for (int i = 0; i < folders.Count; i++)
            {
                folders[i].transform.SetSiblingIndex(i);
            }
            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].transform.SetSiblingIndex(i + folders.Count);
            }
        }
        private int SortByName(SceneFolder folderA, SceneFolder folderB)
        {
            return folderA.Data.name.CompareTo(folderB.Data.name);
        }
        private int SortByName(SceneHolder holderA, SceneHolder holderB)
        {
            return holderA.Data.info.name.CompareTo(holderB.Data.info.name);
        }

        public void ClickFolder(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;

            // Toggle folder on left click
            if (pointerData.button == PointerEventData.InputButton.Left) ToggleFolder();

            // Toggle options panel on right click
            else if (pointerData.button == PointerEventData.InputButton.Right) ToggleOptions();

            requireSend = true;
        }
        private void ToggleFolder()
        {
            // Toggle folder state
            folderOpen = !folderOpen;
            icon.sprite = folderOpen ? openIcon : closedIcon;
            if (optionsOpen) ToggleOptions();
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
            float targetSize = 120.0f;
            if (selectButton.activeInHierarchy) targetSize += 30.0f;
            if (moveHereButton.activeInHierarchy) targetSize += 30.0f;
            if (deselectButton.activeInHierarchy) targetSize += 30.0f;
            if (rootButton.activeInHierarchy) targetSize += 30.0f;

            LeanTween.size(optionsPanel, new Vector2(115.0f, optionsOpen ? targetSize : 0.0f), 0.2f).setOnComplete(() =>
            {
                // Set panel's transform to this after closing the panel
                if (!optionsOpen)
                {
                    optionsPanel.transform.SetParent(transform, true);
                    optionsPanel.anchoredPosition = new Vector2(15.0f, -45.0f);
                    optionsPanel.SetAsLastSibling();
                }

                // Enable / disable content size fitter
                optionsPanel.GetComponent<ContentSizeFitter>().enabled = optionsOpen;
            });
        }

        public void Rename()
        {
            if (optionsOpen) ToggleOptions();
            headerInput.gameObject.SetActive(true);
            headerInput.ActivateInputField();
            header.gameObject.SetActive(false);
        }
        public void Delete()
        {
            ToggleOptions();
            MessageManager.AskConfirmation(new Confirmation("Delete folder", "Delete", "Cancel", (result) =>
            {
                if (result) SocketManager.EmitAsync("remove-scene-folder", async (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean())
                    {
                        await UniTask.SwitchToMainThread();
                        scenesPanel.RemoveFolder(this);
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Path);
            }));
        }
        public void Select()
        {
            ToggleOptions();
            scenesPanel.SelectFolder(this);
        }
        public void MoveRoot()
        {
            ToggleOptions();
            scenesPanel.MoveFolderRoot();
        }
        public void Deselect()
        {
            ToggleOptions();
            scenesPanel.DeselectFolder();
        }
        public void Move()
        {
            ToggleOptions();
            scenesPanel.MoveSelected(this);
        }
        public void ConfirmRename()
        {
            headerInput.gameObject.SetActive(false);
            header.gameObject.SetActive(true);
            string newName = string.IsNullOrEmpty(headerInput.text) ? Data.name : headerInput.text;
            header.text = newName;

            SocketManager.EmitAsync("rename-scene-folder", async (callback) =>
            {
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    Data.name = header.text;
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                header.text = Data.name;
            }, Path, newName);
        }
        public void AddFolder()
        {
            ToggleOptions();
            SocketManager.EmitAsync("create-scene-folder", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    if (!folderOpen) ToggleFolder();
                    requireSend = true;

                    // Create the folder
                    string id = callback.GetValue(1).GetString();
                    SceneFolder createdFolder = scenesPanel.CreateFolder(id, Path);

                    // Activate rename field
                    createdFolder.Rename();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Path, "New Folder");
        }
        public void AddScene()
        {
            ToggleOptions();
            scenesPanel.CreateScene(Path);
        }
        public void LoadData(Folder folder, ScenesPanel panel)
        {
            // Update fields
            Data = folder;
            header.text = folder.name;
            border.color = folder.color;
            headerInput.placeholder.GetComponent<TMP_Text>().text = folder.name;
            headerInput.text = folder.name;
            scenesPanel = panel;
            selectedColor = folder.color;
            selectedColor.a = 0.5f;
        }

        public void CalculatePath(string parentPath)
        {
            Data.path = parentPath;

            SceneFolder[] folders = GetComponentsInChildren<SceneFolder>();
            SceneHolder[] scenes = GetComponentsInChildren<SceneHolder>();

            for (int i = 0; i < folders.Length; i++)
            {
                // Continue if the folder isn't sub folder of ours
                if (folders[i] == this || folders[i].transform.parent != Content) continue;

                folders[i].CalculatePath(Path);
            }
            for (int i = 0; i < scenes.Length; i++)
            {
                // Continue if the scene isn't content of ours
                if (scenes[i].transform.parent != Content) continue;

                scenes[i].UpdatePath(Path);
            }
        }
    }
}