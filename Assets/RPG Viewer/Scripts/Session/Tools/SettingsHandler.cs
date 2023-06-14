using System;
using System.IO;
using System.Threading.Tasks;
using Networking;
using SFB;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class SettingsHandler : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

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
        [SerializeField] private ToolButton viewButton;
        [SerializeField] private ToolButton playerButton;
        [SerializeField] private ToolButton visionButton;
        [SerializeField] private ToolButton clearButton;

        [Header("Masks")]
        [SerializeField] private RectMask2D gridMask;
        [SerializeField] private RectMask2D wallsMask;
        [SerializeField] private RectMask2D lightingMask;
        [SerializeField] private RectMask2D viewMask;

        public static SettingsHandler Instance { get; private set; }
        public Setting Setting { get { return activeSetting; } }

        private Setting activeSetting;
        private Setting lastSetting;
        private Setting lastWalls = Setting.Walls_Regular;
        private Setting lastLighting = Setting.Lighting_Create;
        private GameView lastView = GameView.Clear;

        private void Awake()
        {
            // Create instance
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        private void OnEnable()
        {
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
            if (activeSetting != lastSetting)
            {
                lastSetting = activeSetting;
                Events.OnSettingChanged?.Invoke(activeSetting);
            }
        }

        private void HandleStateChange(SessionState oldState, SessionState newState)
        {
            bool synced = newState.synced || ConnectionManager.Info.isMaster;
            canvasGroup.alpha = (string.IsNullOrEmpty(newState.scene) && synced) ? 0.0f : 1.0f;
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
        public void SelectPlayer()
        {
            // Update selections
            playerButton.Select();
            visionButton.Deselect();
            clearButton.Deselect();

            // Update tool states
            lastView = GameView.Player;
            Events.OnViewChanged?.Invoke(GameView.Player);
        }
        public void SelectVision()
        {
            // Update selections
            visionButton.Select();
            playerButton.Deselect();
            clearButton.Deselect();

            // Update tool states
            lastView = GameView.Vision;
            Events.OnViewChanged?.Invoke(GameView.Vision);
        }
        public void SelectClear()
        {
            // Update selections
            clearButton.Select();
            playerButton.Deselect();
            visionButton.Deselect();

            // Update tool states
            lastView = GameView.Clear;
            Events.OnViewChanged?.Invoke(GameView.Clear);
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

        public void OpenGrid()
        {
            // Update selections
            CloseWalls();
            CloseLighting();
            CloseView();

            // Close panel if it's open
            if (!gridMask.enabled)
            {
                CloseGrid();
                activeSetting = Setting.None;
                return;
            }

            gridMask.enabled = false;
            gridButton.Select();

            // Update tool state
            activeSetting = Setting.Grid;
        }
        public void CloseGrid()
        {
            gridMask.enabled = true;
            gridButton.Deselect();
        }
        public void OpenWalls()
        {
            // Update selections
            CloseGrid();
            CloseLighting();
            CloseView();

            // Close panel if it's open
            if (!wallsMask.enabled)
            {
                CloseWalls();
                activeSetting = Setting.None;
                return;
            }

            // Update rect size
            wallsMask.enabled = false;
            wallsButton.Select();


            // Activate last tool selection
            if (lastWalls == Setting.Walls_Regular) SelectRegular();
            else if (lastWalls == Setting.Walls_Invisible) SelectInvisible();
            else SelectDoors();
        }
        public void CloseWalls()
        {
            wallsMask.enabled = true;
            wallsButton.Deselect();
        }
        public void OpenLighting()
        {
            // Update selections
            CloseGrid();
            CloseWalls();
            CloseView();

            // Close panel if it's open
            if (!lightingMask.enabled)
            {
                CloseLighting();
                activeSetting = Setting.None;
                return;
            }

            // Update rect size
            lightingMask.enabled = false;
            lightingButton.Select();

            // Activate last tool selection
            if (lastLighting == Setting.Lighting_Create) SelectCreate();
            else SelectDelete();
        }
        public void CloseLighting()
        {
            lightingMask.enabled = true;
            lightingButton.Deselect();
        }
        public void OpenView()
        {
            // Update selections
            CloseGrid();
            CloseLighting();
            CloseWalls();

            // Close panel if it's open
            if (!viewMask.enabled)
            {
                CloseView();
                activeSetting = Setting.None;
                return;
            }

            // Update rect size
            viewMask.enabled = false;
            viewButton.Select();


            // Activate last tool selection
            if (lastView == GameView.Player) SelectPlayer();
            else if (lastView == GameView.Vision) SelectPlayer();
            else SelectClear();
        }
        public void CloseView()
        {
            viewMask.enabled = true;
            viewButton.Deselect();
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