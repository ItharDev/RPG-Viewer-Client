using Networking;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class Light : MonoBehaviour
    {
        [SerializeField] private LightSource source;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite onIcon;
        [SerializeField] private Sprite offIcon;

        [Space]
        [SerializeField] private Color offColor;
        [SerializeField] private Color onColor;
        [SerializeField] private LightConfiguration configPrefab;

        private LightConfiguration activeConfig;
        private Canvas canvas;
        private string id;
        private PresetData data;
        private LightData info;
        private bool loaded;
        private bool dragging;

        private void Awake()
        {
            // Get reference of our canvas
            if (canvas == null) canvas = GetComponentInChildren<Canvas>();
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnPresetModified.AddListener(ModifyPreset);
        }
        private void OnDisable()
        {
            // remove event listeners
            Events.OnPresetModified.RemoveListener(ModifyPreset);
        }
        private void Update()
        {
            if (!loaded && Session.Instance.Grid.Grid != null)
            {
                loaded = true;
                UpdateData();
            }

            if (dragging)
            {
                // Update our position when being dragged
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0.0f;
                transform.localPosition = mousePos;
            }
        }

        private void ModifyPreset(string _id, PresetData _data)
        {
            // Return if the effect doesn't affect us
            if (data.id != _id) return;

            data = _data;
            UpdateData();
        }

        private void UpdateData()
        {
            float cellSize = Session.Instance.Grid.CellSize;

            // Update our position and scale
            transform.position = info.position;
            transform.eulerAngles = new Vector3(0.0f, 0.0f, info.rotation);
            canvas.transform.localScale = new Vector3(cellSize * 0.03f, cellSize * 0.03f, 1.0f);

            // Enable UI
            icon.sprite = info.enabled ? onIcon : offIcon;
            icon.color = info.enabled ? onColor : offColor;
            bool enable = SettingsHandler.Instance.Setting.ToString().ToLower().Contains("lighting");
            source.LoadData(data);
            source.Toggle(info.enabled);
        }

        public void LoadData(string _id, LightData _info, PresetData _data)
        {
            id = _id;
            info = _info;
            data = _data;

            // Set our data to dirty
            loaded = false;
        }
        public void LoadData(PresetData _data)
        {
            data = _data;

            // Set our data to dirty
            loaded = false;
        }
        public void LoadData(LightData _info)
        {
            info = _info;

            // Set our data to dirty
            loaded = false;
        }
        public void Toggle(bool enabled)
        {
            info.enabled = enabled;

            // Set our data to dirty
            loaded = false;
        }
        public void BeginDrag(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;

            if (pointerData.button != PointerEventData.InputButton.Left) return;

            dragging = true;
        }
        public void EndDrag(BaseEventData eventData)
        {
            dragging = false;
            LightData newData = info;
            newData.position = transform.localPosition;
            SocketManager.EmitAsync("move-light", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    transform.localPosition = newData.position;
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id, JsonUtility.ToJson(newData));
        }
        public void OnClick(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.dragging) return;

            if (pointerData.button == PointerEventData.InputButton.Left)
            {
                if (LightTools.Instance.Mode == LightMode.Create) ToggleLight();
                else DeleteLight();
            }
            if (pointerData.button == PointerEventData.InputButton.Right) ModifyLight();
        }

        private void ToggleLight()
        {
            SocketManager.EmitAsync("toggle-light", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id, !info.enabled);
        }
        private void DeleteLight()
        {
            if (activeConfig != null) Destroy(activeConfig.gameObject);

            SocketManager.EmitAsync("remove-light", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
        private void ModifyLight()
        {
            if (activeConfig != null) return;

            activeConfig = Instantiate(configPrefab);
            activeConfig.transform.SetParent(UICanvas.Instance.transform);
            activeConfig.transform.localPosition = Vector3.zero;
            activeConfig.transform.SetAsLastSibling();
            activeConfig.LoadData(id, info, data, (newInfo, newData) =>
            {
                SocketManager.EmitAsync("modify-light", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, id, JsonUtility.ToJson(newInfo), JsonUtility.ToJson(newData));
            });
        }
    }
}