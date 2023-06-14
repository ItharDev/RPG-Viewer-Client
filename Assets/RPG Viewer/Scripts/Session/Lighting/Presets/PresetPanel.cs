using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class PresetPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private TMP_InputField radiusInput;
        [SerializeField] private TMP_InputField intensityInput;
        [SerializeField] private Image colorButton;
        [SerializeField] private TMP_InputField strengthInput;
        [SerializeField] private TMP_InputField frequencyInput;
        [SerializeField] private bool hideWhenMinimised;
        [SerializeField] private FlexibleColorPicker colorPicker;

        public string Id { get { return data.id; } }

        private PresetData data;
        private Action<PresetData> callback;

        public void LoadData(PresetData _data, Action<PresetData> onSelected)
        {
            LeanTween.size((RectTransform)transform, new Vector2(300.0f, 239.0f), 0.2f);

            data = _data;
            callback = onSelected;
            _data.color.a = 1.0f;

            nameInput.text = data.name;
            effectDropdown.value = data.effect.type;
            radiusInput.text = data.radius.ToString();
            intensityInput.text = data.intensity.ToString();
            colorButton.color = _data.color;
            strengthInput.text = data.effect.strength.ToString();
            frequencyInput.text = data.effect.frequency.ToString();
        }
        public void OpenColor()
        {
            colorPicker.gameObject.SetActive(true);
            colorPicker.SetColor(data.color);
        }
        public void ChangeColor(Color color)
        {
            color.a = 1.0f;
            colorButton.color = color;
        }

        public void ClosePanel(bool saveData = true)
        {
            LeanTween.size((RectTransform)transform, new Vector2(0.0f, 239.0f), 0.2f).setOnComplete(() =>
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
            float.TryParse(radiusInput.text, out data.radius);
            float.TryParse(intensityInput.text, out data.intensity);
            data.color = new Color(colorButton.color.r, colorButton.color.g, colorButton.color.b, data.color.a);
            data.effect.type = effectDropdown.value;
            float.TryParse(strengthInput.text, out data.effect.strength);
            float.TryParse(frequencyInput.text, out data.effect.frequency);

            // Send callback
            callback?.Invoke(data);
        }
    }
}