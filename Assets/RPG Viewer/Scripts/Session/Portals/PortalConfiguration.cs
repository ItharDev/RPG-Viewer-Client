using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class PortalConfiguration : MonoBehaviour
    {
        [SerializeField] private Toggle activeToggle;
        [SerializeField] private Toggle visibleToggle;
        [SerializeField] private TMP_Dropdown modeDropdown;
        [SerializeField] private TMP_InputField radiusInput;
        [SerializeField] private RectTransform rect;

        private Action<PortalData> callback;

        public void OpenPanel(PortalData _data, Action<PortalData> _callback)
        {
            callback = _callback;
            gameObject.SetActive(true);
            LeanTween.size(rect, new Vector2(250.0f, 117.0f), 0.2f);
            LoadData(_data);
        }

        public void ClosePanel(bool saveData)
        {
            LeanTween.size(rect, new Vector2(250.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                if (saveData) SaveData();
                Destroy(gameObject);
            });
        }

        private void LoadData(PortalData data)
        {
            activeToggle.isOn = data.active;
            visibleToggle.isOn = data.visible;
            modeDropdown.value = data.continuous ? 1 : 0;
            radiusInput.text = data.radius.ToString();
        }

        private void SaveData()
        {
            if (!float.TryParse(radiusInput.text, out float radius))
            {
                radius = Session.Instance.Grid.Unit.scale;
            }

            radius = Mathf.Clamp(radius, 0.0f, float.PositiveInfinity);

            PortalData data = new PortalData
            {
                active = activeToggle.isOn,
                visible = visibleToggle.isOn,
                continuous = modeDropdown.value == 1,
                radius = radius
            };

            callback?.Invoke(data);
            callback = null;
        }
    }
}