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
            if ((Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) && Selected)
            {
                manager.SelectLight(null);
                manager.RemoveLight(this);
            }
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
            lightHandler.Init(data.effect, data.color, data.intensity, data.flickerFrequency, data.flickerAmount, data.pulseInterval, data.pulseAmount);
            Data = data;
            manager = lightManager;

            transform.localPosition = new Vector3(data.position.x, data.position.y, -1);
            lightSource.size = (data.radius / 5.0f) * SessionManager.session.Settings.grid.cellSize;
            lightSource.enabled = data.enabled;

            if (SessionManager.IsMaster) ShowLight(toolActive);
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

        public void Select(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button != PointerEventData.InputButton.Left || pointerData.clickCount < 2) return;
            manager.SelectLight(this);

            Config.gameObject.SetActive(true);
            Config.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            Config.LoadData(Data, this, manager);

            Selected = true;
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
        public LightEffect effect;
        public Color color;
    }
}