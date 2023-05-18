using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class ToolHandler : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private ToolButton moveButton;
        [SerializeField] private ToolButton measureButton;
        [SerializeField] private ToolButton preciseButton;
        [SerializeField] private ToolButton gridButton;
        [SerializeField] private ToolButton pingButton;
        [SerializeField] private ToolButton markButton;
        [SerializeField] private ToolButton pointerButton;
        [SerializeField] private ToolButton notesButton;
        [SerializeField] private ToolButton createButton;
        [SerializeField] private ToolButton deleteButton;

        [Header("Panels")]
        [SerializeField] private RectTransform measureRect;
        [SerializeField] private RectTransform pingRect;
        [SerializeField] private RectTransform notesRect;

        [Header("Info")]
        [SerializeField] private Image currentIcon;
        [SerializeField] private TMP_Text currentText;

        public static ToolHandler Instance { get; private set; }

        private Tool activeTool;
        private Tool lastTool = Tool.Move;
        private Tool lastMeasure = Tool.Measure_Precise;
        private Tool lastPing = Tool.Ping_Marker;
        private Tool lastNotes = Tool.Notes_Create;
        private RectTransform rect;
        private float targetHeight = 170.0f;

        private void Awake()
        {
            // Create instance
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        private void OnEnable()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();

            SelectMove();
        }
        private void Update()
        {
            // Send tool change event whenever the user changes the tool
            if (activeTool != lastTool)
            {
                lastTool = activeTool;
                targetHeight = activeTool == Tool.Move ? 170.0f : 240.0f;
                LeanTween.size(rect, new Vector2(140.0f, targetHeight), 0.2f);

                Events.OnToolChanged?.Invoke(activeTool);
            }
        }

        public void SelectMove()
        {
            // Update info
            currentText.text = "Move";
            currentIcon.sprite = moveButton.Icon.sprite;

            // Update selections
            moveButton.Select();
            CloseMeasure();
            ClosePing();
            CloseNotes();

            // Update tool state
            activeTool = Tool.Move;
        }
        public void SelectPrecise()
        {
            // Update info
            currentText.text = preciseButton.Header.text;
            currentIcon.sprite = preciseButton.Icon.sprite;

            // Update selections
            preciseButton.Select();
            gridButton.Deselect();

            // Update tool states
            activeTool = Tool.Measure_Precise;
            lastMeasure = Tool.Measure_Precise;
        }
        public void SelectGrid()
        {
            // Update info
            currentText.text = gridButton.Header.text;
            currentIcon.sprite = gridButton.Icon.sprite;

            // Update selections
            gridButton.Select();
            preciseButton.Deselect();

            // Update tool states
            activeTool = Tool.Measure_Grid;
            lastMeasure = activeTool;
        }
        public void SelectMark()
        {
            // Update info
            currentText.text = markButton.Header.text;
            currentIcon.sprite = markButton.Icon.sprite;

            // Update selections
            markButton.Select();
            pointerButton.Deselect();

            // Update tool states
            activeTool = Tool.Ping_Marker;
            lastPing = activeTool;
        }
        public void SelectPointer()
        {
            // Update info
            currentText.text = pointerButton.Header.text;
            currentIcon.sprite = pointerButton.Icon.sprite;

            // Update selections
            pointerButton.Select();
            markButton.Deselect();

            // Update tool states
            activeTool = Tool.Ping_Pointer;
            lastPing = activeTool;
        }
        public void SelectCreate()
        {
            // Update info
            currentText.text = createButton.Header.text;
            currentIcon.sprite = createButton.Icon.sprite;

            // Update selections
            createButton.Select();
            deleteButton.Deselect();

            // Update tool states
            activeTool = Tool.Notes_Create;
            lastNotes = activeTool;
        }
        public void SelectDelete()
        {
            // Update info
            currentText.text = deleteButton.Header.text;
            currentIcon.sprite = deleteButton.Icon.sprite;

            // Update selections
            deleteButton.Select();
            createButton.Deselect();

            // Update tool states
            activeTool = Tool.Notes_Create;
            lastNotes = activeTool;
        }

        public void OpenPanel()
        {
            // Update rect size
            if (rect.sizeDelta.y == 30.0f) LeanTween.size(rect, new Vector2(140.0f, targetHeight), 0.2f);
            else LeanTween.size(rect, new Vector2(140.0f, 30.0f), 0.2f);
        }
        public void OpenMeasure()
        {
            // Update rect size
            LeanTween.size(measureRect, new Vector2(120.0f, 100.0f), 0.2f);

            // Update selections
            measureButton.Select();
            moveButton.Deselect();
            ClosePing();
            CloseNotes();

            // Activate last tool selection
            if (lastMeasure == Tool.Measure_Precise) SelectPrecise();
            else SelectGrid();
        }
        public void CloseMeasure()
        {
            LeanTween.size(measureRect, new Vector2(120.0f, 30.0f), 0.2f);

            // Update selections
            measureButton.Deselect();
            preciseButton.Deselect();
            gridButton.Deselect();
        }
        public void OpenPing()
        {
            LeanTween.size(pingRect, new Vector2(120.0f, 100.0f), 0.2f);

            // Update selections
            pingButton.Select();
            moveButton.Deselect();
            CloseMeasure();
            CloseNotes();

            // Activate last tool selection
            if (lastPing == Tool.Ping_Marker) SelectMark();
            else SelectPointer();
        }
        public void ClosePing()
        {
            LeanTween.size(pingRect, new Vector2(120.0f, 30.0f), 0.2f);

            // Update selections
            pingButton.Deselect();
            markButton.Deselect();
            pointerButton.Deselect();
        }
        public void OpenNotes()
        {
            LeanTween.size(notesRect, new Vector2(120.0f, 100.0f), 0.2f);

            // Update selections
            notesButton.Select();
            moveButton.Deselect();
            CloseMeasure();
            ClosePing();

            // Activate last tool selection
            if (lastNotes == Tool.Notes_Create) SelectCreate();
            else SelectDelete();
        }
        public void CloseNotes()
        {
            LeanTween.size(notesRect, new Vector2(120.0f, 30.0f), 0.2f);

            // Update selections
            notesButton.Deselect();
            createButton.Deselect();
            deleteButton.Deselect();
        }
    }

    public enum Tool
    {
        Move,
        Measure_Precise,
        Measure_Grid,
        Ping_Marker,
        Ping_Pointer,
        Notes_Create,
        Notes_Delete
    }
}