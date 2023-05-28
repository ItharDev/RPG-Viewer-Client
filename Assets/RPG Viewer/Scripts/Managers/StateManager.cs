using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class StateManager : MonoBehaviour
    {
        [Header("Pan")]
        [SerializeField] private GameObject panSelection;

        [Header("Measure")]
        [SerializeField] private GameObject measureSelection;
        [SerializeField] private GameObject measurePanel;
        [SerializeField] private GameObject preciseMeasure;
        [SerializeField] private GameObject gridMeasure;

        [Header("Ping")]
        [SerializeField] private GameObject pingSelection;
        [SerializeField] private GameObject pingPanel;
        [SerializeField] private GameObject regularPing;
        [SerializeField] private GameObject pointerPing;

        [Header("Notes")]
        [SerializeField] private GameObject noteSelection;
        [SerializeField] private GameObject notePanel;
        [SerializeField] private GameObject createNoteSelection;
        [SerializeField] private GameObject deleteNoteSelection;

        [Header("Grid")]
        [SerializeField] private GameObject grid;
        [SerializeField] private GameObject gridPanel;
        [SerializeField] private GameObject gridSelection;
        [SerializeField] private GameObject gridConfig;
        [SerializeField] private Toggle enableGrid;
        [SerializeField] private Toggle snapToGrid;
        [SerializeField] private TMP_InputField columnsInput;
        [SerializeField] private TMP_InputField rowsInput;
        [SerializeField] private Image gridColor;
        [SerializeField] private Slider gridOpacity;
        [SerializeField] private TMP_InputField gridOpacityInput;
        [SerializeField] private FlexibleColorPicker gridColorPicker;
        [SerializeField] private GameObject gridButton;

        [Header("Walls")]
        [SerializeField] private GameObject wallsPanel;
        [SerializeField] private GameObject wallsSelection;
        [SerializeField] private GameObject regularSelection;
        [SerializeField] private GameObject doorsSelection;
        [SerializeField] private GameObject invisibleSelection;
        [SerializeField] private GameObject hiddenDoorSelection;
        [SerializeField] private GameObject wallsButton;


        [Header("Fog")]
        [SerializeField] private GameObject fowSelection;
        [SerializeField] private GameObject fowConfig;
        [SerializeField] private Toggle enableFow;
        [SerializeField] private Toggle globalLighting;
        [SerializeField] private Image fogColor;
        [SerializeField] private Slider translucencySlider;
        [SerializeField] private TMP_InputField translucenyInput;
        [SerializeField] private Slider visionSlider;
        [SerializeField] private TMP_InputField visionInput;
        [SerializeField] private FlexibleColorPicker fogColorPicker;
        [SerializeField] private GameObject playerSelection;
        [SerializeField] private GameObject visionSelection;
        [SerializeField] private GameObject hiddenSelection;
        [SerializeField] private GameObject fogButton;
        [SerializeField] private GameObject fowPanel;

        [Header("Lighting")]
        [SerializeField] private GameObject lightSelection;
        [SerializeField] private GameObject lightPanel;
        [SerializeField] private GameObject lightButton;
        [SerializeField] private GameObject createLightSelection;
        [SerializeField] private GameObject deleteLightSelection;
        [SerializeField] private Slider nightSlider;
        [SerializeField] private TMP_InputField nightInput;

        [Header("Sync")]
        [SerializeField] private GameObject stateButton;
        [SerializeField] private Image stateSprite;
        [SerializeField] private Sprite openState;
        [SerializeField] private Sprite closeState;

        [Header("State")]
        public ToolState ToolState;
        private ToolState lastState;
        public FogState FogState;
        public LightState LightState;
        public NoteState NoteState;
        public MeasurementType MeasureType;
        public PingType PingType;
        public bool allowMeaure;

        [SerializeField] private List<GameObject> hiddenButtons = new List<GameObject>();
        [SerializeField] private GameObject gmTools;
        [SerializeField] private Vector2 gmToolsSize;
        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private GameObject saveButton;

        private bool gmToolsOpen = true;
        private bool loaded;

        private void Start()
        {
            UsePan();
            AllowMeasure();
        }
        private void Update()
        {
            if (!loaded && SessionManager.Session != null)
            {
                loaded = true;
                if (!SessionManager.IsMaster) gmTools.gameObject.SetActive(false);

                ToggleGM();
            }

            var isMaster = SessionManager.IsMaster;

            if (hiddenButtons[0].activeInHierarchy && SessionManager.Session.sprite.sprite == null)
            {
                for (int i = 0; i < hiddenButtons.Count; i++)
                {
                    hiddenButtons[i].SetActive(false);
                }
            }

            if (!hiddenButtons[0].activeInHierarchy && SessionManager.Session.sprite.sprite != null)
            {
                for (int i = 0; i < hiddenButtons.Count; i++)
                {
                    hiddenButtons[i].SetActive(true);
                }
            }

            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1) && panSelection.transform.parent.gameObject.activeInHierarchy) UsePan();
                if (Input.GetKeyDown(KeyCode.Alpha2) && measureSelection.transform.parent.gameObject.activeInHierarchy) UseMeasure();
                if (Input.GetKeyDown(KeyCode.Alpha3) && pingSelection.transform.parent.gameObject.activeInHierarchy) UsePing();
                if (Input.GetKeyDown(KeyCode.Alpha4) && pingSelection.transform.parent.gameObject.activeInHierarchy) UseNotes();
            }

            fogButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            lightButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            lightButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            stateButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            gridButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            wallsButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            saveButton.SetActive(isMaster && !string.IsNullOrEmpty(SessionManager.Scene));
            stateSprite.sprite = SessionManager.Synced ? openState : closeState;
        }

        #region Data
        public void LoadData()
        {
            LoadGridConfig(SessionManager.Session.Settings.grid);
            LoadFowConfig(SessionManager.Session.Settings.darkness);
            LoadWalls(SessionManager.Session.Settings.walls);

            nightSlider.value = SessionManager.Session.Settings.info.nightStrength;
            nightInput.text = SessionManager.Session.Settings.info.nightStrength.ToString();

            if (string.IsNullOrEmpty(SessionManager.Session.Settings.id))
            {
                SessionManager.Session.Settings.info.nightStrength = 0.0f;
            }
            LoadNight(SessionManager.Session.Settings.info.nightStrength);
            grid.transform.GetChild(0).gameObject.SetActive(false);
        }
        public async void SaveData()
        {
            MessageManager.QueueMessage("Uploading changes. This may take a while");

            SceneData data = new SceneData()
            {
                name = SessionManager.Session.Settings.info.name,
                image = SessionManager.Session.Settings.info.image,
                nightStrength = nightSlider.value
            };
            GridData gridData = default;
            FogOfWarData fogData = default;
            List<WallData> wallData = new List<WallData>();

            FindObjectOfType<WallTools>().SaveWalls((walls) =>
            {
                wallData = walls;
            });

            gridData = new GridData()
            {
                enabled = enableGrid.isOn,
                snapToGrid = snapToGrid.isOn,
                dimensions = new Vector2Int(int.Parse(columnsInput.text), int.Parse(rowsInput.text)),
                cellSize = grid.GetComponent<SessionGrid>().CellSize,
                position = grid.GetComponent<SessionGrid>().Position + (Vector2)grid.transform.position,
                color = new Color(gridColor.color.r, gridColor.color.g, gridColor.color.b, gridOpacity.value * 0.01f)
            };

            fogData = new FogOfWarData()
            {
                enabled = enableFow.isOn,
                globalLighting = globalLighting.isOn,
                translucency = translucencySlider.value * 0.01f,
                nightVisionStrength = visionSlider.value * 0.01f,
                color = fogColor.color,
            };

            SessionManager.Session.Settings.info = data;
            SessionManager.Session.Settings.grid = gridData;
            SessionManager.Session.Settings.darkness = fogData;
            SessionManager.Session.Settings.walls = wallData;

            if (string.IsNullOrEmpty(SessionManager.Session.Settings.id))
            {
                await SocketManager.Socket.EmitAsync("upload-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, SessionManager.Session.Settings.path, JsonUtility.ToJson(SessionManager.Session.Settings), string.IsNullOrEmpty(data.image) ? Convert.ToBase64String(SessionManager.Session.Settings.bytes) : null);
            }
            else
            {
                await SocketManager.Socket.EmitAsync("modify-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, SessionManager.Session.Settings.id, JsonUtility.ToJson(SessionManager.Session.Settings));
            }
        }

        public void CancelChanges()
        {
            SessionManager.Session.LoadScene(SessionManager.Scene);
        }

        private void LoadGridConfig(GridData data)
        {
            enableGrid.isOn = data.enabled;
            snapToGrid.isOn = data.snapToGrid;
            columnsInput.text = data.dimensions.x.ToString();
            rowsInput.text = data.dimensions.y.ToString();
            gridColor.color = new Color(data.color.r, data.color.g, data.color.b, 1.0f);
            gridOpacity.value = data.color.a * 100;

            grid.GetComponent<GridVisualisation>().GenerateGrid(data);
            grid.GetComponent<SessionGrid>().UpdateColor(data.color);
            grid.GetComponent<SessionGrid>().GenerateGrid(data.dimensions, data.position, data.cellSize);
        }
        private void LoadFowConfig(FogOfWarData data)
        {
            enableFow.isOn = data.enabled;
            globalLighting.isOn = data.globalLighting;
            translucencySlider.value = data.translucency * 100;
            visionSlider.value = data.nightVisionStrength * 100;
            fogColor.color = new Color(data.color.r, data.color.g, data.color.b, 1.0f);
        }
        private void LoadWalls(List<WallData> walls)
        {
            FindObjectOfType<WallTools>().LoadWalls(walls);
        }
        private void UpdateGrid(GridData data)
        {
            Vector2 position = default;
            float cellSize = 0;

            if (data.cellSize == 0)
            {
                grid.GetComponent<GridVisualisation>().ResetCorners(sprite.sprite.texture, data.dimensions, ref position, ref cellSize);
                data.position = position;
                data.cellSize = cellSize;
            }
            else
            {
                grid.GetComponent<GridVisualisation>().UpdateCorners(data.dimensions, data.cellSize, ref position);
                data.position = position;
            }
            grid.GetComponent<SessionGrid>().UpdateColor(data.color);
            grid.GetComponent<SessionGrid>().GenerateGrid(data.dimensions, data.position, data.cellSize);
            SessionManager.Session.Settings.grid = data;
        }

        public void LoadNight(float strength)
        {
            float s = 0.004f * strength;
            float v = 1.0f - 0.008f * strength;
            sprite.color = Color.HSVToRGB(240.0f / 360.0f, s, v);
        }
        #endregion

        #region Buttons
        public void ToggleGM()
        {
            if (gmToolsOpen)
            {
                gmToolsOpen = false;
                LeanTween.size(gmTools.GetComponent<RectTransform>(), new Vector2(gmToolsSize.x, 30), 0.1f);
                if (gridSelection.activeInHierarchy) ToggleGrid();
                if (fowSelection.activeInHierarchy) ToggleFow();
                if (lightSelection.activeInHierarchy) ToggleLighting();
                if (wallsSelection.activeInHierarchy) ToggleWalls();
            }
            else
            {
                gmToolsOpen = true;
                LeanTween.size(gmTools.GetComponent<RectTransform>(), new Vector2(gmToolsSize.x, gmToolsSize.y), 0.1f);
            }
        }

        public void BlockMeasure()
        {
            allowMeaure = false;
        }
        public void AllowMeasure()
        {
            allowMeaure = true;
        }

        public void UsePan()
        {
            if (gridSelection.activeInHierarchy) ToggleGrid();
            if (fowSelection.activeInHierarchy) ToggleFow();
            if (lightSelection.activeInHierarchy) ToggleLighting();
            if (wallsSelection.activeInHierarchy) ToggleWalls();

            ToolState = ToolState.Pan;
            measurePanel.SetActive(false);
            measureSelection.SetActive(false);
            panSelection.SetActive(true);
            pingSelection.SetActive(false);
            noteSelection.SetActive(false);
            notePanel.SetActive(false);
            pingPanel.SetActive(false);
        }

        public void UseMeasure()
        {
            if (gridSelection.activeInHierarchy) ToggleGrid();
            if (fowSelection.activeInHierarchy) ToggleFow();
            if (lightSelection.activeInHierarchy) ToggleLighting();
            if (wallsSelection.activeInHierarchy) ToggleWalls();

            ToolState = ToolState.Measure;
            measurePanel.SetActive(true);
            panSelection.SetActive(false);
            measureSelection.SetActive(true);
            pingSelection.SetActive(false);
            noteSelection.SetActive(false);
            notePanel.SetActive(false);
            pingPanel.SetActive(false);

            if (!preciseMeasure.activeInHierarchy && !gridMeasure.activeInHierarchy) SelectPrecise();
        }
        public void SelectPrecise()
        {
            gridMeasure.SetActive(false);
            preciseMeasure.SetActive(true);

            MeasureType = MeasurementType.Precise;
        }
        public void SelectGrid()
        {
            gridMeasure.SetActive(true);
            preciseMeasure.SetActive(false);

            MeasureType = MeasurementType.Grid;
        }

        public void SelectPlayer()
        {
            playerSelection.SetActive(true);
            visionSelection.SetActive(false);
            hiddenSelection.SetActive(false);

            FogState = FogState.Player;
            GetComponent<Session>().ChangeFog(FogState);
        }
        public void SelectVision()
        {
            playerSelection.SetActive(false);
            hiddenSelection.SetActive(false);
            visionSelection.SetActive(true);

            FogState = FogState.Vision;
            GetComponent<Session>().ChangeFog(FogState);
        }
        public void SelectHidden()
        {
            playerSelection.SetActive(false);
            hiddenSelection.SetActive(true);
            visionSelection.SetActive(false);

            FogState = FogState.Hidden;
            GetComponent<Session>().ChangeFog(FogState);
        }

        public void UsePing()
        {
            if (gridSelection.activeInHierarchy) ToggleGrid();
            if (fowSelection.activeInHierarchy) ToggleFow();
            if (lightSelection.activeInHierarchy) ToggleLighting();
            if (wallsSelection.activeInHierarchy) ToggleWalls();

            ToolState = ToolState.Ping;
            measurePanel.SetActive(false);
            measureSelection.SetActive(false);
            panSelection.SetActive(false);
            pingSelection.SetActive(true);
            noteSelection.SetActive(false);
            notePanel.SetActive(false);
            pingPanel.SetActive(true);

            if (!regularPing.activeInHierarchy && !pointerPing.activeInHierarchy) SelectPing();
        }
        public void SelectPing()
        {
            pointerPing.SetActive(false);
            regularPing.SetActive(true);

            PingType = PingType.Ping;
        }
        public void SelectPointer()
        {
            pointerPing.SetActive(true);
            regularPing.SetActive(false);

            PingType = PingType.Pointer;
        }

        public void UseNotes()
        {
            if (gridSelection.activeInHierarchy) ToggleGrid();
            if (fowSelection.activeInHierarchy) ToggleFow();
            if (lightSelection.activeInHierarchy) ToggleLighting();
            if (wallsSelection.activeInHierarchy) ToggleWalls();

            ToolState = ToolState.Notes;
            measurePanel.SetActive(false);
            measureSelection.SetActive(false);
            panSelection.SetActive(false);
            pingSelection.SetActive(false);
            noteSelection.SetActive(true);
            notePanel.SetActive(true);
            pingPanel.SetActive(false);

            if (!createNoteSelection.activeInHierarchy && !deleteNoteSelection.activeInHierarchy) CreateNote();
        }
        public void CreateNote()
        {
            createNoteSelection.SetActive(true);
            deleteNoteSelection.SetActive(false);

            NoteState = NoteState.Create;
        }
        public void DeleteNote()
        {
            createNoteSelection.SetActive(false);
            deleteNoteSelection.SetActive(true);

            NoteState = NoteState.Delete;
        }

        public void ToggleWalls()
        {
            if (gridSelection.activeInHierarchy) ToggleGrid();
            if (fowSelection.activeInHierarchy) ToggleFow();
            if (lightSelection.activeInHierarchy) ToggleLighting();

            wallsSelection.SetActive(!wallsSelection.activeInHierarchy);
            wallsPanel.SetActive(wallsSelection.activeInHierarchy);

            FindObjectOfType<WallTools>().EnableWalls(wallsSelection.activeInHierarchy);

            if (!regularSelection.activeInHierarchy && !doorsSelection.activeInHierarchy && !hiddenDoorSelection.activeInHierarchy && !invisibleSelection.activeInHierarchy)
            {
                SelectWalls();
            }
        }
        public void ToggleGrid()
        {
            if (wallsSelection.activeInHierarchy) ToggleWalls();
            if (fowSelection.activeInHierarchy) ToggleFow();
            if (lightSelection.activeInHierarchy) ToggleLighting();

            gridSelection.SetActive(!gridSelection.activeInHierarchy);
            gridPanel.SetActive(gridSelection.activeInHierarchy);
        }
        public void ToggleLighting()
        {
            if (gridSelection.activeInHierarchy) ToggleGrid();
            if (wallsSelection.activeInHierarchy) ToggleWalls();
            if (fowSelection.activeInHierarchy) ToggleFow();

            lightSelection.SetActive(!lightSelection.activeInHierarchy);
            lightPanel.SetActive(lightSelection.activeInHierarchy);
            if (lightSelection.activeInHierarchy)
            {
                lastState = ToolState;
                ToolState = ToolState.Light;
            }
            else ToolState = lastState;

            if (!createLightSelection.activeInHierarchy && !deleteLightSelection.activeInHierarchy) CreateLight();
        }

        public void CreateLight()
        {
            createLightSelection.SetActive(true);
            deleteLightSelection.SetActive(false);

            LightState = LightState.Create;
        }
        public void DeleteLight()
        {
            createLightSelection.SetActive(false);
            deleteLightSelection.SetActive(true);

            LightState = LightState.Delete;
        }

        public void ConfigGrid()
        {
            gridConfig.SetActive(!gridConfig.activeInHierarchy);
            if (gridConfig.activeInHierarchy) LoadGridConfig(SessionManager.Session.Settings.grid);
        }
        public void ShowGrid()
        {
            grid.transform.GetChild(0).gameObject.SetActive(!grid.transform.GetChild(0).gameObject.activeInHierarchy);
        }
        public void ToggleFow()
        {
            if (gridSelection.activeInHierarchy) ToggleGrid();
            if (wallsSelection.activeInHierarchy) ToggleWalls();
            if (lightSelection.activeInHierarchy) ToggleLighting();

            fowSelection.SetActive(!fowSelection.activeInHierarchy);
            fowPanel.SetActive(fowSelection.activeInHierarchy);
        }
        public void ConfigFog()
        {
            fowConfig.SetActive(!fowConfig.activeInHierarchy);
            if (fowConfig.activeInHierarchy) LoadFowConfig(SessionManager.Session.Settings.darkness);
        }

        public void SelectWalls()
        {
            regularSelection.SetActive(true);
            doorsSelection.SetActive(false);
            hiddenDoorSelection.SetActive(false);
            invisibleSelection.SetActive(false);

            FindObjectOfType<WallTools>().ChangeWallType(WallType.Wall);
        }
        public void SelectDoors()
        {
            regularSelection.SetActive(false);
            doorsSelection.SetActive(true);
            hiddenDoorSelection.SetActive(false);
            invisibleSelection.SetActive(false);

            FindObjectOfType<WallTools>().ChangeWallType(WallType.Door);
        }
        public void SelectInvisible()
        {
            regularSelection.SetActive(false);
            doorsSelection.SetActive(false);
            hiddenDoorSelection.SetActive(false);
            invisibleSelection.SetActive(true);

            FindObjectOfType<WallTools>().ChangeWallType(WallType.Invisible);
        }
        public void SelectHiddenDoor()
        {
            regularSelection.SetActive(false);
            doorsSelection.SetActive(false);
            invisibleSelection.SetActive(false);
            hiddenDoorSelection.SetActive(true);

            FindObjectOfType<WallTools>().ChangeWallType(WallType.Hidden_Door);
        }

        public void SaveGridConfiguration()
        {
            if (int.Parse(columnsInput.text) < 1 || int.Parse(rowsInput.text) < 1)
            {
                MessageManager.QueueMessage("Invalid dimensions");
                return;
            }

            GridData data = new GridData()
            {
                enabled = enableGrid.isOn,
                snapToGrid = snapToGrid.isOn,
                dimensions = new Vector2Int(int.Parse(columnsInput.text), int.Parse(rowsInput.text)),
                cellSize = grid.GetComponent<SessionGrid>().CellSize,
                position = grid.GetComponent<SessionGrid>().Position + (Vector2)grid.transform.position,
                color = new Color(gridColor.color.r, gridColor.color.g, gridColor.color.b, gridOpacity.value * 0.01f)
            };

            UpdateGrid(data);
        }
        public void SaveFowConfiguration()
        {
            FogOfWarData data = new FogOfWarData()
            {
                enabled = enableFow.isOn,
                globalLighting = globalLighting.isOn,
                translucency = translucencySlider.value * 0.01f,
                nightVisionStrength = visionSlider.value * 0.01f,
                color = fogColor.color,
            };

            SessionManager.Session.Settings.darkness = data;
        }
        public void SaveNightConfiguration()
        {
            SessionManager.Session.Settings.info.nightStrength = nightSlider.value;
        }

        public void OpenGridColor()
        {
            gridColorPicker.SetColor(gridColor.color);
            gridColorPicker.gameObject.SetActive(true);
        }
        public void CloseGridColor()
        {
            gridColor.color = gridColorPicker.color;
            gridColorPicker.gameObject.SetActive(false);
        }
        public void ChangeGridSlider()
        {
            gridOpacityInput.text = gridOpacity.value.ToString();
        }
        public void ChangeGridInput()
        {
            if (float.Parse(gridOpacityInput.text) > 100.0f || float.Parse(gridOpacityInput.text) < 0.0f) ChangeGridSlider();
            gridOpacity.value = float.Parse(gridOpacityInput.text);
        }

        public void OpenFogColor()
        {
            fogColorPicker.SetColor(fogColor.color);
            fogColorPicker.gameObject.SetActive(true);
        }
        public void CloseFogColor()
        {
            fogColor.color = fogColorPicker.color;
            fogColorPicker.gameObject.SetActive(false);
        }
        public void ChangeTranslucencySlider()
        {
            translucenyInput.text = translucencySlider.value.ToString();
        }
        public void ChangeTranslucencyInput()
        {
            if (float.Parse(translucenyInput.text) > 100.0f || float.Parse(translucenyInput.text) < 0.0f) ChangeTranslucencySlider();
            translucencySlider.value = float.Parse(translucenyInput.text);
        }
        public void ChangeVisionSlider()
        {
            visionInput.text = visionSlider.value.ToString();
        }
        public void ChangeVisionInput()
        {
            if (float.Parse(visionInput.text) > 100.0f || float.Parse(visionInput.text) < 0.0f) ChangeVisionSlider();
            visionSlider.value = float.Parse(visionInput.text);
        }

        public void ChangeNightSlider()
        {
            nightInput.text = nightSlider.value.ToString();
            LoadNight(nightSlider.value);
        }
        public void ChangeNightInput()
        {
            if (float.Parse(nightInput.text) > 100.0f || float.Parse(nightInput.text) < 0.0f) ChangeNightSlider();
            nightSlider.value = float.Parse(nightInput.text);

            LoadNight(nightSlider.value);
        }
        #endregion
    }

    public enum ToolState
    {
        Pan,
        Light,
        Measure,
        Ping,
        Notes
    }
    public enum FogState
    {
        Player,
        Vision,
        Hidden
    }
    public enum LightState
    {
        Create,
        Delete
    }
    public enum NoteState
    {
        Create,
        Delete
    }

}