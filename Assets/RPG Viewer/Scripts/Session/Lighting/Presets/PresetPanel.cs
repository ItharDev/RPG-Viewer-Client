using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class PresetPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private bool hideWhenMinimised;
        [SerializeField] private FlexibleColorPicker colorPicker;

        [Space]
        [SerializeField] private LightInput primary;
        [SerializeField] private LightInput secondary;

        public string Id { get { return data.id; } }

        private PresetData data;
        private Action<PresetData> callback;
        private bool editingPrimary;

        public void LoadData(PresetData _data, Action<PresetData> onSelected)
        {
            LeanTween.size((RectTransform)transform, new Vector2(450.0f, 268.0f), 0.2f);

            data = _data;
            callback = onSelected;
            _data.primary.color.a = 1.0f;
            _data.secondary.color.a = 1.0f;

            nameInput.text = data.name;

            // Primary
            primary.effectDropdown.value = data.primary.effect.type;
            primary.radiusInput.text = data.primary.radius.ToString();
            ((TMP_Text)primary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            primary.angleInput.text = data.primary.angle.ToString();
            primary.intensityInput.text = Mathf.RoundToInt(data.primary.color.a * 100.0f).ToString();
            primary.colorButton.color = _data.primary.color;
            primary.strengthInput.text = data.primary.effect.strength.ToString();
            primary.frequencyInput.text = data.primary.effect.frequency.ToString();

            // Secondary
            secondary.effectDropdown.value = data.secondary.effect.type;
            secondary.radiusInput.text = data.secondary.radius.ToString();
            ((TMP_Text)secondary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            secondary.angleInput.text = data.secondary.angle.ToString();
            secondary.intensityInput.text = Mathf.RoundToInt(data.secondary.color.a * 100.0f).ToString();
            secondary.colorButton.color = _data.secondary.color;
            secondary.strengthInput.text = data.secondary.effect.strength.ToString();
            secondary.frequencyInput.text = data.secondary.effect.frequency.ToString();
        }
        public void OpenColor(bool isPrimary)
        {
            editingPrimary = isPrimary;
            colorPicker.SetColor(isPrimary ? data.primary.color : data.secondary.color);
            colorPicker.gameObject.SetActive(true);
        }
        public void ChangeColor(Color color)
        {
            if (editingPrimary)
            {
                data.primary.color = color;
                primary.intensityInput.text = Mathf.RoundToInt(color.a * 100.0f).ToString();
                color.a = 1.0f;
                primary.colorButton.color = color;
            }
            else
            {
                data.secondary.color = color;
                secondary.intensityInput.text = Mathf.RoundToInt(color.a * 100.0f).ToString();
                color.a = 1.0f;
                secondary.colorButton.color = color;
            }
        }
        public void ChangeIntensity(bool isPrimary)
        {
            editingPrimary = isPrimary;
            if (isPrimary)
            {
                data.primary.color.a = float.Parse(primary.intensityInput.text) * 0.01f;
            }
            else
            {
                data.secondary.color.a = float.Parse(secondary.intensityInput.text) * 0.01f;
            }
        }

        public void ClosePanel(bool saveData = true)
        {
            LeanTween.size((RectTransform)transform, new Vector2(0.0f, 268.0f), 0.2f).setOnComplete(() =>
            {
                if (saveData) SaveData();
                if (hideWhenMinimised)
                {
                    gameObject.SetActive(false);
                    return;
                }
            });
        }
        private void SaveData()
        {
            data.name = string.IsNullOrEmpty(nameInput.text) ? data.name : nameInput.text;

            // Primary
            float.TryParse(primary.radiusInput.text, out data.primary.radius);
            float.TryParse(primary.angleInput.text, out data.primary.angle);
            data.primary.color = new Color(primary.colorButton.color.r, primary.colorButton.color.g, primary.colorButton.color.b, data.primary.color.a);
            data.primary.effect.type = primary.effectDropdown.value;
            float.TryParse(primary.strengthInput.text, out data.primary.effect.strength);
            float.TryParse(primary.frequencyInput.text, out data.primary.effect.frequency);

            // Secondary
            float.TryParse(secondary.radiusInput.text, out data.secondary.radius);
            float.TryParse(secondary.angleInput.text, out data.secondary.angle);
            data.secondary.color = new Color(secondary.colorButton.color.r, secondary.colorButton.color.g, secondary.colorButton.color.b, data.secondary.color.a);
            data.secondary.effect.type = secondary.effectDropdown.value;
            float.TryParse(secondary.strengthInput.text, out data.secondary.effect.strength);
            float.TryParse(secondary.frequencyInput.text, out data.secondary.effect.frequency);

            // Send callback
            callback?.Invoke(data);
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