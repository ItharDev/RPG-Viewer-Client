using System.Runtime.Serialization;
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

        private Canvas canvas;
        private EdgeCollider2D edgeCollider;
        private LightCollider2D lightCollider;
        private bool loaded;
        private WallData data;

        private void OnEnable()
        {
            // Get reference of out canvas and colliders
            if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
            if (edgeCollider == null) edgeCollider = GetComponent<EdgeCollider2D>();
            if (lightCollider == null) lightCollider = GetComponent<LightCollider2D>();

            // Add event listeners
            Events.OnSettingChanged.AddListener(ToggleUI);
            Events.OnViewChanged.AddListener(HandleView);
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

            if (SettingsHandler.Instance.LastView == GameView.Player) HandleLayers();
        }

        private void HandleLayers()
        {
            bool showDoor = false;
            foreach (var token in Session.Instance.TokenManager.Tokens)
            {
                float distance = Vector2.Distance(canvas.transform.position, token.Value.transform.position);
                bool canOpen = token.Value.Permission.type != PermissionType.None;

                if (distance <= Session.Instance.Grid.CellSize && canOpen) showDoor = true;
            }

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
        }
        private void HandleCollider()
        {
            // Update colliders
            edgeCollider.points = data.points.ToArray();
            edgeCollider.enabled = !data.open;

            lightCollider.enabled = edgeCollider.enabled;
            lightCollider.maskType = LightCollider2D.MaskType.None;

            // Update layer
            gameObject.layer = 8;
            if (data.type == WallType.Invisible)
            {
                gameObject.layer = 7;
                lightCollider.enabled = false;
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
        }

        public void OnClick(BaseEventData eventData)
        {
            if (SettingsHandler.Instance.LastView == GameView.Player && canvas.sortingLayerName == "Defaulg") return;
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