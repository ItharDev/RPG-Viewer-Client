using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Networking;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class TokenConfiguration : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text label;

        [Header("Buttons")]
        [SerializeField] private Color normalColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private TMP_Text apprearanceText;
        [SerializeField] private Image apprearanceIcon;
        [SerializeField] private TMP_Text lightingText;
        [SerializeField] private Image lightingIcon;
        [SerializeField] private TMP_Text conditionText;
        [SerializeField] private Image conditionIcon;

        [Header("Appearance")]
        [SerializeField] private List<GameObject> appearanceFields = new List<GameObject>();
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_Dropdown typeDropdown;
        [SerializeField] private TMP_InputField widthInput, heightInput;

        [Header("Lighting")]
        [SerializeField] private List<GameObject> lightingFields = new List<GameObject>();
        [SerializeField] private Toggle visionToggle;
        [SerializeField] private Toggle nightVisionToggle;
        [SerializeField] private Toggle highlightToggle;
        [SerializeField] private TMP_InputField radiusInput;
        [SerializeField] private TMP_InputField intensityInput;
        [SerializeField] private TMP_InputField flickerFrequencyInput;
        [SerializeField] private TMP_InputField flickerAmountInput;
        [SerializeField] private TMP_InputField pulseIntervalInput;
        [SerializeField] private TMP_InputField pulseAmountInput;
        [SerializeField] private TMP_InputField presetInput;
        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private Image lightColor;
        [SerializeField] private FlexibleColorPicker colorPicker;

        [Header("Conditions")]
        [SerializeField] private List<GameObject> conditionFields = new List<GameObject>();

        public object Reference;

        private byte[] bytes;
        private bool imageChanged;
        private TokenData data;
        private string folder;
        public ConditionFlag ConditionFlags;

        private LightPreset selectedPreset;

        private void Start()
        {
            SelectAppearance();
        }

        #region Data
        public void LoadData(TokenData _data, Token _reference, byte[] _bytes)
        {
            imageChanged = false;
            data = _data;
            Reference = _reference;
            bytes = _bytes;
            label.text = $"Modify token: {data.name}";

            LoadAppearance();
            LoadLighting();
            LoadConditions();
        }
        public void LoadData(TokenData _data, MasterPanel _reference, string _folder)
        {
            imageChanged = false;

            data = _data;
            Reference = _reference;
            folder = _folder;

            label.text = "Create blueprint";

            data.type = TokenType.Character;
            data.dimensions = new Vector2Int(5, 5);

            LoadAppearance();
            LoadLighting();
            LoadConditions();
        }
        public void LoadData(TokenData _data, BlueprintHolder _reference, string _folder, byte[] _bytes)
        {
            imageChanged = false;

            data = _data;
            Reference = _reference;
            folder = _folder;
            bytes = _bytes;

            label.text = $"Modify blueprint: {data.name}";

            LoadAppearance();
            LoadLighting();
            LoadConditions();
        }

        public void SaveData()
        {
            if (colorPicker.gameObject.activeInHierarchy) CloseColor();

            if (Reference.GetType() == typeof(Token))
            {
                if (int.Parse(widthInput.text) <= 0 || int.Parse(heightInput.text) <= 0)
                {
                    MessageManager.QueueMessage("Invalid dimensions");
                    return;
                }

                TokenData newData = new TokenData()
                {
                    id = data.id,
                    name = nameInput.text,
                    type = (TokenType)typeDropdown.value,
                    dimensions = new Vector2Int(int.Parse(widthInput.text), int.Parse(heightInput.text)),
                    hasVision = visionToggle.isOn,
                    nightVision = nightVisionToggle.isOn,
                    highlighted = highlightToggle.isOn,
                    lightRadius = int.Parse(radiusInput.text),
                    lightIntensity = int.Parse(intensityInput.text) * 0.01f,
                    lightEffect = effectDropdown.value,
                    lightColor = lightColor.color,
                    flickerFrequency = int.Parse(flickerFrequencyInput.text),
                    flickerAmount = int.Parse(flickerAmountInput.text) * 0.01f,
                    pulseInterval = int.Parse(pulseIntervalInput.text),
                    pulseAmount = int.Parse(pulseAmountInput.text) * 0.01f,
                    image = data.image,
                    conditions = (int)ConditionFlags,
                    preset = selectedPreset.id
                };

                (Reference as Token).ModifyToken(newData, bytes, imageChanged);
            }
            else if (Reference.GetType() == typeof(MasterPanel))
            {
                if (bytes == null)
                {
                    MessageManager.QueueMessage("No image selected");
                    return;
                }
                if (int.Parse(widthInput.text) <= 0 || int.Parse(heightInput.text) <= 0)
                {
                    MessageManager.QueueMessage("Invalid dimensions");
                    return;
                }

                TokenData newData = new TokenData()
                {
                    id = "",
                    name = nameInput.text,
                    type = (TokenType)typeDropdown.value,
                    dimensions = new Vector2Int(int.Parse(widthInput.text), int.Parse(heightInput.text)),
                    hasVision = visionToggle.isOn,
                    nightVision = nightVisionToggle.isOn,
                    highlighted = highlightToggle.isOn,
                    lightRadius = int.Parse(radiusInput.text),
                    lightIntensity = int.Parse(intensityInput.text) * 0.01f,
                    lightEffect = effectDropdown.value,
                    lightColor = lightColor.color,
                    flickerFrequency = int.Parse(flickerFrequencyInput.text),
                    flickerAmount = int.Parse(flickerAmountInput.text) * 0.01f,
                    pulseInterval = int.Parse(pulseIntervalInput.text),
                    pulseAmount = int.Parse(pulseAmountInput.text) * 0.01f,
                    image = "",
                    preset = selectedPreset.id
                };



                FindObjectOfType<MasterPanel>().AddBlueprint(newData, folder, bytes);
            }
            else if (Reference.GetType() == typeof(BlueprintHolder))
            {
                if (int.Parse(widthInput.text) <= 0 || int.Parse(heightInput.text) <= 0)
                {
                    MessageManager.QueueMessage("Invalid dimensions");
                    return;
                }

                TokenData newData = new TokenData()
                {
                    id = data.id,
                    name = nameInput.text,
                    type = (TokenType)typeDropdown.value,
                    dimensions = new Vector2Int(int.Parse(widthInput.text), int.Parse(heightInput.text)),
                    hasVision = visionToggle.isOn,
                    nightVision = nightVisionToggle.isOn,
                    highlighted = highlightToggle.isOn,
                    lightRadius = int.Parse(radiusInput.text),
                    lightIntensity = int.Parse(intensityInput.text) * 0.01f,
                    lightEffect = effectDropdown.value,
                    lightColor = lightColor.color,
                    flickerFrequency = int.Parse(flickerFrequencyInput.text),
                    flickerAmount = int.Parse(flickerAmountInput.text) * 0.01f,
                    pulseInterval = int.Parse(pulseIntervalInput.text),
                    pulseAmount = int.Parse(pulseAmountInput.text) * 0.01f,
                    image = data.image,
                    permissions = data.permissions,
                    preset = selectedPreset.id
                };

                (Reference as BlueprintHolder).ModifyBlueprint(newData, bytes, imageChanged);
            }
        }
        #endregion

        public void OpenColor()
        {
            colorPicker.SetColor(lightColor.color);
            colorPicker.gameObject.SetActive(true);
            colorPicker.SetColor(lightColor.color);
        }
        public void CloseColor()
        {
            lightColor.color = colorPicker.color;
            colorPicker.gameObject.SetActive(false);
        }

        #region Appearance
        public void SelectAppearance()
        {
            lightingText.color = normalColor;
            lightingIcon.color = normalColor;
            for (int i = 0; i < lightingFields.Count; i++)
            {
                lightingFields[i].SetActive(false);
            }

            conditionText.color = normalColor;
            conditionIcon.color = normalColor;
            for (int i = 0; i < conditionFields.Count; i++)
            {
                conditionFields[i].SetActive(false);
            }

            apprearanceText.color = selectedColor;
            apprearanceIcon.color = selectedColor;
            for (int i = 0; i < appearanceFields.Count; i++)
            {
                appearanceFields[i].SetActive(true);
            }
        }

        private void LoadAppearance()
        {
            nameInput.text = data.name;
            typeDropdown.value = (int)data.type;
            widthInput.text = data.dimensions.x.ToString();
            heightInput.text = data.dimensions.y.ToString();
        }

        #endregion

        #region Lighting
        public void SelectLighting()
        {
            lightingText.color = selectedColor;
            lightingIcon.color = selectedColor;
            for (int i = 0; i < lightingFields.Count; i++)
            {
                lightingFields[i].SetActive(true);
            }

            conditionText.color = normalColor;
            conditionIcon.color = normalColor;
            for (int i = 0; i < conditionFields.Count; i++)
            {
                conditionFields[i].SetActive(false);
            }

            apprearanceText.color = normalColor;
            apprearanceIcon.color = normalColor;
            for (int i = 0; i < appearanceFields.Count; i++)
            {
                appearanceFields[i].SetActive(false);
            }

            ChangeEffect();
        }

        private void LoadLighting()
        {
            var presets = LightingPresets.Presets;
            for (int i = 0; i < presets.Values.ToArray().Length; i++)
            {
                var name = presets.Values.ToArray()[i].name;
                effectDropdown.AddOptions(new List<string>() { name });
            }

            visionToggle.isOn = data.hasVision;
            nightVisionToggle.isOn = data.nightVision;
            highlightToggle.isOn = data.highlighted;
            effectDropdown.value = data.lightEffect;
            lightColor.color = new Color(data.lightColor.r, data.lightColor.g, data.lightColor.b, 1.0f);
            radiusInput.text = data.lightRadius.ToString();
            intensityInput.text = (Mathf.RoundToInt(data.lightIntensity * 100.0f)).ToString();
            flickerFrequencyInput.text = data.flickerFrequency.ToString();
            flickerAmountInput.text = (Mathf.RoundToInt(data.flickerAmount * 100.0f)).ToString();
            pulseIntervalInput.text = data.pulseInterval.ToString();
            pulseAmountInput.text = (Mathf.RoundToInt(data.pulseAmount * 100.0f)).ToString();
        }

        private void LoadPreset(string id)
        {
            Debug.Log(id);
            if (string.IsNullOrEmpty(id)) return;

            var presets = LightingPresets.Presets;
            var preset = presets[id];

            flickerAmountInput.transform.parent.gameObject.SetActive(false);
            flickerFrequencyInput.transform.parent.gameObject.SetActive(false);
            pulseIntervalInput.transform.parent.gameObject.SetActive(false);
            pulseAmountInput.transform.parent.gameObject.SetActive(false);
            intensityInput.transform.parent.gameObject.SetActive(false);
            radiusInput.transform.parent.gameObject.SetActive(false);
            presetInput.transform.parent.gameObject.SetActive(false);

            switch (preset.effect)
            {
                case LightEffect.No_source:
                    break;
                case LightEffect.No_effect:
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radiusInput.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.No_effect;
                    break;
                case LightEffect.Pulsing:
                    pulseIntervalInput.transform.parent.gameObject.SetActive(true);
                    pulseAmountInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radiusInput.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.Pulsing;
                    break;
                case LightEffect.Flickering:
                    flickerAmountInput.transform.parent.gameObject.SetActive(true);
                    flickerFrequencyInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radiusInput.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.Flickering;
                    break;
            }

            lightColor.color = new Color(preset.color.r, preset.color.g, preset.color.b, 1.0f);
            radiusInput.text = preset.radius.ToString();
            intensityInput.text = (Mathf.RoundToInt(preset.intensity * 100.0f)).ToString();
            flickerFrequencyInput.text = preset.flickerFrequency.ToString();
            flickerAmountInput.text = (Mathf.RoundToInt(preset.flickerAmount * 100.0f)).ToString();
            pulseIntervalInput.text = preset.pulseInterval.ToString();
            pulseAmountInput.text = (Mathf.RoundToInt(preset.pulseAmount * 100.0f)).ToString();
            presetInput.text = preset.name;
        }

        public void ChangeEffect()
        {
            flickerAmountInput.transform.parent.gameObject.SetActive(false);
            flickerFrequencyInput.transform.parent.gameObject.SetActive(false);
            pulseIntervalInput.transform.parent.gameObject.SetActive(false);
            pulseAmountInput.transform.parent.gameObject.SetActive(false);
            intensityInput.transform.parent.gameObject.SetActive(false);
            radiusInput.transform.parent.gameObject.SetActive(false);
            presetInput.transform.parent.gameObject.SetActive(false);
            presetInput.text = "";

            selectedPreset.id = "";
            switch (effectDropdown.value)
            {
                case 0:
                    radiusInput.text = "0";
                    intensityInput.text = "0";
                    lightColor.color = Color.HSVToRGB(0, 0, 0);
                    lightColor.color = new Color(lightColor.color.r, lightColor.color.g, lightColor.color.b, 1.0f);
                    colorPicker.SetColor(lightColor.color);
                    selectedPreset.effect = LightEffect.No_source;
                    break;
                case 1:
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radiusInput.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.No_effect;
                    break;
                case 2:
                    pulseIntervalInput.transform.parent.gameObject.SetActive(true);
                    pulseAmountInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radiusInput.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.Pulsing;
                    break;
                case 3:
                    flickerAmountInput.transform.parent.gameObject.SetActive(true);
                    flickerFrequencyInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radiusInput.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.Flickering;
                    break;
                default:
                    selectedPreset.id = LightingPresets.Presets.Values.ToArray()[effectDropdown.value - 4].id;
                    LoadPreset(selectedPreset.id);
                    break;
            }
        }

        public async void CreatePreset()
        {
            if (string.IsNullOrEmpty(presetInput.text)) return;

            var preset = new LightPreset()
            {
                name = presetInput.text,
                radius = float.Parse(radiusInput.text),
                color = lightColor.color,
                intensity = float.Parse(intensityInput.text) * 0.01f,
                flickerFrequency = float.Parse(flickerFrequencyInput.text),
                flickerAmount = float.Parse(flickerAmountInput.text) * 0.01f,
                pulseInterval = float.Parse(pulseIntervalInput.text),
                pulseAmount = float.Parse(pulseAmountInput.text) * 0.01f,
                effect = selectedPreset.effect
            };
            if (string.IsNullOrEmpty(selectedPreset.id))
            {
                await SocketManager.Socket.EmitAsync("create-preset", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (callback.GetValue().GetBoolean()) MessageManager.QueueMessage("Preset created");
                    else MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, JsonUtility.ToJson(preset));
            }
            else
            {
                await SocketManager.Socket.EmitAsync("modify-preset", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (callback.GetValue().GetBoolean()) MessageManager.QueueMessage("Preset modified");
                    else MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, selectedPreset.id, JsonUtility.ToJson(preset));
            }
        }

        public async void DeletePreset()
        {
            if (string.IsNullOrEmpty(selectedPreset.id)) return;
            await SocketManager.Socket.EmitAsync("remove-preset", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean()) MessageManager.QueueMessage("Preset deleted");
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, selectedPreset.id);
        }
        #endregion

        #region Conditions
        public void SelectConditions()
        {
            lightingText.color = normalColor;
            lightingIcon.color = normalColor;
            for (int i = 0; i < lightingFields.Count; i++)
            {
                lightingFields[i].SetActive(false);
            }

            conditionText.color = selectedColor;
            conditionIcon.color = selectedColor;
            for (int i = 0; i < conditionFields.Count; i++)
            {
                conditionFields[i].SetActive(true);
            }

            apprearanceText.color = normalColor;
            apprearanceIcon.color = normalColor;
            for (int i = 0; i < appearanceFields.Count; i++)
            {
                appearanceFields[i].SetActive(false);
            }
        }

        private void LoadConditions()
        {
            var conditions = conditionFields[0].GetComponentsInChildren<ConditionHolder>(true);
            ConditionFlags = (ConditionFlag)data.conditions;

            for (int i = 0; i < conditions.Length; i++)
            {
                conditions[i].Config = this;
            }
        }

        public void ToggleCondition(Condition con)
        {
            if (ConditionFlags.HasFlag(con.flag)) ConditionFlags &= ~con.flag;
            else ConditionFlags |= con.flag;
        }
        #endregion

        #region Image
        public async void SelectImage() => await ImageTask();
        private async Task ImageTask()
        {
            var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
            StandaloneFileBrowser.OpenFilePanelAsync("Select file", "", extensions, false, (string[] paths) =>
            {
                if (paths.Length == 0) return;
                bytes = File.ReadAllBytes(paths[0]);
                imageChanged = true;
            });
            await Task.Yield();
        }
        #endregion
    }
}
