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

        public string Id { get { return data.id; } }

        private LightData data;
        private Action<LightData> callback;

        public void LoadData(LightData _data, Action<LightData> onSelected)
        {
            LeanTween.size((RectTransform)transform, new Vector2(325.0f, 286.0f), 0.2f);

            data = _data;
            callback = onSelected;

            nameInput.text = data.name;
            effectDropdown.value = data.effect.type;
            radiusInput.text = data.radius.ToString();
            intensityInput.text = data.intensity.ToString();
            colorButton.color = data.color;
            strengthInput.text = data.effect.strength.ToString();
            frequencyInput.text = data.effect.frequency.ToString();
        }

        public void ClosePanel(bool saveData = true)
        {
            LeanTween.size((RectTransform)transform, new Vector2(0.0f, 286.0f), 0.2f).setOnComplete(() =>
            {
                if (saveData) SaveData();
            });
        }
        private void SaveData()
        {
            data.name = string.IsNullOrEmpty(nameInput.text) ? data.name : nameInput.text;
            float.TryParse(radiusInput.text, out data.radius);
            float.TryParse(intensityInput.text, out data.intensity);
            data.color = colorButton.color;
            data.effect.type = effectDropdown.value;
            float.TryParse(strengthInput.text, out data.effect.strength);
            float.TryParse(frequencyInput.text, out data.effect.frequency);

            // Send callback
            callback?.Invoke(data);
            if (hideWhenMinimised)
            {
                gameObject.SetActive(false);
                return;
            }
        }
    }
}