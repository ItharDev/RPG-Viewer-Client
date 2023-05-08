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
            get { return string.IsNullOrEmpty(data.path) ? Id : $"{data.path}/{Id}"; }
        }
        public string Id { get { return data.id; } }
        public Transform Content { get { return content.transform; } }

        private Folder data;
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
            Events.OnTokenFolderClicked.AddListener(Resize);
            Events.OnTokenFolderMoveInit.AddListener(HandleFolderMoveInit);
            Events.OnTokenFolderMoveCancel.AddListener(HandleFolderMoveCancel);
            Events.OnTokenFolderMoveFinish.AddListener(HandleFolderMoveFinish);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnTokenFolderClicked.RemoveListener(Resize);
            Events.OnTokenFolderMoveInit.RemoveListener(HandleFolderMoveInit);
            Events.OnTokenFolderMoveCancel.RemoveListener(HandleFolderMoveCancel);
            Events.OnTokenFolderMoveFinish.RemoveListener(HandleFolderMoveFinish);
        }

        private void Resize(TokenFolder folder)
        {
            // Close options panel if it's open and not ours
            if (optionsOpen && folder != this) ToggleOptions();

            // Resize rect
            rect.sizeDelta = new Vector2(0.0f, folderOpen ? 40.0f + content.sizeDelta.y : 40.0f);
            rect.sizeDelta = new Vector2(0.0f, folderOpen ? 40.0f + content.sizeDelta.y : 40.0f);

            // Refresh rounded corners
            background.enabled = false;
            background.enabled = true;

            border.enabled = false;
            border.enabled = true;
        }
        private void HandleFolderMoveInit(TokenFolder folder)
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
        private void HandleFolderMoveCancel()
        {
            selectButton.SetActive(true);
            moveHereButton.SetActive(false);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

            background.color = normalColor;
        }
        private void HandleFolderMoveFinish()
        {
            selectButton.SetActive(true);
            moveHereButton.SetActive(false);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

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
            Events.OnTokenFolderClicked?.Invoke(this);
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
        public void InitMove()
        {
            ToggleOptions();
            tokensPanel.InitFolderMove(this);
        }
        public void MoveRoot()
        {
            ToggleOptions();
            tokensPanel.MoveRoot();
        }
        public void CancelMove()
        {
            ToggleOptions();
            tokensPanel.CancelFolderMove();
        }
        public void FinishMove()
        {
            ToggleOptions();
            tokensPanel.FinishFolderMove(this);
        }
        public void ConfirmRename()
        {
            headerInput.gameObject.SetActive(false);
            header.gameObject.SetActive(true);
            string newName = string.IsNullOrEmpty(headerInput.text) ? data.name : headerInput.text;

            Debug.Log($"Renamed folder to: {newName}");
        }
        public void LoadData(Folder folder, TokensPanel panel)
        {
            // Update fields
            data = folder;
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
            data.path = parentPath;

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