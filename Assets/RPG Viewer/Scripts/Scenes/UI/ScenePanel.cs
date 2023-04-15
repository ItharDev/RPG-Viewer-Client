using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class ScenePanel : MonoBehaviour
    {
        public SceneSettings Data = default;

        [SerializeField] private SpriteRenderer sprite;
        [SerializeField] private StateManager stateManager;

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

        [Header("Walls")]
        [SerializeField] private GameObject wallsPanel;
        [SerializeField] private GameObject wallsSelection;
        [SerializeField] private GameObject regularSelection;
        [SerializeField] private GameObject doorsSelection;
        [SerializeField] private GameObject invisibleSelection;
        [SerializeField] private GameObject hiddenDoorSelection;

        [Header("Fog Of War")]
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

        [Header("Lighting")]
        [SerializeField] private GameObject lightSelection;
        [SerializeField] private GameObject lightPanel;
        [SerializeField] private Slider nightSlider;
        [SerializeField] private TMP_InputField nightInput;

        #region Data
        public void LoadData()
        {
            Data = SessionManager.Session.Settings;
            if (Data == null) return;

            LoadGridConfig(Data.grid);
            LoadFowConfig(Data.fogOfWar);
            LoadWalls(Data.walls);

            nightSlider.value = Data.data.nightStrength;
            nightInput.text = Data.data.nightStrength.ToString();

            if (string.IsNullOrEmpty(Data.id))
            {
                Data.data.nightStrength = 0.0f;
            }
            LoadNight(Data.data.nightStrength);
            grid.transform.GetChild(0).gameObject.SetActive(false);
        }
        public async void SaveData()
        {
            MessageManager.QueueMessage("Uploading changes. This may take a while");

            SceneData data = new SceneData()
            {
                name = Data.data.name,
                image = Data.data.image,
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

            Data.data = data;
            Data.grid = gridData;
            Data.fogOfWar = fogData;
            Data.walls = wallData;

            if (string.IsNullOrEmpty(Data.id))
            {
                await SocketManager.Socket.EmitAsync("upload-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Data.path, JsonUtility.ToJson(Data), string.IsNullOrEmpty(data.image) ? Convert.ToBase64String(Data.bytes) : null);
            }
            else
            {
                await SocketManager.Socket.EmitAsync("modify-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Data.id, JsonUtility.ToJson(Data));
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
            Data.grid = data;
        }

        public void LoadNight(float strength)
        {
            float s = 0.004f * strength;
            float v = 1.0f - 0.008f * strength;
            sprite.color = Color.HSVToRGB(240.0f / 360.0f, s, v);
        }
        #endregion

        #region Buttons
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
        }

        public void ConfigGrid()
        {
            gridConfig.SetActive(!gridConfig.activeInHierarchy);
            if (gridConfig.activeInHierarchy) LoadGridConfig(Data.grid);
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
            fowConfig.SetActive(fowSelection.activeInHierarchy);

            LoadFowConfig(Data.fogOfWar);
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

            Data.fogOfWar = data;
        }
        public void SaveNightConfiguration()
        {
            Data.data.nightStrength = nightSlider.value;
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
}