using System;
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
    public class TokenFolder : MonoBehaviour
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
        private TokensPanel tokensPanel;
        private RectTransform rect;

        private bool folderOpen;
        private bool optionsOpen;
        private Color selectedColor;
        private bool requireSend;
        private float lastSize;

        private void OnEnable()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();

            // Add event listeners
            Events.OnBlueprintFolderClicked.AddListener(HandleClick);
            Events.OnBlueprintClicked.AddListener(HandleClick);
            Events.OnBlueprintFolderSelected.AddListener(HandleSelect);
            Events.OnBlueprintFolderDeselected.AddListener(HandleDeselect);
            Events.OnBlueprintFolderMoved.AddListener(HandleMoved);
            Events.OnSidePanelChanged.AddListener(CloseOptions);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnBlueprintFolderClicked.RemoveListener(HandleClick);
            Events.OnBlueprintClicked.RemoveListener(HandleClick);
            Events.OnBlueprintFolderSelected.RemoveListener(HandleSelect);
            Events.OnBlueprintFolderDeselected.RemoveListener(HandleDeselect);
            Events.OnBlueprintFolderMoved.RemoveListener(HandleMoved);
            Events.OnSidePanelChanged.RemoveListener(CloseOptions);
        }
        private void Update()
        {
            if (requireSend)
            {
                // Send folder toggled event
                Events.OnBlueprintFolderClicked?.Invoke(this);
                requireSend = false;
            }
            if (content.sizeDelta.y != lastSize)
            {
                lastSize = content.sizeDelta.y;
                Resize();
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

        private void HandleClick(TokenFolder folder)
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
        private void HandleClick(TokenHolder token)
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
        private void HandleSelect(TokenFolder folder)
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
            if (tokensPanel.IsSubFolderOf(this, folder)) moveHereButton.SetActive(false);
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
        public void SortContent()
        {
            List<TokenFolder> folders = tokensPanel.GetFolders(this);
            List<TokenHolder> holders = tokensPanel.GetTokens(this);

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
        private int SortByName(TokenFolder folderA, TokenFolder folderB)
        {
            return folderA.Data.name.CompareTo(folderB.Data.name);
        }
        private int SortByName(TokenHolder holderA, TokenHolder holderB)
        {
            return holderA.Data.name.CompareTo(holderB.Data.name);
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
            if (Path.Contains("public")) targetSize = 30.0f;

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
                if (result) SocketManager.EmitAsync("remove-blueprint-folder", async (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean())
                    {
                        await UniTask.SwitchToMainThread();
                        tokensPanel.RemoveFolder(this);
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Path);
            }));
        }
        public void Select()
        {
            ToggleOptions();
            tokensPanel.SelectFolder(this);
        }
        public void MoveRoot()
        {
            ToggleOptions();
            tokensPanel.MoveFolderRoot();
        }
        public void Deselect()
        {
            ToggleOptions();
            tokensPanel.DeselectFolder();
        }
        public void Move()
        {
            ToggleOptions();
            tokensPanel.MoveSelected(this);
        }
        public void ConfirmRename()
        {
            if (headerInput.text == "Public")
            {
                MessageManager.QueueMessage("Folder name cannot be 'Public'");
                return;
            }

            headerInput.gameObject.SetActive(false);
            header.gameObject.SetActive(true);
            string newName = string.IsNullOrEmpty(headerInput.text) ? Data.name : headerInput.text;
            header.text = newName;

            SocketManager.EmitAsync("rename-blueprint-folder", async (callback) =>
            {
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    Data.name = header.text;
                    GetComponentInParent<TokenFolder>(true).SortContent();
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
            SocketManager.EmitAsync("create-blueprint-folder", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    if (!folderOpen) ToggleFolder();
                    requireSend = true;

                    // Create the folder
                    string id = callback.GetValue(1).GetString();
                    TokenFolder createdFolder = tokensPanel.CreateFolder(id, Path);

                    // Activate rename field
                    createdFolder.Rename();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Path, "New folder");
        }
        public void AddToken()
        {
            ToggleOptions();
            tokensPanel.CreateToken(Path);
        }
        public void LoadData(Folder folder, TokensPanel panel, Action onComplete)
        {
            // Update fields
            Data = folder;
            header.text = folder.name;
            border.color = folder.color;
            headerInput.placeholder.GetComponent<TMP_Text>().text = folder.name;
            headerInput.text = folder.name;
            tokensPanel = panel;
            selectedColor = folder.color;
            selectedColor.a = 0.5f;

            onComplete?.Invoke();
        }

        public void CalculatePath(string parentPath)
        {
            Data.path = parentPath;

            TokenFolder[] folders = GetComponentsInChildren<TokenFolder>();
            TokenHolder[] tokens = GetComponentsInChildren<TokenHolder>();

            for (int i = 0; i < folders.Length; i++)
            {
                // Continue if the folder isn't sub folder of ours
                if (folders[i] == this || folders[i].transform.parent != Content) continue;

                folders[i].CalculatePath(Path);
            }
            for (int i = 0; i < tokens.Length; i++)
            {
                // Continue if the token isn't content of ours
                if (tokens[i].transform.parent != Content) continue;

                tokens[i].UpdatePath(Path);
            }
        }
    }
}