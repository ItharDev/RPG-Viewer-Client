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

        private void OnEnable()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();

            // Add event listeners
            Events.OnBlueprintFolderClicked.AddListener(HandleClick);
            Events.OnBlueprintFolderSelected.AddListener(HandleSelect);
            Events.OnBlueprintFolderDeselected.AddListener(HandleDeselect);
            Events.OnBlueprintFolderMoved.AddListener(HandleMoved);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnBlueprintFolderClicked.RemoveListener(HandleClick);
            Events.OnBlueprintFolderSelected.RemoveListener(HandleSelect);
            Events.OnBlueprintFolderDeselected.RemoveListener(HandleDeselect);
            Events.OnBlueprintFolderMoved.RemoveListener(HandleMoved);
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

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            layout.CalculateLayoutInputVertical();
            layout.SetLayoutVertical();
            rect.sizeDelta = new Vector2(0.0f, folderOpen ? 40.0f + content.sizeDelta.y : 40.0f);
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

        public void ClickFolder(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;

            // Toggle folder on left click
            if (pointerData.button == PointerEventData.InputButton.Left) ToggleFolder();

            // Toggle options panel on right click
            else if (pointerData.button == PointerEventData.InputButton.Right) ToggleOptions();

            // Send folder toggled event
            Events.OnBlueprintFolderClicked?.Invoke(this);
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
            float targetSize = 60.0f;
            if (selectButton.activeInHierarchy) targetSize += 30.0f;
            if (moveHereButton.activeInHierarchy) targetSize += 30.0f;
            if (deselectButton.activeInHierarchy) targetSize += 30.0f;
            if (rootButton.activeInHierarchy) targetSize += 30.0f;

            LeanTween.size(optionsPanel, new Vector2(110.0f, optionsOpen ? targetSize : 0.0f), 0.2f).setOnComplete(() =>
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
            ToggleOptions();
            headerInput.gameObject.SetActive(true);
            headerInput.ActivateInputField();
            header.gameObject.SetActive(false);
        }
        public void Delete()
        {
            ToggleOptions();
            Debug.Log($"Delete folder");
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
            headerInput.gameObject.SetActive(false);
            header.gameObject.SetActive(true);
            string newName = string.IsNullOrEmpty(headerInput.text) ? Data.name : headerInput.text;

            Debug.Log($"Renamed folder to: {newName}");
        }
        public void LoadData(Folder folder, TokensPanel panel)
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