using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class GridConfiguration : MonoBehaviour
    {
        [SerializeField] private TMP_InputField widthInput;
        [SerializeField] private TMP_InputField heightInput;
        [SerializeField] private TMP_InputField unitName;
        [SerializeField] private TMP_InputField unitScale;
        [SerializeField] private Image colorButton;
        [SerializeField] private Toggle snapToggle;
        [SerializeField] private FlexibleColorPicker colorPicker;

        private RectTransform rect;
        private GridData data;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            Events.OnGridChanged.AddListener(ReloadData);
        }
        private void OnDestroy()
        {
            // Add event listeners
            Events.OnGridChanged.RemoveListener(ReloadData);
        }

        private void ReloadData(GridData newData, bool reloadRequired, bool globalUpdate)
        {
            data = newData;
        }

        public void OpenPanel()
        {
            if (data.cellSize == 0.0f) data = Session.Instance.Settings.grid;
            gameObject.SetActive(true);
            LeanTween.size(rect, new Vector2(250.0f, 117.0f), 0.2f);
            LoadData(data);
        }
        public void ClosePanel(bool saveData)
        {
            LeanTween.size(rect, new Vector2(250.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                if (saveData) SaveData();
                else Events.OnGridChanged?.Invoke(Session.Instance.Settings.grid, true, false);
                gameObject.SetActive(false);
            });
        }
        public void OpenColor()
        {
            Color initColor = data.color;
            initColor.a = 1.0f;
            colorPicker.SetColor(data.color);
            colorPicker.gameObject.SetActive(true);
            colorButton.color = initColor;
        }
        public void LoadData(GridData _data)
        {
            data = _data;
            widthInput.text = _data.dimensions.x.ToString();
            ((TMP_Text)widthInput.placeholder).text = Session.Instance.Grid.Unit.name;
            heightInput.text = _data.dimensions.y.ToString();
            ((TMP_Text)heightInput.placeholder).text = Session.Instance.Grid.Unit.name;
            unitName.text = _data.unit.name;
            unitScale.text = _data.unit.scale.ToString();
            snapToggle.isOn = _data.snapToGrid;
        }
        public void ChangeColor(Color color)
        {
            data.color = color;
            Events.OnGridChanged?.Invoke(data, true, false);
            color.a = 1.0f;
            colorButton.color = color;
        }
        public void ChangeDimensions()
        {
            int x = int.Parse(widthInput.text);
            int y = int.Parse(heightInput.text);
            data.dimensions = new Vector2Int(x, y);

            Events.OnGridChanged?.Invoke(data, true, false);
        }

        private void SaveData()
        {
            int x = int.Parse(widthInput.text);
            int y = int.Parse(heightInput.text);

            int scale = string.IsNullOrEmpty(unitScale.text) ? data.unit.scale : int.Parse(unitScale.text);
            string name = string.IsNullOrEmpty(unitName.text) ? data.unit.name : unitName.text;

            data.dimensions = new Vector2Int(x, y);
            data.snapToGrid = snapToggle.isOn;
            data.unit = new GridUnit(unitName.text, scale);

            SocketManager.EmitAsync("modify-grid", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                Events.OnGridChanged?.Invoke(Session.Instance.Settings.grid, true, false);

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(data));
        }
    }
}