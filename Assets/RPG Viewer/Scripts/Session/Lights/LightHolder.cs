using System;
using FunkyCode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class LightHolder : MonoBehaviour
    {
        [SerializeField] private Color activeColor;
        [SerializeField] private Color normalColor;
        [SerializeField] private Image icon;

        [SerializeField] private Light2D lightSource;
        [SerializeField] private LightHandler lightHandler;
        [SerializeField] private Canvas canvas;

        [SerializeField] private LightConfig configPrefab;

        public LightData Data;
        public bool Selected;

        private LightManager manager;
        public LightConfig Config;

        private bool draggingCenter;
        private bool canDrag;

        private void Start()
        {
            Config = Instantiate(configPrefab, GameObject.Find("Main Canvas").transform);
            Config.gameObject.SetActive(false);

            Deselect();
            if (!SessionManager.IsMaster)
            {
                canvas.gameObject.SetActive(false);
            }
        }
        private void Update()
        {
            canDrag = RectTransformUtility.RectangleContainsScreenPoint(GetComponentInChildren<Canvas>(true).GetComponent<RectTransform>(), Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (Input.GetMouseButtonUp(0) && CanDeselect() && Selected)
            {
                manager.ModifyLight(this);
                manager.SelectLight(null);
            }
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C) && Selected)
            {
                manager.CopyLight(this.Data);
            }
        }

        private bool CanDeselect()
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(canvas.GetComponent<RectTransform>(), Camera.main.ScreenToWorldPoint(Input.mousePosition))) return false;
            if (Config.gameObject.activeInHierarchy) return false;

            return true;
        }

        public void LoadData(LightData data, LightManager lightManager, bool toolActive)
        {
            if (!string.IsNullOrEmpty(data.preset))
            {
                if (!LightingPresets.Presets.ContainsKey(data.preset)) return;
                LightingPresets.MoveActor(this, Data.preset);
                var preset = LightingPresets.Presets[data.preset];

                lightHandler.Init((LightEffect)preset.effect, preset.color, preset.intensity, preset.flickerFrequency, preset.flickerAmount, preset.pulseInterval, preset.pulseAmount);
            }
            else lightHandler.Init((LightEffect)data.effect, data.color, data.intensity, data.flickerFrequency, data.flickerAmount, data.pulseInterval, data.pulseAmount);
            Data = data;
            manager = lightManager;

            transform.localPosition = new Vector3(data.position.x, data.position.y, -1);
            lightSource.size = (data.radius / 5.0f) * SessionManager.Session.Settings.grid.cellSize;
            lightSource.enabled = data.enabled;

            if (SessionManager.IsMaster) ShowLight(toolActive);

            icon.color = Data.enabled ? activeColor : normalColor;
        }
        public void UpdatePreset(LightPreset preset)
        {
            Data = new LightData()
            {
                id = Data.id,
                radius = preset.radius,
                enabled = Data.enabled,
                position = Data.position,
                intensity = preset.intensity,
                flickerFrequency = preset.flickerFrequency,
                flickerAmount = preset.flickerAmount,
                pulseInterval = preset.pulseInterval,
                pulseAmount = preset.pulseAmount,
                effect = (int)preset.effect,
                color = preset.color,
                preset = Data.preset
            };

            lightHandler.Init((LightEffect)Data.effect, Data.color, Data.intensity, Data.flickerFrequency, Data.flickerAmount, Data.pulseInterval, Data.pulseAmount);
        }
        public void ShowLight(bool enabled)
        {
            canvas.enabled = enabled;
        }
        public void Deselect()
        {
            Selected = false;
            if (configPrefab.gameObject.activeInHierarchy) configPrefab.SaveData();
            configPrefab.gameObject.SetActive(false);
        }

        public void SelectLight(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;

            if (pointerData.button != PointerEventData.InputButton.Left) return;

            if (pointerData.clickCount >= 2 && manager.StateManager.LightState == LightState.Create)
            {
                manager.SelectLight(this);

                Config.gameObject.SetActive(true);
                Config.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                Config.LoadData(Data, this, manager);

                Selected = true;
            }
            else if (manager.StateManager.LightState == LightState.Delete)
            {
                Config.gameObject.SetActive(false);

                manager.SelectLight(null);
                manager.RemoveLight(this);
            }
        }

        public void BeginDrag(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;
            draggingCenter = true;
        }
        public void Drag(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (!draggingCenter) return;


            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(transform.position.x, transform.position.y, -1);
            Data.position = transform.position;
        }
        public void EndDrag(BaseEventData eventData)
        {
            draggingCenter = false;
            Data.position = transform.position;
            manager.ModifyLight(this);

        }
    }

    [Serializable]
    public struct LightData
    {
        public string id;
        public float radius;
        public bool enabled;
        public Vector2 position;
        public float intensity;
        public float flickerFrequency;
        public float flickerAmount;
        public float pulseInterval;
        public float pulseAmount;
        public int effect;
        public Color color;
        public string preset;
    }
}