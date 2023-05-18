using FunkyCode;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class Light : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Sprite onIcon;
        [SerializeField] private Sprite offIcon;

        [Space]
        [SerializeField] private Color offColor;
        [SerializeField] private Color onColor;

        private Light2D source;
        private Canvas canvas;
        private LightData data;
        private bool loaded;

        private void OnEnable()
        {
            // Get reference of our light source and canvas
            if (source == null) source = GetComponent<Light2D>();
            if (canvas == null) canvas = GetComponentInChildren<Canvas>();

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
            if (!loaded && Session.Instance.Grid.Grid != null)
            {
                loaded = true;
                UpdateData();
            }
        }

        private void UpdateData()
        {
            float cellSize = Session.Instance.Grid.CellSize;

            // Update our position and scale
            transform.position = data.position;
            canvas.transform.localScale = new Vector3(cellSize * 0.03f, cellSize * 0.03f, 1.0f);

            // Update the light source
            source.size = data.radius * 0.2f * cellSize;
            source.enabled = data.enabled;
            source.color = data.color;

            // Enable UI
            icon.sprite = data.enabled ? onIcon : offIcon;
            icon.color = data.enabled ? onColor : offColor;
            bool enable = SettingsHandler.Instance.Setting.ToString().Contains("Lighting");
            ToggleUI(enable);
        }

        public void LoadData(LightData _data)
        {
            data = _data;

            // Set our data to be dirty
            loaded = false;

            // Disable UI
            ToggleUI(false);
        }
        private void ToggleUI(Setting setting)
        {
            // Toggle ui based on tool state
            bool enable = SettingsHandler.Instance.Setting.ToString().Contains("Lighting");
            ToggleUI(enable);
        }
        private void ToggleUI(bool enabled)
        {
            // Enable / disable UI
            canvas.enabled = enabled;
        }
    }
}