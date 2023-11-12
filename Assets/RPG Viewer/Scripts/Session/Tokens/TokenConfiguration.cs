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
        [SerializeField] private TMP_Text header;

        [Header("Appearance")]
        [SerializeField] private CanvasGroup appearancePanel;
        [SerializeField] private Vector2 appearancePanelSize;
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Dropdown typeDropdown;
        [SerializeField] private TMP_InputField widthInput;
        [SerializeField] private TMP_InputField heightInput;
        [SerializeField] private PermissionPanel permissionPanel;
        [SerializeField] private VisibilityPanel visibilityPanel;

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
        [SerializeField] private FlexibleColorPicker colorPicker;

        private RectTransform rect;
        private byte[] image;
        private TokenData data;
        private PresetData lightData;
        private Action<TokenData, byte[], PresetData> callback;

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
            if (presetList.gameObject.activeInHierarchy) return;
            presetList.LoadData(ApplyPreset);
        }

        private void LoadPreset(string id, PresetData data)
        {
            if (lightData.id == id) ApplyPreset(data);
        }
        private void RemovePreset(string id, PresetData data)
        {
            if (lightData.id == id) lightData.id = data.id;
            ApplyPreset(data);
        }
        private void ApplyPreset(PresetData data)
        {
            lightData = data;
            this.data.light = data.id;
            effectDropdown.value = data.effect.type;
            radiusInput.text = data.radius.ToString();
            intensityInput.text = ((int)(data.color.a * 100.0f)).ToString();
            colorButton.color = data.color;
            strengthInput.text = data.effect.strength.ToString();
            frequencyInput.text = data.effect.frequency.ToString();
        }

        public async void ChooseImage()
        {
            await ImageTask((bytes) =>
            {
                if (bytes != null) image = bytes;
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

        public void ClosePanel(bool saveData = true)
        {
            LeanTween.size(rect, new Vector2(appearancePanelSize.x, 0.0f), 0.1f).setOnComplete(() =>
            {
                if (saveData) SaveData();
                Destroy(gameObject);
            });
        }
        public void OpenPermissions()
        {
            if (permissionPanel.gameObject.activeInHierarchy) return;

            permissionPanel.LoadData(data.permissions, (permissions) =>
            {
                data.permissions = permissions;
            });
        }
        public void OpenVisibility()
        {
            if (visibilityPanel.gameObject.activeInHierarchy) return;

            visibilityPanel.LoadData(data.visible, (visiblity) =>
            {
                data.visible = visiblity;
            });
        }
        public void OpenColor()
        {
            colorPicker.gameObject.SetActive(true);
            colorPicker.SetColor(lightData.color);
        }
        public void ChangeColor(Color color)
        {
            lightData.color.a = color.a;
            intensityInput.text = ((int)(color.a * 100.0f)).ToString();
            color.a = 1.0f;
            colorButton.color = color;
        }
        public void ChangeIntensity()
        {
            lightData.color.a = float.Parse(intensityInput.text) * 0.01f;
            colorPicker.SetColor(lightData.color);
        }
        public void LoadData(TokenData _data, PresetData _lightData, byte[] _image, string _header, Action<TokenData, byte[], PresetData> _callback)
        {
            data = _data;
            lightData = _lightData;
            image = _image;
            header.text = _header;
            callback = _callback;

            LoadAppearance();
            LoadLighting(_lightData);
        }
        private void LoadAppearance()
        {
            nameInput.text = data.name;
            typeDropdown.value = (int)data.type;
            widthInput.text = data.dimensions.x.ToString();
            heightInput.text = data.dimensions.y.ToString();
        }
        public void LoadLighting(PresetData preset)
        {
            lightData = preset;
            visionInput.text = data.visionRadius.ToString();
            nightInput.text = data.nightRadius.ToString();
            effectDropdown.value = preset.effect.type;
            radiusInput.text = preset.radius.ToString();
            intensityInput.text = ((int)(preset.color.a * 100.0f)).ToString();
            preset.color.a = 1.0f;
            colorButton.color = preset.color;
            strengthInput.text = preset.effect.strength.ToString();
            frequencyInput.text = preset.effect.frequency.ToString();
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
            lightData.color = new Color(colorButton.color.r, colorButton.color.g, colorButton.color.b, lightData.color.a);
            lightData.effect.type = effectDropdown.value;
            float.TryParse(strengthInput.text, out lightData.effect.strength);
            float.TryParse(frequencyInput.text, out lightData.effect.frequency);

            if (!string.IsNullOrEmpty(lightData.id))
            {
                if (lightData != PresetManager.Instance.GetPreset(lightData.id))
                {
                    lightData.id = data.id;
                    data.light = data.id;
                }
            }

            // Visibility & permissions
            data.visible = visibilityPanel.GetData();
            data.permissions = permissionPanel.GetData();

            // Send callback
            callback?.Invoke(data, image, lightData);
        }
    }
}