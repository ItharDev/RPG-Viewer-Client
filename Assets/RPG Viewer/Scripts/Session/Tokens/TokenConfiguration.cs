using System;
using System.IO;
using System.Threading.Tasks;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class TokenConfiguration : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private CanvasGroup appearancePanel;
        [SerializeField] private Vector2 appearancePanelSize;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Dropdown typeDropdown;
        [SerializeField] private TMP_InputField widthInput;
        [SerializeField] private TMP_InputField heightInput;

        [Header("Lighting")]
        [SerializeField] private CanvasGroup lightingPanel;
        [SerializeField] private CanvasGroup presetsButton;
        [SerializeField] private PresetList presetList;
        [SerializeField] private Vector2 lightingPanelSize;
        [SerializeField] private TMP_InputField visionInput;
        [SerializeField] private TMP_InputField nightInput;
        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private TMP_InputField radiusInput;
        [SerializeField] private TMP_InputField intensityInput;
        [SerializeField] private Image colorButton;
        [SerializeField] private TMP_InputField strengthInput;
        [SerializeField] private TMP_InputField frequencyInput;

        private RectTransform rect;
        private byte[] image;
        private TokenData data;
        private LightData lightData;
        private Action<TokenData, byte[], LightData> callback;

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
            // Add event listeners
            Events.OnPresetModified.RemoveListener(LoadPreset);
            Events.OnPresetRemoved.RemoveListener(RemovePreset);
        }

        public void OpenAppearance()
        {
            // Enable / disable raycasting
            appearancePanel.blocksRaycasts = true;
            lightingPanel.blocksRaycasts = false;
            presetsButton.blocksRaycasts = false;

            // Show / hide panels
            appearancePanel.LeanAlpha(1.0f, 0.2f);
            lightingPanel.LeanAlpha(0.0f, 0.2f);
            presetsButton.LeanAlpha(0.0f, 0.2f);

            LeanTween.size(rect, appearancePanelSize, 0.2f);
            presetList.ClosePanel();
        }
        public void OpenLighting()
        {
            // Enable / disable raycasting
            lightingPanel.blocksRaycasts = true;
            appearancePanel.blocksRaycasts = false;
            presetsButton.blocksRaycasts = true;

            // Show / hide panels
            lightingPanel.LeanAlpha(1.0f, 0.2f);
            appearancePanel.LeanAlpha(0.0f, 0.2f);
            presetsButton.LeanAlpha(1.0f, 0.2f);

            LeanTween.size(rect, lightingPanelSize, 0.2f);
        }
        public void OpenPresets()
        {
            presetList.gameObject.SetActive(true);
            presetList.LoadData(ApplyPreset);
            LeanTween.size((RectTransform)presetList.transform, new Vector2(120.0f, lightingPanelSize.y), 0.2f);
        }

        private void LoadPreset(string id, LightData data)
        {
            if (lightData.id == id) ApplyPreset(data);
        }
        private void RemovePreset(string id)
        {
            if (lightData.id == id) lightData.id = data.id;
        }
        private void ApplyPreset(LightData data)
        {
            lightData = data;
            effectDropdown.value = data.effect.type;
            radiusInput.text = data.radius.ToString();
            intensityInput.text = data.intensity.ToString();
            colorButton.color = data.color;
            strengthInput.text = data.effect.strength.ToString();
            frequencyInput.text = data.effect.frequency.ToString();
        }

        public async void ChooseImage() => await ImageTask();
        private async Task ImageTask()
        {
            // Only allow image files
            ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };

            // Open file explorer
            StandaloneFileBrowser.OpenFilePanelAsync("Select file", "", extensions, false, (string[] paths) =>
            {
                // Return if no items are selected
                if (paths.Length == 0) return;

                // Read bytes from selected file
                image = File.ReadAllBytes(paths[0]);
            });
            await Task.Yield();
        }

        public void ClosePanel()
        {
            LeanTween.size(rect, new Vector2(appearancePanelSize.x, 0.0f), 0.1f).setOnComplete(SaveData);
        }
        public void LoadData(TokenData _data, byte[] _image, Action<TokenData, byte[], LightData> _callback)
        {
            data = _data;
            image = _image;
            callback = _callback;
        }
        private void SaveData()
        {
            // Appearance
            data.name = nameInput.text;
            data.type = (TokenType)typeDropdown.value;

            int width = 5;
            int height = 5;
            int.TryParse(widthInput.text, out width);
            int.TryParse(heightInput.text, out height);
            data.dimensions = new Vector2Int(width, height);

            // Vision
            float.TryParse(visionInput.text, out data.visionRadius);
            float.TryParse(nightInput.text, out data.nightRadius);

            // Lighting
            float.TryParse(radiusInput.text, out lightData.radius);
            float.TryParse(intensityInput.text, out lightData.intensity);
            lightData.color = colorButton.color;
            lightData.effect.type = effectDropdown.value;
            float.TryParse(strengthInput.text, out lightData.effect.strength);
            float.TryParse(frequencyInput.text, out lightData.effect.frequency);

            if (!string.IsNullOrEmpty(data.id))
            {
                if (lightData != PresetManager.Instance.GetPreset(lightData.id)) lightData.id = data.id;
            }

            // Send callback
            callback?.Invoke(data, image, lightData);
            Destroy(gameObject);
        }
    }
}