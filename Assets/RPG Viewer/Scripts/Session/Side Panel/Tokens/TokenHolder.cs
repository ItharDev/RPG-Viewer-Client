using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class TokenHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private Image icon;

        [Header("Options")]
        [SerializeField] private RectTransform optionsPanel;
        [SerializeField] private GameObject selectButton;
        [SerializeField] private GameObject rootButton;
        [SerializeField] private GameObject deselectButton;

        [Header("Colors")]
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor;

        public string Path { get { return _path; } }
        public string Id { get { return Data.id; } }

        public TokenData Data;

        private string _path;
        private TokensPanel tokensPanel;
        private bool optionsOpen;
        private Color selectedColor;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnBlueprintClicked.AddListener(HandleClick);
            Events.OnBlueprintSelected.AddListener(HandleSelect);
            Events.OnBlueprintDeselected.AddListener(HandleDeselect);
            Events.OnBlueprintMoved.AddListener(HandleMoved);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnBlueprintClicked.RemoveListener(HandleClick);
            Events.OnBlueprintSelected.RemoveListener(HandleSelect);
            Events.OnBlueprintDeselected.RemoveListener(HandleDeselect);
            Events.OnBlueprintMoved.RemoveListener(HandleMoved);
        }

        private void HandleClick(TokenHolder token)
        {
            // Close options panel if it's open and not ours
            if (optionsOpen && token != this) ToggleOptions();
        }
        private void HandleSelect(TokenHolder token)
        {
            // This token was selected
            if (token == this)
            {
                selectButton.SetActive(false);
                rootButton.SetActive(true);
                deselectButton.SetActive(true);

                background.color = selectedColor;

                return;
            }

            // Allow to select another token
            rootButton.SetActive(false);
            selectButton.SetActive(true);
            deselectButton.SetActive(false);
            background.color = normalColor;
        }
        private void HandleDeselect()
        {
            // Allow token selection
            selectButton.SetActive(true);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

            // Reset background color
            background.color = normalColor;
        }
        private void HandleMoved()
        {
            // Allow token selection
            selectButton.SetActive(true);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

            // Reset background color
            background.color = normalColor;
        }

        public void ClickToken(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;

            ToggleOptions();

            // Send token toggled event
            Events.OnBlueprintClicked?.Invoke(this);
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
            float targetSize = 30.0f;
            if (selectButton.activeInHierarchy) targetSize += 30.0f;
            if (deselectButton.activeInHierarchy) targetSize += 30.0f;
            if (rootButton.activeInHierarchy) targetSize += 30.0f;

            LeanTween.size(optionsPanel, new Vector2(110.0f, optionsOpen ? targetSize : 0.0f), 0.2f).setOnComplete(() =>
            {
                // Set panel's transform to this after closing the panel
                if (!optionsOpen)
                {
                    optionsPanel.transform.SetParent(transform, true);
                    optionsPanel.anchoredPosition = new Vector2(15.0f, -65.0f);
                    optionsPanel.SetAsLastSibling();
                }

                // Enable / disable content size fitter
                optionsPanel.GetComponent<ContentSizeFitter>().enabled = optionsOpen;
            });
        }

        public void Delete()
        {
            ToggleOptions();
            Debug.Log($"Delete blueprint");
        }
        public void Select()
        {
            ToggleOptions();
            tokensPanel.SelectToken(this);
        }
        public void MoveRoot()
        {
            ToggleOptions();
            tokensPanel.MoveTokenRoot();
        }
        public void Deselect()
        {
            ToggleOptions();
            tokensPanel.DeselectScene();
        }

        public void LoadData(string id, string path, TokensPanel panel)
        {
            SocketManager.EmitAsync("get-blueprint", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    // Load Data
                    await UniTask.SwitchToMainThread();

                    TokenData data = JsonUtility.FromJson<TokenData>(callback.GetValue(1).ToString());
                    data.id = id;
                    tokensPanel = panel;
                    selectedColor = string.IsNullOrEmpty(path) ? tokensPanel.GetColor() : tokensPanel.GetDirectoryByPath(path).Data.color;
                    this._path = path;
                    LoadData(data);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
        public void UpdatePath(string newPath)
        {
            _path = newPath;
            selectedColor = string.IsNullOrEmpty(newPath) ? tokensPanel.GetColor() : tokensPanel.GetDirectoryByPath(newPath).Data.color;
        }

        private void LoadData(TokenData settings)
        {
            Data = settings;
            header.text = settings.name;
            WebManager.Download(settings.image, true, async (bytes) =>
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