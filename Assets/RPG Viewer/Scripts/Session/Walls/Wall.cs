using System;
using System.Collections.Generic;
using System.Linq;
using FunkyCode;
using Networking;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class Wall : MonoBehaviour
    {
        [SerializeField] private Image doorIcon;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;
        [SerializeField] private Color regularColor;
        [SerializeField] private Color secretColor;
        [SerializeField] private GameObject lockedIcon;
        [SerializeField] private GameObject info;

        private Canvas canvas;
        private EdgeCollider2D edgeCollider;
        private CircleCollider2D interactableCollider;
        private LightCollider2D lightCollider;
        private bool loaded;
        private WallData data;
        private bool showDoor;
        private List<Token> tokensInRange = new List<Token>();

        private void OnEnable()
        {
            // Get reference of out canvas and colliders
            if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
            if (edgeCollider == null) edgeCollider = GetComponent<EdgeCollider2D>();
            if (interactableCollider == null) interactableCollider = GetComponentInChildren<CircleCollider2D>();
            if (lightCollider == null) lightCollider = GetComponent<LightCollider2D>();

            // Add event listeners
            Events.OnSettingChanged.AddListener(ToggleUI);
            Events.OnViewChanged.AddListener(HandleView);
            Events.OnTokenCreated.AddListener(HandleLayers);
            Events.OnTokenModified.AddListener(HandleLayers);
            Events.OnSceneLoaded.AddListener(HandleLayers);
        }

        private void HandleLayers(SceneData data)
        {
            HandleLayers();
        }

        private void HandleLayers(string id, TokenData data)
        {
            HandleLayers();
        }

        private void HandleLayers(TokenData data)
        {
            HandleLayers();
        }

        public void HandleLocked()
        {
            if (!ConnectionManager.Info.isMaster) info.SetActive(!info.activeInHierarchy);
        }

        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSettingChanged.RemoveListener(ToggleUI);
            Events.OnViewChanged.RemoveListener(HandleView);
        }
        private void Update()
        {
            // Load settings after grid has been initialised
            if (!loaded && Session.Instance.Grid.Grid != null)
            {
                loaded = true;
                UpdateData();
            }
        }

        private void HandleLayers()
        {
            bool state = false;

            foreach (var token in tokensInRange)
            {
                if (token.Data.type == TokenType.Character && token.Permission.type == PermissionType.Controller && token.Visibility.visible) state = true;
            }

            // New token has entered the radius
            if (state)
            {
                bool enableDoor = false;
                foreach (var item in tokensInRange)
                {
                    bool hasView = true;
                    // Check if the token can see the door
                    RaycastHit2D[] results = Physics2D.RaycastAll(canvas.transform.position, item.transform.position - canvas.transform.position, Vector2.Distance(item.transform.position, canvas.transform.position) + Session.Instance.Grid.CellSize * 0.01f);
                    for (int i = 0; i < results.Length; i++)
                    {
                        bool isWall = results[i].transform.gameObject.CompareTag("Wall");
                        bool hitDetectionCollider = results[i].collider.GetType() == typeof(CircleCollider2D);
                        bool hitOurSelves = results[i].transform.gameObject == gameObject;
                        if (isWall && !hitDetectionCollider && !hitOurSelves) hasView = false;
                    }
                    if (hasView) enableDoor = true;
                }
                showDoor = enableDoor;
            }
            else showDoor = false;

            canvas.sortingOrder = showDoor ? 1 : 0;
            canvas.sortingLayerName = showDoor ? "Above Fog" : "Default";
        }

        private void UpdateData()
        {
            float cellSize = Session.Instance.Grid.CellSize;

            // Update canvas
            canvas.transform.localScale = new Vector3(cellSize * 0.03f, cellSize * 0.03f, 1.0f);
            canvas.sortingOrder = ConnectionManager.Info.isMaster ? 1 : 0;
            canvas.sortingLayerName = SettingsHandler.Instance.LastView == GameView.Clear ? "Above Fog" : "Default";

            canvas.transform.position = (data.points[0] + data.points[1]) / 2f;
            doorIcon.sprite = data.open ? openSprite : closedSprite;
            lockedIcon.SetActive(data.locked);
            info.SetActive(ConnectionManager.Info.isMaster);
            doorIcon.color = data.type == WallType.Hidden_Door ? secretColor : regularColor;

            HandleCollider();

            if (ConnectionManager.Info.isMaster)
            {
                bool toolActivated = SettingsHandler.Instance.Setting.ToString().ToLower().Contains("walls");
                bool isDoor = data.type.ToString().ToLower().Contains("door");

                // Show UI if tool is not selected
                if (!toolActivated) ToggleUI(isDoor);
            }
            else ToggleUI(data.type == WallType.Door);

            lightCollider.Initialize();
            HandleLayers();
        }
        private void HandleCollider()
        {
            // Update colliders
            edgeCollider.points = data.points.ToArray();
            edgeCollider.enabled = !data.open;

            lightCollider.enabled = edgeCollider.enabled;
            lightCollider.maskType = LightCollider2D.MaskType.None;

            interactableCollider.radius = Session.Instance.Grid.CellSize * 2.0f;
            interactableCollider.offset = (data.points[0] + data.points[1]) / 2f;

            // Update layer
            gameObject.layer = 8;
            if (data.type == WallType.Invisible)
            {
                gameObject.layer = 7;
                lightCollider.enabled = false;
            }
            else if (data.type == WallType.Fog)
            {
                gameObject.layer = 6;
                lightCollider.enabled = true;
                edgeCollider.enabled = false;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer == 11) tokensInRange.Add(other.GetComponent<Token>());
            if (SettingsHandler.Instance.LastView == GameView.Player && data.type == WallType.Door) HandleLayers();
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            Token token = other.GetComponent<Token>();
            if (other.gameObject.layer == 11) tokensInRange.Remove(token);
            if (SettingsHandler.Instance.LastView == GameView.Player && data.type == WallType.Door) HandleLayers();
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            if (SettingsHandler.Instance.LastView == GameView.Player && data.type == WallType.Door) HandleLayers();
            else
            {
                canvas.sortingOrder = 1;
                canvas.sortingLayerName = "Above Fog";
            }
        }

        public void LoadData(WallData _data)
        {
            data = _data;
            loaded = false;

            ToggleUI(false);
            HandleCollider();
        }
        private void ToggleUI(Setting setting)
        {
            // Enable / disable UI based on setting
            bool toolSelected = setting.ToString().ToLower().Contains("walls");
            bool isDoor = data.type.ToString().ToLower().Contains("door");

            // Show UI if tool is not selected
            ToggleUI(!toolSelected && isDoor);
        }
        private void ToggleUI(bool enabled)
        {
            // Enable / disable canvas
            canvas.enabled = enabled;
        }
        private void HandleView(GameView view)
        {
            canvas.sortingLayerName = view == GameView.Clear ? "Above Fog" : "Default";

            // Enable / disable UI based on setting
            bool toolSelected = SettingsHandler.Instance.Setting.ToString().ToLower().Contains("walls");
            bool isDoor = data.type.ToString().ToLower().Contains("door");

            // Show UI if tool is not selected
            ToggleUI(!toolSelected && isDoor);
        }

        public void OnClick(BaseEventData eventData)
        {
            if (SettingsHandler.Instance.LastView == GameView.Player && canvas.sortingLayerName == "Default") return;
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.button == PointerEventData.InputButton.Left) ToggleDoor();
            if (pointerData.button == PointerEventData.InputButton.Right) LockDoor();
        }

        private void ToggleDoor()
        {
            if (!ConnectionManager.Info.isMaster && data.locked)
            {
                MessageManager.QueueMessage("This door is locked");
                return;
            }

            WallData newData = data;
            newData.open = !newData.open;
            SocketManager.EmitAsync("modify-wall", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(newData));
        }
        private void LockDoor()
        {
            if (!ConnectionManager.Info.isMaster) return;

            WallData newData = data;
            newData.locked = !newData.locked;
            SocketManager.EmitAsync("modify-wall", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(newData));
        }
    }
}