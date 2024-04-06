using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class TokenUI : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private Canvas uICanvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private CanvasGroup uIcanvasGroup;

        [Header("UI")]
        [SerializeField] private Image image;
        [SerializeField] private Image outlineImage;
        [SerializeField] private TMP_Text label;
        [SerializeField] private CanvasGroup buttonsGroup;
        [SerializeField] private GameObject lockButton;
        [SerializeField] private GameObject visibilityButton;
        [SerializeField] private GameObject conditionsButton;


        [Space]
        [SerializeField] private Image lockedImage;
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite unlockedSprite;

        [Space]
        [SerializeField] private TMP_InputField elevationInput;
        [SerializeField] private GameObject elevationPanel;

        [Space]
        [SerializeField] private TMP_InputField healthInput;
        [SerializeField] private GameObject healthPanel;

        [Space]
        [SerializeField] private TokenConfiguration configPrefab;

        public RectTransform Rect { get { return canvas.GetComponent<RectTransform>(); } }
        public bool MouseOver
        {
            get
            {
                return RectTransformUtility.RectangleContainsScreenPoint(Rect, Camera.main.ScreenToWorldPoint(Input.mousePosition)) ||
                token.Conditions.MouseOver;
            }
        }
        public bool Editing
        {
            get
            {
                return config != null || healthInput.isFocused || elevationInput.isFocused;
            }
        }

        private Token token;
        private Outline outline;
        private TokenConfiguration config;
        private Shader outlineShader;

        public float OriginalRotation;

        private void OnEnable()
        {
            // Get reference of main class and outline
            if (token == null) token = GetComponent<Token>();
            if (outline == null) outline = outlineImage.GetComponent<Outline>();

            outlineShader = Shader.Find("GUI/Text Shader");

            // Add event listeners
            Events.OnToolChanged.AddListener(HandleRaycast);
            Events.OnTokensSelected.AddListener(HandleSelection);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnToolChanged.RemoveListener(HandleRaycast);
            Events.OnTokensSelected.RemoveListener(HandleSelection);
        }
        private void Awake()
        {
            lockButton.SetActive(ConnectionManager.Info.isMaster);
            visibilityButton.SetActive(ConnectionManager.Info.isMaster);
        }
        private void Update()
        {
            // Return if token is not selected
            if (!token.Selected) return;

            if (Input.GetMouseButtonDown(0) && !MouseOver && !Session.Instance.TokenManager.TokenHovered)
            {
                if (token.Conditions.IsOpen) token.Conditions.ToggleConditions();
                Events.OnTokenSelected?.Invoke(null, false);
            }

            HandleElevation();
        }
        private void HandleSelection(List<Token> tokens)
        {
            bool selected = tokens.Contains(token);
            outline.enabled = selected;
            buttonsGroup.alpha = selected && ConnectionManager.Info.isMaster ? 1.0f : 0.0f;
            buttonsGroup.blocksRaycasts = selected && ConnectionManager.Info.isMaster ? true : false;
            conditionsButton.SetActive(selected);

            if (string.IsNullOrEmpty(healthInput.text)) healthPanel.SetActive(selected);
            else healthPanel.SetActive(true);

            if (string.IsNullOrEmpty(elevationInput.text)) elevationPanel.SetActive(selected);
            else elevationPanel.SetActive(true);
            UpdateSorting();
        }

        private void HandleRaycast(Tool tool)
        {
            // Return if we aren't the owner
            if (!token.IsOwner)
            {
                image.raycastTarget = false;
                return;
            }

            // Enable / disable raycasting
            image.raycastTarget = tool == Tool.Move;
        }
        private void HandleElevation()
        {
            if (Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown))
            {
                int elevationValue = string.IsNullOrEmpty(elevationInput.text) ? 0 : int.Parse(elevationInput.text);

                // Modify elevation value
                elevationValue += Input.GetKey(KeyCode.PageUp) ? 5 : -5;
            }
        }

        public void SetAlpha(float value)
        {
            // Update group alpha
            canvasGroup.alpha = value;
        }
        public void ToggleUI(bool enabled)
        {
            uICanvas.enabled = enabled;
            canvas.enabled = enabled;
        }
        public void SetHealth(int value)
        {
            healthInput.interactable = token.IsOwner;
            healthInput.text = value == 0 ? "" : value.ToString();
            if (!token.IsOwner)
            {
                healthPanel.SetActive(false);
                return;
            }

            healthPanel.SetActive(token.Selected);
            if (!token.Selected) healthPanel.SetActive(value != 0);
        }
        public void SetElevation(int value)
        {
            elevationInput.interactable = token.IsOwner;
            elevationInput.text = value == 0 ? "" : value.ToString();

            elevationPanel.SetActive(token.Selected);
            if (!token.Selected) elevationPanel.SetActive(value != 0);
        }
        public void UpdateHealth()
        {
            int health = 0;
            int.TryParse(healthInput.text, out health);

            SocketManager.EmitAsync("update-health", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, token.Id, health);
        }
        public void UpdateElevation()
        {
            int elevation = 0;
            int.TryParse(elevationInput.text, out elevation);

            SocketManager.EmitAsync("update-elevation", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, token.Id, elevation);
        }
        public void EnableToken()
        {
            SocketManager.EmitAsync("update-visibility", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, token.Id, !token.Data.enabled);
        }
        public void Toggle(bool enabled)
        {
            if (!ConnectionManager.Info.isMaster)
            {
                SetAlpha(enabled ? 1.0f : 0.0f);
            }
            Color imageColor = Color.white;
            imageColor.a = enabled ? 1.0f : 0.5f;
            image.color = imageColor;
        }
        public void DisableOutline()
        {
            outlineImage.gameObject.SetActive(false);
        }
        public void SetRotation(float value)
        {
            image.transform.eulerAngles = new Vector3(0.0f, 0.0f, value);
            token.Data.rotation = value;
            OriginalRotation = value;
        }
        public void PreviewRotation(float value)
        {
            image.transform.eulerAngles = new Vector3(0.0f, 0.0f, value);
        }
        public void SetImage(Sprite sprite)
        {
            // Update image sprite and color
            image.sprite = sprite;
            outlineImage.sprite = sprite;
            outlineImage.material.shader = outlineShader;
        }
        public void Reload()
        {
            // Reload all UI elements
            SetRotation(token.Data.rotation);
            UpdateSorting();
            Resize();
            LoadUI();
            LockToken(token.Data.locked);
            ApplyVisibility();

            if (!ConnectionManager.Info.isMaster) EnableRaycasting(token.Permission.type == PermissionType.Controller);
        }
        private void LoadUI()
        {
            label.text = token.Data.name;
            label.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(label.text));
            Color imageColor = Color.white;
            imageColor.a = token.Data.enabled ? 1.0f : 0.5f;
            image.color = imageColor;
            uIcanvasGroup.blocksRaycasts = token.IsOwner;
            SetElevation(token.Data.elevation);
            SetHealth(token.Data.health);
        }
        private void ApplyVisibility()
        {
            ToggleUI(token.Visibility.visible);
        }
        public void EnableRaycasting(bool enable)
        {
            // Enable / disable raycasting
            image.raycastTarget = enable;
        }
        public void LockToken(bool state)
        {
            lockedImage.sprite = state ? lockedSprite : unlockedSprite;
        }

        public void OnPointerClick(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button == PointerEventData.InputButton.Left) Events.OnTokenSelected?.Invoke(token, Input.GetKey(KeyCode.LeftControl));
            else if (pointerData.button == PointerEventData.InputButton.Right) ModifyToken();
        }
        public void OnPointerEnter(BaseEventData eventData)
        {
            outline.enabled = true;
            Session.Instance.TokenManager.HoverToken(true);
        }
        public void OnPointerExit(BaseEventData eventData)
        {
            if (!token.Selected) outline.enabled = false;
            Session.Instance.TokenManager.HoverToken(false);
        }

        private void ModifyToken()
        {
            if (config != null) return;

            config = Instantiate(configPrefab);
            config.transform.SetParent(UICanvas.Instance.transform);
            config.transform.localPosition = Vector3.zero;
            config.transform.SetAsLastSibling();
            config.LoadData(token.Data, token.Lighting, image.sprite.texture.GetRawTextureData(), "Modify Token", (data, image, lighting) =>
            {
                bool imageChanged = !image.SequenceEqual(this.image.sprite.texture.GetRawTextureData());
                SocketManager.EmitAsync("modify-token", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, token.Id, JsonUtility.ToJson(data), JsonUtility.ToJson(lighting), imageChanged ? Convert.ToBase64String(image) : null);

            });
        }

        public void UpdateSorting()
        {
            string targetLayer = "Characters";
            // Move token to correct sorting layer
            switch (token.Data.type)
            {
                case TokenType.Character:
                    // Check if we are dead
                    if (token.Conditions.IsDead) targetLayer = "Dead Characters";

                    // Check if we are owners
                    if (token.IsOwner) targetLayer = "Owned Characters";
                    break;
                case TokenType.Mount:
                    targetLayer = "Mounts";

                    // Check if we are dead
                    if (token.Conditions.IsDead) targetLayer = "Dead Mounts";

                    // Check if we are owners
                    if (token.IsOwner) targetLayer = "Owned Mounts";
                    break;
                case TokenType.Item:
                    targetLayer = "Items";

                    // Check if we are owners
                    if (token.IsOwner) targetLayer = "Owned Items";
                    break;
            }

            uICanvas.sortingLayerName = outline.enabled ? "Above Fog" : targetLayer;
            canvas.sortingLayerName = outline.enabled ? "Above Fog" : targetLayer;
        }
        public void Resize()
        {
            // Calculate correct canvas scale
            float cellSize = Session.Instance.Grid.CellSize;
            Vector2 dimensions = token.Data.dimensions;
            float targetSize = dimensions.x >= dimensions.y ? cellSize * (dimensions.y / Session.Instance.Grid.Unit.scale) : cellSize * (dimensions.x / Session.Instance.Grid.Unit.scale);

            // Apply scaling
            Rect.sizeDelta = new Vector2(100 * cellSize * (dimensions.x / Session.Instance.Grid.Unit.scale), 100 * cellSize * (dimensions.y / Session.Instance.Grid.Unit.scale));
            uICanvas.transform.localScale = new Vector3(targetSize, targetSize, 1.0f);
        }
    }
}