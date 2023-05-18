using UnityEngine;

namespace RPG
{
    public class SettingsHandler : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private ToolButton gridButton;
        [SerializeField] private ToolButton wallsButton;
        [SerializeField] private ToolButton regularButton;
        [SerializeField] private ToolButton invisibleButton;
        [SerializeField] private ToolButton doorsButton;
        [SerializeField] private ToolButton hiddenButton;
        [SerializeField] private ToolButton lightingButton;
        [SerializeField] private ToolButton createButton;
        [SerializeField] private ToolButton deleteButton;

        [Header("Panels")]
        [SerializeField] private RectTransform gridRect;
        [SerializeField] private RectTransform wallsRect;
        [SerializeField] private RectTransform lightingRect;

        public static SettingsHandler Instance { get; private set; }
        public Setting Setting { get { return activeSetting; } }

        private RectTransform rect;

        private Setting activeSetting;
        private Setting lastSetting;
        private Setting lastWalls = Setting.Walls_Regular;
        private Setting lastLighting = Setting.Lighting_Create;

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
        }
        private void Update()
        {
            // Send tool change event whenever the user changes the tool
            if (activeSetting != lastSetting)
            {
                lastSetting = activeSetting;

                Events.OnSettingChanged?.Invoke(activeSetting);
            }
        }

        public void ConfigureGrid()
        {
            Debug.Log("Configuring grid");
        }
        public void SelectRegular()
        {
            // Update selections
            regularButton.Select();
            invisibleButton.Deselect();
            doorsButton.Deselect();
            hiddenButton.Deselect();

            // Update tool states
            activeSetting = Setting.Walls_Regular;
            lastWalls = activeSetting;
        }
        public void SelectInvisible()
        {
            // Update selections
            invisibleButton.Select();
            regularButton.Deselect();
            doorsButton.Deselect();
            hiddenButton.Deselect();

            // Update tool states
            activeSetting = Setting.Walls_Invisible;
            lastWalls = activeSetting;
        }
        public void SelectDoors()
        {
            // Update selections
            doorsButton.Select();
            regularButton.Deselect();
            invisibleButton.Deselect();
            hiddenButton.Deselect();

            // Update tool states
            activeSetting = Setting.Walls_Door;
            lastWalls = activeSetting;
        }
        public void SelectHidden()
        {
            // Update selections
            hiddenButton.Select();
            doorsButton.Deselect();
            regularButton.Deselect();
            invisibleButton.Deselect();

            // Update tool states
            activeSetting = Setting.Walls_Hidden_Door;
            lastWalls = activeSetting;
        }
        public void ConfigureLighting()
        {
            Debug.Log("Configuring lighting");
        }
        public void SelectCreate()
        {
            // Update selections
            createButton.Select();
            deleteButton.Deselect();

            // Update tool states
            activeSetting = Setting.Lighting_Create;
            lastLighting = activeSetting;
        }
        public void SelectDelete()
        {
            // Update selections
            deleteButton.Select();
            createButton.Deselect();

            // Update tool states
            activeSetting = Setting.Lighting_Delete;
            lastLighting = activeSetting;
        }
        public void SelectImage()
        {
            Debug.Log("Selecting new image");
        }

        public void OpenPanel()
        {
            // Update rect size
            if (rect.sizeDelta.x == 100.0f) LeanTween.size(rect, new Vector2(605.0f, 170.0f), 0.2f);
            else LeanTween.size(rect, new Vector2(100.0f, 170.0f), 0.2f);
        }

        public void OpenGrid()
        {
            // Update selections
            CloseWalls();
            CloseLighting();

            // Close panel if it's open
            if (gridRect.sizeDelta.x == 120.0f)
            {
                CloseGrid();
                activeSetting = Setting.None;
                return;
            }

            // Update rect size
            LeanTween.size(gridRect, new Vector2(120.0f, 65.0f), 0.2f);
            gridButton.Select();

            // Update tool state
            activeSetting = Setting.Grid;
        }
        public void CloseGrid()
        {
            LeanTween.size(gridRect, new Vector2(100.0f, 30.0f), 0.2f);
            gridButton.Deselect();
        }
        public void OpenWalls()
        {
            // Update selections
            CloseGrid();
            CloseLighting();

            // Close panel if it's open
            if (wallsRect.sizeDelta.x == 150.0f)
            {
                CloseWalls();
                activeSetting = Setting.None;
                return;
            }

            // Update rect size
            LeanTween.size(wallsRect, new Vector2(150.0f, 170.0f), 0.2f);
            wallsButton.Select();


            // Activate last tool selection
            if (lastWalls == Setting.Walls_Regular) SelectRegular();
            else if (lastWalls == Setting.Walls_Invisible) SelectInvisible();
            else SelectDoors();
        }
        public void CloseWalls()
        {
            LeanTween.size(wallsRect, new Vector2(100.0f, 30.0f), 0.2f);
            wallsButton.Deselect();
        }
        public void OpenLighting()
        {
            // Update selections
            CloseGrid();
            CloseWalls();

            // Close panel if it's open
            if (lightingRect.sizeDelta.x == 120.0f)
            {
                CloseLighting();
                activeSetting = Setting.None;
                return;
            }

            // Update rect size
            LeanTween.size(lightingRect, new Vector2(120.0f, 135.0f), 0.2f);
            lightingButton.Select();

            // Activate last tool selection
            if (lastLighting == Setting.Lighting_Create) SelectCreate();
            else SelectDelete();
        }
        public void CloseLighting()
        {
            LeanTween.size(lightingRect, new Vector2(100.0f, 30.0f), 0.2f);
            lightingButton.Deselect();
        }
    }

    public enum Setting
    {
        None,
        Grid,
        Walls_Regular,
        Walls_Invisible,
        Walls_Door,
        Walls_Hidden_Door,
        Lighting_Create,
        Lighting_Delete
    }
}