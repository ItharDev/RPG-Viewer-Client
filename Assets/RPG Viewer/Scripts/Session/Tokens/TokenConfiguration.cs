using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Networking;
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
        [SerializeField] private GameObject artButton;
        [SerializeField] private TMP_InputField widthInput;
        [SerializeField] private TMP_InputField heightInput;
        [SerializeField] private PermissionPanel permissionPanel;
        [SerializeField] private VisibilityPanel visibilityPanel;

        [Header("Lighting")]
        [SerializeField] private CanvasGroup lightingPanel;
        [SerializeField] private PresetList presetList;
        [SerializeField] private TMP_Text presetInfo;
        [SerializeField] private Vector2 lightingPanelSize;
        [SerializeField] private TMP_InputField visionInput;
        [SerializeField] private TMP_InputField nightInput;
        [SerializeField] private FlexibleColorPicker colorPicker;

        [Space]
        [SerializeField] private LightInput primary;
        [SerializeField] private LightInput secondary;

        private RectTransform rect;
        private byte[] image;
        private byte[] art;
        private TokenData data;
        private PresetData lightData;
        private bool editingPrimary;
        private Action<TokenData, byte[], byte[], PresetData> callback;

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

            // Show / hide panels
            appearancePanel.LeanAlpha(1.0f, 0.2f);
            lightingPanel.LeanAlpha(0.0f, 0.2f);

            LeanTween.size(rect, appearancePanelSize, 0.2f);
            presetList.ClosePanel();
        }
        public void OpenLighting()
        {
            // Enable / disable raycasting
            lightingPanel.blocksRaycasts = true;
            appearancePanel.blocksRaycasts = false;

            // Show / hide panels
            lightingPanel.LeanAlpha(1.0f, 0.2f);
            appearancePanel.LeanAlpha(0.0f, 0.2f);

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

            // Load primary
            primary.effectDropdown.SetValueWithoutNotify(data.primary.effect.type);
            primary.radiusInput.SetTextWithoutNotify(data.primary.radius.ToString());
            ((TMP_Text)primary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            primary.angleInput.SetTextWithoutNotify(data.primary.angle.ToString());
            primary.intensityInput.SetTextWithoutNotify(((int)(data.primary.color.a * 100.0f)).ToString());
            data.primary.color.a = 1.0f;
            primary.colorButton.color = data.primary.color;
            primary.strengthInput.SetTextWithoutNotify(data.primary.effect.strength.ToString());
            primary.frequencyInput.SetTextWithoutNotify(data.primary.effect.frequency.ToString());

            // Load secondary
            secondary.effectDropdown.SetValueWithoutNotify(data.secondary.effect.type);
            secondary.radiusInput.SetTextWithoutNotify(data.secondary.radius.ToString());
            ((TMP_Text)secondary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            secondary.angleInput.SetTextWithoutNotify(data.secondary.angle.ToString());
            secondary.intensityInput.SetTextWithoutNotify(((int)(data.secondary.color.a * 100.0f)).ToString());
            data.secondary.color.a = 1.0f;
            secondary.colorButton.color = data.secondary.color;
            secondary.strengthInput.SetTextWithoutNotify(data.secondary.effect.strength.ToString());
            secondary.frequencyInput.SetTextWithoutNotify(data.secondary.effect.frequency.ToString());

            presetInfo.text = string.IsNullOrEmpty(lightData.name) ? "No preset" : lightData.name;
        }

        public async void ChooseImage(bool art)
        {
            await ImageTask((bytes) =>
            {
                if (bytes == null) return;
                if (art) this.art = bytes;
                else image = bytes;
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

        public void ShowImage(bool art)
        {
            SocketManager.EmitAsync("show-image", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Token image sent to others");
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, art ? data.art : data.image, GameData.User.id);
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
        public void OpenColor(bool isPrimary)
        {
            editingPrimary = isPrimary;
            colorPicker.SetColor(isPrimary ? lightData.primary.color : lightData.secondary.color);
            colorPicker.gameObject.SetActive(true);
        }
        public void ChangeColor(Color color)
        {
            if (editingPrimary)
            {
                lightData.primary.color = color;
                primary.intensityInput.text = Mathf.RoundToInt(color.a * 100.0f).ToString();
                color.a = 1.0f;
                primary.colorButton.color = color;
            }
            else
            {
                lightData.secondary.color = color;
                secondary.intensityInput.text = Mathf.RoundToInt(color.a * 100.0f).ToString();
                color.a = 1.0f;
                secondary.colorButton.color = color;
            }

            ModifyField();
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
        public void LoadData(TokenData _data, PresetData _lightData, byte[] _image, string _header, Action<TokenData, byte[], byte[], PresetData> _callback)
        {
            data = _data;
            lightData = _lightData;
            image = _image;
            header.text = _header;
            callback = _callback;

            LoadAppearance();
            LoadLighting(_lightData);
            if (data.permissions == null) data.permissions = new List<Permission>();
            if (data.visible == null) data.visible = new List<Visible>();

            permissionPanel.LoadData(data.permissions);
            visibilityPanel.LoadData(data.visible);

            if (string.IsNullOrEmpty(_data.art))
            {
                artButton.SetActive(false);
                return;
            }

            art = null;
        }
        private void LoadAppearance()
        {
            nameInput.text = data.name;
            typeDropdown.value = (int)data.type;
            widthInput.text = data.dimensions.x.ToString();
            ((TMP_Text)widthInput.placeholder).text = Session.Instance.Grid.Unit.name;
            heightInput.text = data.dimensions.y.ToString();
            ((TMP_Text)heightInput.placeholder).text = Session.Instance.Grid.Unit.name;
        }
        public void LoadLighting(PresetData preset)
        {
            lightData = preset;

            // Load vision
            visionInput.text = data.visionRadius.ToString();
            ((TMP_Text)visionInput.placeholder).text = Session.Instance.Grid.Unit.name;
            nightInput.text = data.nightRadius.ToString();
            ((TMP_Text)nightInput.placeholder).text = Session.Instance.Grid.Unit.name;


            // Load primary
            primary.effectDropdown.SetValueWithoutNotify(preset.primary.effect.type);
            primary.radiusInput.SetTextWithoutNotify(preset.primary.radius.ToString());
            ((TMP_Text)primary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            primary.angleInput.SetTextWithoutNotify(preset.primary.angle.ToString());
            ((TMP_Text)primary.angleInput.placeholder).text = Session.Instance.Grid.Unit.name;
            primary.intensityInput.SetTextWithoutNotify(((int)(preset.primary.color.a * 100.0f)).ToString());
            preset.primary.color.a = 1.0f;
            primary.colorButton.color = preset.primary.color;
            primary.strengthInput.SetTextWithoutNotify(preset.primary.effect.strength.ToString());
            primary.frequencyInput.SetTextWithoutNotify(preset.primary.effect.frequency.ToString());

            // Load secondary
            secondary.effectDropdown.SetValueWithoutNotify(preset.secondary.effect.type);
            secondary.radiusInput.SetTextWithoutNotify(preset.secondary.radius.ToString());
            ((TMP_Text)secondary.radiusInput.placeholder).text = Session.Instance.Grid.Unit.name;
            secondary.angleInput.SetTextWithoutNotify(preset.secondary.angle.ToString());
            ((TMP_Text)secondary.angleInput.placeholder).text = Session.Instance.Grid.Unit.name;
            secondary.intensityInput.SetTextWithoutNotify(((int)(preset.secondary.color.a * 100.0f)).ToString());
            preset.secondary.color.a = 1.0f;
            secondary.colorButton.color = preset.secondary.color;
            secondary.strengthInput.SetTextWithoutNotify(preset.secondary.effect.strength.ToString());
            secondary.frequencyInput.SetTextWithoutNotify(preset.secondary.effect.frequency.ToString());

            presetInfo.text = string.IsNullOrEmpty(lightData.name) ? "No preset" : lightData.name;
        }

        private void SaveData()
        {
            // Appearance
            data.name = nameInput.text;
            data.type = (TokenType)typeDropdown.value;

            float width;
            float height;
            float.TryParse(widthInput.text, out width);
            float.TryParse(heightInput.text, out height);
            data.dimensions = new Vector2(width <= 0 ? 1 : width, height <= 0 ? 1 : height);

            // Vision
            float.TryParse(visionInput.text, out data.visionRadius);
            float.TryParse(nightInput.text, out data.nightRadius);

            // Primary
            float.TryParse(primary.radiusInput.text, out lightData.primary.radius);
            float.TryParse(primary.angleInput.text, out lightData.primary.angle);
            lightData.primary.color = new Color(primary.colorButton.color.r, primary.colorButton.color.g, primary.colorButton.color.b, lightData.primary.color.a);
            lightData.primary.effect.type = primary.effectDropdown.value;
            float.TryParse(primary.strengthInput.text, out lightData.primary.effect.strength);
            float.TryParse(primary.frequencyInput.text, out lightData.primary.effect.frequency);

            // Secondary
            float.TryParse(secondary.radiusInput.text, out lightData.secondary.radius);
            float.TryParse(secondary.angleInput.text, out lightData.secondary.angle);
            lightData.secondary.color = new Color(secondary.colorButton.color.r, secondary.colorButton.color.g, secondary.colorButton.color.b, lightData.secondary.color.a);
            lightData.secondary.effect.type = secondary.effectDropdown.value;
            float.TryParse(secondary.strengthInput.text, out lightData.secondary.effect.strength);
            float.TryParse(secondary.frequencyInput.text, out lightData.secondary.effect.frequency);

            if (!string.IsNullOrEmpty(lightData.id))
            {
                if (lightData != PresetManager.Instance.GetPreset(lightData.id))
                {
                    lightData.id = data.id;
                    lightData.name = "";
                    data.light = data.id;
                }
            }

            // Visibility & permissions
            data.visible = visibilityPanel.GetData();
            data.permissions = permissionPanel.GetData();

            // Send callback
            callback?.Invoke(data, image, art, lightData);
        }

        public void ModifyField()
        {
            presetInfo.text = "No preset";
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