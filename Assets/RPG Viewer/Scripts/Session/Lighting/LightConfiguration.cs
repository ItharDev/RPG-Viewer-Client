using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class LightConfiguration : MonoBehaviour
    {
        [SerializeField] private PresetList presetList;
        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private Toggle enabledToggle;
        [SerializeField] private TMP_InputField radiusInput;
        [SerializeField] private TMP_InputField angleInput;
        [SerializeField] private TMP_InputField directionInput;
        [SerializeField] private TMP_InputField intensityInput;
        [SerializeField] private Image colorButton;
        [SerializeField] private TMP_InputField strengthInput;
        [SerializeField] private TMP_InputField frequencyInput;
        [SerializeField] private FlexibleColorPicker colorPicker;

        private RectTransform rect;
        private string id;
        private LightData info;
        private PresetData lightData;
        private Action<LightData, PresetData> callback;

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
            effectDropdown.value = data.effect.type;
            radiusInput.text = data.radius.ToString();
            ((TMP_Text)radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            angleInput.text = data.angle.ToString();
            directionInput.text = info.rotation.ToString();
            intensityInput.text = ((int)(data.color.a * 100.0f)).ToString();
            colorButton.color = data.color;
            strengthInput.text = data.effect.strength.ToString();
            frequencyInput.text = data.effect.frequency.ToString();
        }
        public void ClosePanel(bool saveData = true)
        {
            LeanTween.size(rect, new Vector2(300, 0.0f), 0.1f).setOnComplete(() =>
            {
                if (saveData) SaveData();
                Destroy(gameObject);
            });
        }
        public void OpenColor()
        {
            colorPicker.gameObject.SetActive(true);
            colorPicker.SetColor(lightData.color);
        }
        public void ChangeColor(Color color)
        {
            lightData.color = color;
            intensityInput.text = ((int)(color.a * 100.0f)).ToString();
            color.a = 1.0f;
            colorButton.color = color;
        }
        public void ChangeIntensity()
        {
            lightData.color.a = float.Parse(intensityInput.text) * 0.01f;
            colorPicker.SetColor(lightData.color);
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
            effectDropdown.value = preset.effect.type;
            radiusInput.text = preset.radius.ToString();
            ((TMP_Text)radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            angleInput.text = preset.angle.ToString();
            directionInput.text = info.rotation.ToString();
            intensityInput.text = ((int)(preset.color.a * 100.0f)).ToString();
            preset.color.a = 1.0f;
            colorButton.color = preset.color;
            strengthInput.text = preset.effect.strength.ToString();
            frequencyInput.text = preset.effect.frequency.ToString();
        }

        private void SaveData()
        {
            float.TryParse(radiusInput.text, out lightData.radius);
            int.TryParse(angleInput.text, out lightData.angle);
            int.TryParse(directionInput.text, out info.rotation);
            lightData.color = new Color(colorButton.color.r, colorButton.color.g, colorButton.color.b, lightData.color.a);
            lightData.effect.type = effectDropdown.value;
            info.enabled = enabledToggle.isOn;
            float.TryParse(strengthInput.text, out lightData.effect.strength);
            float.TryParse(frequencyInput.text, out lightData.effect.frequency);

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
    }
}