using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class LightConfig : MonoBehaviour
    {
        [SerializeField] private TMP_InputField radius;
        [SerializeField] private TMP_InputField intensity;
        [SerializeField] private TMP_InputField intensityInput;
        [SerializeField] private TMP_InputField flickerFrequencyInput;
        [SerializeField] private TMP_InputField flickerAmountInput;
        [SerializeField] private TMP_InputField pulseIntervalInput;
        [SerializeField] private TMP_InputField pulseAmountInput;
        [SerializeField] private TMP_InputField presetInput;
        [SerializeField] private TMP_Dropdown effect;
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image lightColor;
        [SerializeField] private FlexibleColorPicker colorPicker;

        private LightData lightData;
        private LightHolder holder;
        private LightManager manager;
        private LightPreset selectedPreset;

        public void LoadData(LightData data, LightHolder lightHolder, LightManager lightManager)
        {
            effect.ClearOptions();
            effect.AddOptions(new List<string>() { "No light", "No effect", "Pulsing effect", "Flickering effect" });
            lightData = data;
            holder = lightHolder;
            manager = lightManager;

            var presets = LightingPresets.Presets;
            for (int i = 0; i < presets.Values.ToArray().Length; i++)
            {
                var name = presets.Values.ToArray()[i].name;
                effect.AddOptions(new List<string>() { name });
            }

            radius.text = data.radius.ToString();
            intensity.text = Mathf.RoundToInt(data.intensity * 100).ToString();
            flickerFrequencyInput.text = data.flickerFrequency.ToString();
            flickerAmountInput.text = (Mathf.RoundToInt(data.flickerAmount * 100.0f)).ToString();
            pulseIntervalInput.text = data.pulseInterval.ToString();
            pulseAmountInput.text = (Mathf.RoundToInt(data.pulseAmount * 100.0f)).ToString();
            effect.value = data.effect;
            toggle.isOn = data.enabled;
            lightColor.color = data.color;
            colorPicker.color = lightColor.color;
        }

        private void LoadPreset(string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            var presets = LightingPresets.Presets;
            var preset = presets[id];

            flickerAmountInput.transform.parent.gameObject.SetActive(false);
            flickerFrequencyInput.transform.parent.gameObject.SetActive(false);
            pulseIntervalInput.transform.parent.gameObject.SetActive(false);
            pulseAmountInput.transform.parent.gameObject.SetActive(false);
            intensityInput.transform.parent.gameObject.SetActive(false);
            radius.transform.parent.gameObject.SetActive(false);
            presetInput.transform.parent.gameObject.SetActive(false);

            switch (preset.effect)
            {
                case LightEffect.No_source:
                    break;
                case LightEffect.No_effect:
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radius.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.No_effect;
                    break;
                case LightEffect.Pulsing:
                    pulseIntervalInput.transform.parent.gameObject.SetActive(true);
                    pulseAmountInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radius.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.Pulsing;
                    break;
                case LightEffect.Flickering:
                    flickerAmountInput.transform.parent.gameObject.SetActive(true);
                    flickerFrequencyInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radius.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.Flickering;
                    break;
            }

            lightColor.color = new Color(preset.color.r, preset.color.g, preset.color.b, 1.0f);
            radius.text = preset.radius.ToString();
            intensityInput.text = (Mathf.RoundToInt(preset.intensity * 100.0f)).ToString();
            flickerFrequencyInput.text = preset.flickerFrequency.ToString();
            flickerAmountInput.text = (Mathf.RoundToInt(preset.flickerAmount * 100.0f)).ToString();
            pulseIntervalInput.text = preset.pulseInterval.ToString();
            pulseAmountInput.text = (Mathf.RoundToInt(preset.pulseAmount * 100.0f)).ToString();
            presetInput.text = preset.name;
        }

        public async void CreatePreset()
        {
            if (string.IsNullOrEmpty(presetInput.text)) return;

            var preset = new LightPreset()
            {
                name = presetInput.text,
                radius = float.Parse(radius.text),
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

        public void SaveData()
        {
            if (colorPicker.gameObject.activeInHierarchy) CloseColor();
            var data = new LightData()
            {
                id = lightData.id,
                radius = float.Parse(radius.text),
                enabled = toggle.isOn,
                position = holder.transform.position,
                intensity = int.Parse(intensity.text) * 0.01f,
                flickerFrequency = int.Parse(flickerFrequencyInput.text),
                flickerAmount = int.Parse(flickerAmountInput.text) * 0.01f,
                pulseInterval = int.Parse(pulseIntervalInput.text),
                pulseAmount = int.Parse(pulseAmountInput.text) * 0.01f,
                effect = effect.value,
                color = lightColor.color,
                preset = string.IsNullOrEmpty(selectedPreset.id) ? "" : selectedPreset.id
            };

            holder.Data = data;
            manager.ModifyLight(holder);
            gameObject.SetActive(false);
        }

        public void OpenColor()
        {
            colorPicker.SetColor(lightColor.color);
            colorPicker.gameObject.SetActive(true);
        }
        public void CloseColor()
        {
            lightColor.color = colorPicker.color;
            colorPicker.gameObject.SetActive(false);
        }

        public void ChangeEffect()
        {
            flickerAmountInput.transform.parent.gameObject.SetActive(false);
            flickerFrequencyInput.transform.parent.gameObject.SetActive(false);
            pulseIntervalInput.transform.parent.gameObject.SetActive(false);
            pulseAmountInput.transform.parent.gameObject.SetActive(false);
            intensityInput.transform.parent.gameObject.SetActive(false);
            radius.transform.parent.gameObject.SetActive(false);
            presetInput.transform.parent.gameObject.SetActive(false);
            presetInput.text = "";

            selectedPreset.id = "";
            switch (effect.value)
            {
                case 0:
                    radius.text = "0";
                    intensityInput.text = "0";
                    lightColor.color = Color.HSVToRGB(0, 0, 0);
                    lightColor.color = new Color(lightColor.color.r, lightColor.color.g, lightColor.color.b, 1.0f);
                    colorPicker.SetColor(lightColor.color);
                    selectedPreset.effect = LightEffect.No_source;
                    break;
                case 1:
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radius.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.No_effect;
                    break;
                case 2:
                    pulseIntervalInput.transform.parent.gameObject.SetActive(true);
                    pulseAmountInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radius.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.Pulsing;
                    break;
                case 3:
                    flickerAmountInput.transform.parent.gameObject.SetActive(true);
                    flickerFrequencyInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radius.transform.parent.gameObject.SetActive(true);
                    presetInput.transform.parent.gameObject.SetActive(true);
                    selectedPreset.effect = LightEffect.Flickering;
                    break;
                default:
                    selectedPreset.id = LightingPresets.Presets.Values.ToArray()[effect.value - 4].id;
                    LoadPreset(selectedPreset.id);
                    break;
            }
        }
    }
}