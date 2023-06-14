using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class GridConfiguration : MonoBehaviour
    {
        [SerializeField] private TMP_InputField widthInput;
        [SerializeField] private TMP_InputField heightInput;
        [SerializeField] private Image colorButton;
        [SerializeField] private Toggle snapToggle;
        [SerializeField] private FlexibleColorPicker colorPicker;

        private RectTransform rect;
        private GridData data;
        private Action<GridData> callback;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnGridSizeChanged.AddListener(ChangeSize);
        }
        private void OnDisable()
        {
            // Add event listeners
            Events.OnGridSizeChanged.RemoveListener(ChangeSize);
        }

        private void ChangeSize(float size)
        {
            data.cellSize = size;
        }

        public void OpenPanel(GridData _data)
        {
            gameObject.SetActive(true);
            LeanTween.size(rect, new Vector2(250.0f, 122.0f), 0.2f);
            LoadData(_data);
        }
        public void ClosePanel(bool saveData)
        {
            LeanTween.size(rect, new Vector2(250.0f, 122.0f), 0.2f).setOnComplete(() =>
            {
                if (saveData) SaveData();
                gameObject.SetActive(false);
            });
        }
        public void OpenColor()
        {
            colorPicker.gameObject.SetActive(true);
            colorPicker.SetColor(data.color);
        }
        public void LoadData(GridData _data)
        {
            data = _data;
            widthInput.text = _data.dimensions.x.ToString();
            heightInput.text = _data.dimensions.y.ToString();
            colorButton.color = _data.color;
            snapToggle.isOn = _data.snapToGrid;
        }
        public void ChangeColor(Color color)
        {
            Events.OnGridColorChanged?.Invoke(color);
            color.a = 1.0f;
            colorButton.color = color;
        }

        private void SaveData()
        {
            int x = int.Parse(widthInput.text);
            int y = int.Parse(heightInput.text);

            data.dimensions = new Vector2Int(x, y);
        }
    }
}