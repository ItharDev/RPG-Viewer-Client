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
    public class JournalFolder : MonoBehaviour
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
        private JournalsPanel journalsPanel;
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
            Events.OnJournalFolderClicked.AddListener(HandleClick);
            Events.OnJournalClicked.AddListener(HandleClick);
            Events.OnJournalFolderSelected.AddListener(HandleSelect);
            Events.OnJournalFolderDeselected.AddListener(HandleDeselect);
            Events.OnJournalFolderMoved.AddListener(HandleMoved);
            Events.OnSidePanelChanged.AddListener(CloseOptions);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnJournalFolderClicked.RemoveListener(HandleClick);
            Events.OnJournalClicked.RemoveListener(HandleClick);
            Events.OnJournalFolderSelected.RemoveListener(HandleSelect);
            Events.OnJournalFolderDeselected.RemoveListener(HandleDeselect);
            Events.OnJournalFolderMoved.RemoveListener(HandleMoved);
            Events.OnSidePanelChanged.RemoveListener(CloseOptions);
        }
        private void Update()
        {
            if (requireSend)
            {
                // Send folder toggled event
                Events.OnJournalFolderClicked?.Invoke(this);
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

        private void HandleClick(JournalFolder folder)
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
        private void HandleClick(JournalHolder journal)
        {
            // Close options panel if it's open
            if (optionsOpen) ToggleOptions();

            // Refresh rounded corners
            background.enabled = false;
            background.enabled = true;

            border.enabled = false;
            border.enabled = true;

            Resize();
        }
        private void HandleSelect(JournalFolder folder)
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
            if (journalsPanel.IsSubFolderOf(this, folder) && folder != null) moveHereButton.SetActive(false);
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
            List<JournalFolder> folders = journalsPanel.GetFolders(this);
            List<JournalHolder> holders = journalsPanel.GetJournals(this);

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
        private int SortByName(JournalFolder folderA, JournalFolder folderB)
        {
            return folderA.Data.name.CompareTo(folderB.Data.name);
        }
        private int SortByName(JournalHolder holderA, JournalHolder holderB)
        {
            return holderA.Data.header.CompareTo(holderB.Data.header);
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
        private void ToggleOptions()
        {
            // Prevent clicking shared folders
            if (Path.Contains("shared")) return;

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
            headerInput.Select();
            header.gameObject.SetActive(false);
        }
        public void Delete()
        {
            ToggleOptions();
            MessageManager.AskConfirmation(new Confirmation("Delete folder", "Delete", "Cancel", (result) =>
            {
                if (result) SocketManager.EmitAsync("remove-journal-folder", async (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean())
                    {
                        await UniTask.SwitchToMainThread();
                        journalsPanel.RemoveFolder(this);
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Path);
            }));
        }
        public void Select()
        {
            ToggleOptions();
            journalsPanel.SelectFolder(this);
        }
        public void MoveRoot()
        {
            ToggleOptions();
            journalsPanel.MoveFolderRoot();
        }
        public void Deselect()
        {
            ToggleOptions();
            journalsPanel.DeselectFolder();
        }
        public void Move()
        {
            ToggleOptions();
            journalsPanel.MoveSelected(this);
        }
        public void ConfirmRename()
        {
            headerInput.gameObject.SetActive(false);
            header.gameObject.SetActive(true);
            string newName = string.IsNullOrEmpty(headerInput.text) ? Data.name : headerInput.text;
            header.text = newName;

            SocketManager.EmitAsync("rename-journal-folder", async (callback) =>
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
            SocketManager.EmitAsync("create-journal-folder", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    if (!folderOpen) ToggleFolder();
                    requireSend = true;

                    // Create the folder
                    string id = callback.GetValue(1).GetString();
                    JournalFolder createdFolder = journalsPanel.CreateFolder(id, Path);

                    // Activate rename field
                    createdFolder.Rename();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Path, "New folder");
        }
        public void AddJournal()
        {
            ToggleOptions();
            journalsPanel.CreateJournal(Path);
        }
        public void LoadData(Folder folder, JournalsPanel panel)
        {
            // Update fields
            Data = folder;
            header.text = folder.name;
            border.color = folder.color;
            headerInput.placeholder.GetComponent<TMP_Text>().text = folder.name;
            headerInput.text = folder.name;
            journalsPanel = panel;
            selectedColor = folder.color;
            selectedColor.a = 0.5f;

            // This is a shared folder, fetch new folder name
            if (Path.Contains("shared") && Id != "shared")
            {
                SocketManager.EmitAsync("get-user", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();

                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean())
                    {
                        string name = callback.GetValue(1).GetString();
                        Data.name = name;
                        headerInput.text = name;
                        header.text = name;
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Id);
            }
        }

        public void CalculatePath(string parentPath)
        {
            Data.path = parentPath;

            JournalFolder[] folders = GetComponentsInChildren<JournalFolder>();
            JournalHolder[] journals = GetComponentsInChildren<JournalHolder>();

            for (int i = 0; i < folders.Length; i++)
            {
                // Continue if the folder isn't sub folder of ours
                if (folders[i] == this || folders[i].transform.parent != Content) continue;

                folders[i].CalculatePath(Path);
            }
            for (int i = 0; i < journals.Length; i++)
            {
                // Continue if the journal isn't content of ours
                if (journals[i].transform.parent != Content) continue;

                journals[i].UpdatePath(Path);
            }
        }
    }
}