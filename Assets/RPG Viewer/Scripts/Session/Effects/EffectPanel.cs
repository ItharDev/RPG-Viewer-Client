using System;
using System.IO;
using System.Threading.Tasks;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class EffectPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private bool hideWhenMinimised;
        [SerializeField] private FlexibleColorPicker colorPicker;

        [Space]
        public TMP_InputField radiusInput;
        public Toggle overTokensToggle;
        public Image colorButton;
        public TMP_InputField alphaInput;

        public TMP_Dropdown animationDropdown;
        public TMP_InputField pulseFrequency;
        public TMP_InputField pulseStrength;
        public TMP_InputField rotationSpeed;

        public string Id { get { return data.id; } }

        private byte[] image;
        private EffectData data;
        private Action<EffectData, byte[]> callback;

        public void LoadData(EffectData _data, byte[] _image, Action<EffectData, byte[]> onSelected)
        {
            LeanTween.size((RectTransform)transform, new Vector2(450.0f, 297.0f), 0.2f);

            data = _data;
            image = _image;
            callback = onSelected;
            _data.color.a = 1.0f;

            nameInput.text = data.name;
            radiusInput.text = data.radius.ToString();
            overTokensToggle.isOn = data.overTokens;
            ((TMP_Text)radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            alphaInput.text = Mathf.RoundToInt(data.color.a * 100.0f).ToString();
            colorButton.color = _data.color;
            animationDropdown.value = (int)data.animation.type;
            pulseFrequency.text = data.animation.pulseFrequency.ToString();
            pulseStrength.text = data.animation.pulseStrength.ToString();
            rotationSpeed.text = data.animation.rotationSpeed.ToString();
        }
        public void OpenColor()
        {
            colorPicker.SetColor(data.color);
            colorPicker.gameObject.SetActive(true);
        }
        public void ChangeColor(Color color)
        {
            data.color = color;
            alphaInput.text = Mathf.RoundToInt(color.a * 100.0f).ToString();
            color.a = 1.0f;
            colorButton.color = color;
        }
        public void ChangeAlpha()
        {
            data.color.a = float.Parse(alphaInput.text) * 0.01f;
        }

        public void ClosePanel(bool saveData = true)
        {
            LeanTween.size((RectTransform)transform, new Vector2(0.0f, 297.0f), 0.2f).setOnComplete(() =>
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
            // Parse data from inputs
            data.name = string.IsNullOrEmpty(nameInput.text) ? data.name : nameInput.text;
            float.TryParse(radiusInput.text, out data.radius);
            data.overTokens = overTokensToggle.isOn;
            data.color = new Color(colorButton.color.r, colorButton.color.g, colorButton.color.b, data.color.a);
            data.animation.type = (EffectAnimationType)animationDropdown.value;
            float.TryParse(pulseFrequency.text, out data.animation.pulseFrequency);
            float.TryParse(pulseStrength.text, out data.animation.pulseStrength);
            float.TryParse(rotationSpeed.text, out data.animation.rotationSpeed);

            // Send callback
            callback?.Invoke(data, image);
        }

        public async void ChooseImage()
        {
            await ImageTask((bytes) =>
            {
                if (bytes == null) return;
                image = bytes;
            });
        }
        private async Task ImageTask(Action<byte[]> callback)
        {
            // Only allow image files
            ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg", "webp") };

            // Open file explorer
            StandaloneFileBrowser.OpenFilePanelAsync("Select file", "", extensions, false, (string[] paths) =>
            {
                // Return if no items are selected
                if (paths.Length == 0) callback(null);

                // Read bytes from selected file
                callback(File.ReadAllBytes(paths[0]));
            });
            await Task.Yield();
        }
    }
}