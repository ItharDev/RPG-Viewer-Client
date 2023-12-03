using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class LightConfiguration : MonoBehaviour
    {
        [SerializeField] private PresetList presetList;
        [SerializeField] private Toggle enabledToggle;
        [SerializeField] private TMP_InputField directionInput;
        [SerializeField] private FlexibleColorPicker colorPicker;

        [Space]
        [SerializeField] private LightInput primary;
        [SerializeField] private LightInput secondary;

        private RectTransform rect;
        private string id;
        private LightData info;
        private PresetData lightData;
        private Action<LightData, PresetData> callback;
        private bool editingPrimary;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnPresetModified.AddListener(LoadPreset);
            Events.OnPresetRemoved.AddListener(RemovePreset);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnPresetModified.RemoveListener(LoadPreset);
            Events.OnPresetRemoved.RemoveListener(RemovePreset);
        }

        public void OpenPresets()
        {
            if (presetList.gameObject.activeInHierarchy) return;
            presetList.LoadData(ApplyPreset);
        }

        private void LoadPreset(string id, PresetData data)
        {
            if (lightData.id == id) ApplyPreset(data);
        }
        private void RemovePreset(string id, PresetData data)
        {
            if (lightData.id == id)
            {
                lightData.id = data.id;
                ApplyPreset(data);
            }
        }
        private void ApplyPreset(PresetData data)
        {
            lightData = data;
            info.id = data.id;

            // Primary
            primary.effectDropdown.value = data.primary.effect.type;
            primary.radiusInput.text = data.primary.radius.ToString();
            ((TMP_Text)primary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            primary.angleInput.text = data.primary.angle.ToString();
            directionInput.text = info.rotation.ToString();
            primary.intensityInput.text = ((int)(data.primary.color.a * 100.0f)).ToString();
            data.primary.color.a = 1.0f;
            primary.colorButton.color = data.primary.color;
            primary.strengthInput.text = data.primary.effect.strength.ToString();
            primary.frequencyInput.text = data.primary.effect.frequency.ToString();

            // Secondary
            secondary.effectDropdown.value = data.secondary.effect.type;
            secondary.radiusInput.text = data.secondary.radius.ToString();
            ((TMP_Text)secondary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            secondary.angleInput.text = data.secondary.angle.ToString();
            directionInput.text = info.rotation.ToString();
            secondary.intensityInput.text = ((int)(data.secondary.color.a * 100.0f)).ToString();
            data.secondary.color.a = 1.0f;
            secondary.colorButton.color = data.secondary.color;
            secondary.strengthInput.text = data.secondary.effect.strength.ToString();
            secondary.frequencyInput.text = data.secondary.effect.frequency.ToString();
        }
        public void ClosePanel(bool saveData = true)
        {
            LeanTween.size(rect, new Vector2(450.0f, 0.0f), 0.1f).setOnComplete(() =>
            {
                if (saveData) SaveData();
                Destroy(gameObject);
            });
        }
        public void OpenColor(bool isPrimary)
        {
            editingPrimary = isPrimary;
            colorPicker.gameObject.SetActive(true);
            colorPicker.SetColor(isPrimary ? lightData.primary.color : lightData.secondary.color);
        }
        public void ChangeColor(Color color)
        {
            if (editingPrimary)
            {
                lightData.primary.color = color;
                primary.intensityInput.text = ((int)(color.a * 100.0f)).ToString();
                color.a = 1.0f;
                primary.colorButton.color = color;
            }
            else
            {
                lightData.secondary.color = color;
                secondary.intensityInput.text = ((int)(color.a * 100.0f)).ToString();
                color.a = 1.0f;
                secondary.colorButton.color = color;
            }

        }
        public void ChangeIntensity(bool isPrimary)
        {
            editingPrimary = isPrimary;
            if (isPrimary)
            {
                lightData.primary.color.a = float.Parse(primary.intensityInput.text) * 0.01f;
            }
            else
            {
                lightData.secondary.color.a = float.Parse(secondary.intensityInput.text) * 0.01f;
            }

        }
        public void LoadData(string _id, LightData _info, PresetData data, Action<LightData, PresetData> _callback)
        {
            id = _id;
            lightData = data;
            info = _info;
            callback = _callback;

            LoadLighting(lightData);
        }
        public void LoadLighting(PresetData preset)
        {
            lightData = preset;
            enabledToggle.isOn = info.enabled;

            // Primary
            primary.effectDropdown.value = preset.primary.effect.type;
            primary.radiusInput.text = preset.primary.radius.ToString();
            ((TMP_Text)primary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            primary.angleInput.text = preset.primary.angle.ToString();
            directionInput.text = info.rotation.ToString();
            primary.intensityInput.text = ((int)(preset.primary.color.a * 100.0f)).ToString();
            preset.primary.color.a = 1.0f;
            primary.colorButton.color = preset.primary.color;
            primary.strengthInput.text = preset.primary.effect.strength.ToString();
            primary.frequencyInput.text = preset.primary.effect.frequency.ToString();

            // Secondary
            secondary.effectDropdown.value = preset.secondary.effect.type;
            secondary.radiusInput.text = preset.secondary.radius.ToString();
            ((TMP_Text)secondary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            secondary.angleInput.text = preset.secondary.angle.ToString();
            directionInput.text = info.rotation.ToString();
            secondary.intensityInput.text = ((int)(preset.secondary.color.a * 100.0f)).ToString();
            preset.secondary.color.a = 1.0f;
            secondary.colorButton.color = preset.secondary.color;
            secondary.strengthInput.text = preset.secondary.effect.strength.ToString();
            secondary.frequencyInput.text = preset.secondary.effect.frequency.ToString();
        }

        private void SaveData()
        {
            // Primary
            float.TryParse(primary.radiusInput.text, out lightData.primary.radius);
            float.TryParse(primary.angleInput.text, out lightData.primary.angle);
            int.TryParse(directionInput.text, out info.rotation);
            lightData.primary.color = new Color(primary.colorButton.color.r, primary.colorButton.color.g, primary.colorButton.color.b, lightData.primary.color.a);
            lightData.primary.effect.type = primary.effectDropdown.value;
            info.enabled = enabledToggle.isOn;
            float.TryParse(primary.strengthInput.text, out lightData.primary.effect.strength);
            float.TryParse(primary.frequencyInput.text, out lightData.primary.effect.frequency);

            // Secondary
            float.TryParse(secondary.radiusInput.text, out lightData.secondary.radius);
            float.TryParse(secondary.angleInput.text, out lightData.secondary.angle);
            int.TryParse(directionInput.text, out info.rotation);
            lightData.secondary.color = new Color(secondary.colorButton.color.r, secondary.colorButton.color.g, secondary.colorButton.color.b, lightData.secondary.color.a);
            lightData.secondary.effect.type = secondary.effectDropdown.value;
            info.enabled = enabledToggle.isOn;
            float.TryParse(secondary.strengthInput.text, out lightData.secondary.effect.strength);
            float.TryParse(secondary.frequencyInput.text, out lightData.secondary.effect.frequency);

            if (!string.IsNullOrEmpty(lightData.id))
            {
                if (lightData != PresetManager.Instance.GetPreset(lightData.id))
                {
                    lightData.id = id;
                    info.id = id;
                }
            }

            // Send callback
            callback?.Invoke(info, lightData);
        }

        [Serializable]
        private struct LightInput
        {
            public TMP_InputField radiusInput;
            public TMP_InputField angleInput;
            public TMP_InputField intensityInput;
            public Image colorButton;

            public TMP_Dropdown effectDropdown;
            public TMP_InputField strengthInput;
            public TMP_InputField frequencyInput;
        }
    }
}