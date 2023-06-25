using FunkyCode;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class Wall : MonoBehaviour
    {
        [SerializeField] private Image doorIcon;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;

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
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSettingChanged.RemoveListener(ToggleUI);
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

        private void UpdateData()
        {
            float cellSize = Session.Instance.Grid.CellSize;

            // Update canvas
            canvas.transform.localScale = new Vector3(cellSize * 0.03f, cellSize * 0.03f, 1.0f);
            canvas.sortingOrder = ConnectionManager.Info.isMaster ? 1 : 0;
            canvas.transform.position = (data.points[0] + data.points[1]) / 2f;

            HandleCollider();

            // Update layer
            gameObject.layer = 8;
            if (data.type == WallType.Invisible)
            {
                gameObject.layer = 7;
                lightCollider.enabled = false;
            }

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
        }

        public void LoadData(WallData _data)
        {
            data = _data;
            loaded = false;

            // Disable UI
            ToggleUI(false);
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
    }
}