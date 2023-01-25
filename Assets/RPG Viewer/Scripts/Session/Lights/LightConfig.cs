using UnityEngine;
using TMPro;
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
        [SerializeField] private TMP_Dropdown effect;
        [SerializeField] private Toggle toggle;
        [SerializeField] private Image lightColor;
        [SerializeField] private FlexibleColorPicker colorPicker;

        private LightData lightData;
        private LightHolder holder;
        private LightManager manager;

        public void LoadData(LightData data, LightHolder lightHolder, LightManager lightManager)
        {
            lightData = data;
            holder = lightHolder;
            manager = lightManager;

            radius.text = data.radius.ToString();
            intensity.text = Mathf.RoundToInt(data.intensity * 100).ToString();
            flickerFrequencyInput.text = data.flickerFrequency.ToString();
            flickerAmountInput.text = (Mathf.RoundToInt(data.flickerAmount * 100.0f)).ToString();
            pulseIntervalInput.text = data.pulseInterval.ToString();
            pulseAmountInput.text = (Mathf.RoundToInt(data.pulseAmount * 100.0f)).ToString();
            effect.value = (int)data.effect;
            toggle.isOn = data.enabled;
            lightColor.color = data.color;
            colorPicker.color = lightColor.color;
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
                effect = (LightEffect)effect.value,
                color = lightColor.color
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

            switch (effect.value)
            {
                case 0:
                    radius.text = "0";
                    intensityInput.text = "0";
                    flickerFrequencyInput.text = "0";
                    flickerAmountInput.text = "0";
                    lightColor.color = Color.HSVToRGB(0, 0, 0);
                    lightColor.color = new Color(lightColor.color.r, lightColor.color.g, lightColor.color.b, 1.0f);
                    colorPicker.SetColor(lightColor.color);
                    break;
                case 1:
                    radius.text = "30";
                    intensityInput.text = "100";
                    flickerFrequencyInput.text = "15";
                    flickerAmountInput.text = "10";
                    lightColor.color = Color.HSVToRGB(50.0f / 360.0f, 0.5f, 1.0f);
                    lightColor.color = new Color(lightColor.color.r, lightColor.color.g, lightColor.color.b, 1.0f);
                    colorPicker.SetColor(lightColor.color);
                    break;
                case 2:
                    radius.text = "15";
                    intensityInput.text = "100";
                    lightColor.color = Color.HSVToRGB(200.0f / 360.0f, 0.3f, 1.0f);
                    lightColor.color = new Color(lightColor.color.r, lightColor.color.g, lightColor.color.b, 1.0f);
                    pulseIntervalInput.text = "2";
                    pulseAmountInput.text = "60";
                    colorPicker.SetColor(lightColor.color);
                    break;
                case 3:
                    radius.text = "40";
                    intensityInput.text = "100";
                    flickerFrequencyInput.text = "10";
                    flickerAmountInput.text = "5";
                    lightColor.color = Color.HSVToRGB(50.0f / 360.0f, 0.2f, 1.0f);
                    lightColor.color = new Color(lightColor.color.r, lightColor.color.g, lightColor.color.b, 1.0f);
                    colorPicker.SetColor(lightColor.color);
                    break;
                case 4:
                    radius.text = "10";
                    intensityInput.text = "50";
                    flickerFrequencyInput.text = "10";
                    flickerAmountInput.text = "5";
                    lightColor.color = Color.HSVToRGB(50.0f / 360.0f, 0.3f, 1.0f);
                    lightColor.color = new Color(lightColor.color.r, lightColor.color.g, lightColor.color.b, 1.0f);
                    colorPicker.SetColor(lightColor.color);
                    break;
                case 5:
                    pulseIntervalInput.transform.parent.gameObject.SetActive(true);
                    pulseAmountInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    colorPicker.SetColor(lightColor.color);
                    radius.transform.parent.gameObject.SetActive(true);
                    break;
                case 6:
                    flickerAmountInput.transform.parent.gameObject.SetActive(true);
                    flickerFrequencyInput.transform.parent.gameObject.SetActive(true);
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radius.transform.parent.gameObject.SetActive(true);
                    break;
                case 7:
                    intensityInput.transform.parent.gameObject.SetActive(true);
                    radius.transform.parent.gameObject.SetActive(true);
                    break;
            }
        }
    }
}