using System.Collections.Generic;
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

        [Header("UI")]
        [SerializeField] private Image image;
        [SerializeField] private Image outlineImage;
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject rotateButton;

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

        public RectTransform Rect { get { return canvas.GetComponent<RectTransform>(); } }
        public bool Editing { get { return true /* TODO: */; } }

        private Token token;
        private Vector2 screenPos;
        private Outline outline;
        private List<Token> rotatedTokens = new List<Token>();

        private Shader outlineShader;
        private Shader regularShader;

        private void OnEnable()
        {
            // Get reference of main class and outline
            if (token == null) token = GetComponent<Token>();
            if (outline == null) outline = outlineImage.GetComponent<Outline>();

            outlineShader = Shader.Find("GUI/Text Shader");

            // Add event listeners
            Events.OnToolChanged.AddListener(HandleRaycast);
            Events.OnTokenSelected.AddListener(UpdateSorting);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnToolChanged.RemoveListener(HandleRaycast);
            Events.OnTokenSelected.RemoveListener(UpdateSorting);
        }
        private void Update()
        {
            // Return if token is not selected
            if (!token.Selected) return;

            HandleElevation();
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
                // Trim out the 'ft' part of the elevation and parse it to integer
                string elevation = elevationInput.text.TrimEnd('f', 't', ' ');
                int elevationValue = string.IsNullOrEmpty(elevation) ? 0 : int.Parse(elevation);

                // Modify elevation value
                elevationValue += Input.GetKey(KeyCode.PageUp) ? 5 : -5;
                string newElevation = $"{elevationValue} ft";
            }
        }

        public void SetAlpha(float value)
        {
            // Update group alpha
            canvasGroup.alpha = value;
        }
        public void SetRotation(float value)
        {
            image.transform.eulerAngles = new Vector3(0.0f, 0.0f, value);
        }
        public void SetImage(Sprite sprite)
        {
            // Update image sprite and color
            image.sprite = sprite;
            outlineImage.sprite = sprite;
            outlineImage.material.shader = outlineShader;
            image.color = Color.white;
        }
        public void Reload()
        {
            // Reload all UI elements
            SetRotation(token.Data.rotation);
            UpdateSorting();
            Resize();

            // TODO: Update all elements
        }
        public void EnableRaycasting(bool enable)
        {
            // Enable / disable raycasting
            image.raycastTarget = enable;
        }

        public void OnPointerClick(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = eventData as PointerEventData;
        }
        public void OnPointerEnter(BaseEventData eventData)
        {
            outline.enabled = true;
        }
        public void OnPointerExit(BaseEventData eventData)
        {
            outline.enabled = false;
        }

        public void BeginRotate(BaseEventData eventData)
        {
            // Clear previous nearby tokens
            rotatedTokens.Clear();

            // Return if we aren't dragging whit LMB
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;

            // Store our position in screen space
            screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 mouseDistance = (Vector2)Input.mousePosition - screenPos;

            // Calculate angle offset
            float offset = (Mathf.Atan2(transform.right.y, transform.right.x) - Mathf.Atan2(mouseDistance.y, mouseDistance.x)) * Mathf.Rad2Deg;

            // Return if this is not a mount
            if (token.Data.type != TokenType.Mount) return;

            // Check for nearby tokens
            rotatedTokens = token.GetNearbyTokens();

            for (int i = 0; i < rotatedTokens.Count; i++)
            {
                // Initialise rotation
                rotatedTokens[i].transform.parent = image.transform;
            }
        }
        public void Rotate(BaseEventData eventData)
        {
            // Calculate new angle
            Vector2 mouseDistance = (Vector2)Input.mousePosition - screenPos;
            float angle = Mathf.Atan2(mouseDistance.y, mouseDistance.x) * Mathf.Rad2Deg;

            // Rotate token
            image.transform.eulerAngles = new Vector3(0, 0, angle);
        }
        public void EndRotate(BaseEventData eventData)
        {
            for (int i = 0; i < rotatedTokens.Count; i++)
            {
                // Check if token is still there
                if (rotatedTokens[i] == null) continue;

                // Update nearby tokens position
                token.transform.parent = transform.parent;
                token.Movement.MountedRotation();
            }

            // Update our rotation
            token.Movement.FinishRotation(image.transform.eulerAngles.z, false);
        }

        public void UpdateSorting()
        {
            // Move token to correct sorting layer
            switch (token.Data.type)
            {
                case TokenType.Character:
                    canvas.sortingLayerName = "Characters";

                    // Check if we are highlighted
                    if (token.Data.highlighted) canvas.sortingLayerName = "Highlighted Characters";

                    // Check if we are dead
                    if (token.Conditions.IsDead) canvas.sortingLayerName = "Dead Characters";

                    // Check if we are owners
                    if (token.IsOwner) canvas.sortingLayerName = "Owned Characters";
                    break;
                case TokenType.Mount:
                    canvas.sortingLayerName = "Mounts";

                    // Check if we are highlighted
                    if (token.Data.highlighted) canvas.sortingLayerName = "Highlighted Mounts";

                    // Check if we are dead
                    if (token.Conditions.IsDead) canvas.sortingLayerName = "Dead Mounts";

                    // Check if we are owners
                    if (token.IsOwner) canvas.sortingLayerName = "Owned Mounts";
                    break;
                case TokenType.Item:
                    canvas.sortingLayerName = "Items";

                    // Check if we are highlighted
                    if (token.Data.highlighted) canvas.sortingLayerName = "Highlighted Items";

                    // Check if we are owners
                    if (token.IsOwner) canvas.sortingLayerName = "Owned Items";
                    break;
            }
        }
        public void Resize()
        {
            // Calculate correct canvas scale
            float cellSize = 1;
            Vector2Int dimensions = token.Data.dimensions;
            float targetSize = dimensions.x >= dimensions.y ? cellSize * (dimensions.y / 5.0f) : cellSize * (dimensions.x / 5.0f);

            // Apply scaling
            Rect.sizeDelta = new Vector2(100 * cellSize * (dimensions.x / 5.0f), 100 * cellSize * (dimensions.y / 5.0f));
            uICanvas.transform.localScale = new Vector3(targetSize, targetSize, 1.0f);
        }
    }
}