using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class DarknessConfiguration : MonoBehaviour
    {
        [SerializeField] private Toggle enabledToggle;
        [SerializeField] private Image colorButton;
        [SerializeField] private Toggle globalToggle;
        [SerializeField] private TMP_InputField visionInput;
        [SerializeField] private FlexibleColorPicker colorPicker;

        private RectTransform rect;
        private LightingSettings data;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnLightingChanged.AddListener(ReloadData);
        }
        private void OnDisable()
        {
            // Add event listeners
            Events.OnLightingChanged.RemoveListener(ReloadData);
        }

        private void ReloadData(LightingSettings newData, bool globalUpdate)
        {
            if (globalUpdate) data = newData;
        }

        public void OpenPanel(LightingSettings _data)
        {
            gameObject.SetActive(true);
            LeanTween.size(rect, new Vector2(250.0f, 146.0f), 0.2f);
            LoadData(_data);
        }
        public void ClosePanel(bool saveData)
        {
            LeanTween.size(rect, new Vector2(250.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                if (saveData) SaveData();
                else Events.OnLightingChanged?.Invoke(Session.Instance.Settings.darkness, true);
                gameObject.SetActive(false);
            });
        }
        public void OpenColor()
        {
            Color initColor = data.color;
            initColor.a = 1.0f;
            colorPicker.SetColor(data.color);
            colorPicker.gameObject.SetActive(true);
            colorButton.color = initColor;
        }
        public void LoadData(LightingSettings _data)
        {
            data = _data;
            _data.color.a = 1.0f;
            colorButton.color = _data.color;
            colorPicker.SetColor(data.color);
            globalToggle.isOn = _data.globalLighting;

            visionInput.text = data.visionRange.ToString();
            ((TMP_Text)visionInput.placeholder).text = Session.Instance.Grid.Unit.name;
        }
        public void ChangeColor(Color color)
        {
            data.color = color;
            color.a = 1.0f;
            colorButton.color = color;
            Events.OnLightingChanged?.Invoke(data, false);
        }

        private void SaveData()
        {
            data.enabled = enabledToggle.isOn;
            data.globalLighting = globalToggle.isOn;
            if (string.IsNullOrEmpty(visionInput.text)) visionInput.text = "0";
            float visionRange = float.Parse(visionInput.text);
            if (visionRange < 0.0f) visionRange = 0.0f;
            data.visionRange = visionRange;

            SocketManager.EmitAsync("modify-lighting", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                Events.OnLightingChanged?.Invoke(Session.Instance.Settings.darkness, true);

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(data));
        }
    }
}