using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class ToolHandler : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

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

        [Header("Masks")]
        [SerializeField] private RectMask2D measureMask;
        [SerializeField] private RectMask2D pingMask;
        [SerializeField] private RectMask2D notesMask;

        public static ToolHandler Instance { get; private set; }

        private Tool activeTool;
        private Tool lastTool = Tool.Move;
        private Tool lastMeasure = Tool.Measure_Precise;
        private Tool lastPing = Tool.Ping_Marker;
        private Tool lastNotes = Tool.Notes_Create;

        private void Awake()
        {
            // Create instance
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        private void OnEnable()
        {
            SelectMove();
            // Add event listeners
            Events.OnStateChanged.AddListener(HandleStateChange);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnStateChanged.RemoveListener(HandleStateChange);
        }
        private void Update()
        {
            // Send tool change event whenever the user changes the tool
            if (activeTool != lastTool)
            {
                lastTool = activeTool;
                Events.OnToolChanged?.Invoke(activeTool);
            }
        }

        private void HandleStateChange(SessionState oldState, SessionState newState)
        {
            canvasGroup.alpha = string.IsNullOrEmpty(newState.scene) ? 0.0f : 1.0f;
        }

        public void SelectMove()
        {
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
            // Update selections
            preciseButton.Select();
            gridButton.Deselect();

            // Update tool states
            activeTool = Tool.Measure_Precise;
            lastMeasure = Tool.Measure_Precise;
        }
        public void SelectGrid()
        {
            // Update selections
            gridButton.Select();
            preciseButton.Deselect();

            // Update tool states
            activeTool = Tool.Measure_Grid;
            lastMeasure = activeTool;
        }
        public void SelectMark()
        {
            // Update selections
            markButton.Select();
            pointerButton.Deselect();

            // Update tool states
            activeTool = Tool.Ping_Marker;
            lastPing = activeTool;
        }
        public void SelectPointer()
        {
            // Update selections
            pointerButton.Select();
            markButton.Deselect();

            // Update tool states
            activeTool = Tool.Ping_Pointer;
            lastPing = activeTool;
        }
        public void SelectCreate()
        {
            // Update selections
            createButton.Select();
            deleteButton.Deselect();

            // Update tool states
            activeTool = Tool.Notes_Create;
            lastNotes = activeTool;
        }
        public void SelectDelete()
        {
            // Update selections
            deleteButton.Select();
            createButton.Deselect();

            // Update tool states
            activeTool = Tool.Notes_Create;
            lastNotes = activeTool;
        }

        public void OpenMeasure()
        {
            measureMask.enabled = false;

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
            measureMask.enabled = true;

            // Update selections
            measureButton.Deselect();
            preciseButton.Deselect();
            gridButton.Deselect();
        }
        public void OpenPing()
        {
            pingMask.enabled = false;

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
            pingMask.enabled = true;

            // Update selections
            pingButton.Deselect();
            markButton.Deselect();
            pointerButton.Deselect();
        }
        public void OpenNotes()
        {
            notesMask.enabled = false;

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
            notesMask.enabled = true;

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