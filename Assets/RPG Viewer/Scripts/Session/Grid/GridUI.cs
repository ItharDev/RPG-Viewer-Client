using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace RPG
{
    public class GridUI : MonoBehaviour
    {
        [SerializeField] private Color hoveredColor;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool drawGizmos;
        [SerializeField] private float lineWidth;
        [SerializeField] private List<GridCorner> corners;

        private GridManager grid;
        private GridData gridData;
        private VectorLine line;
        private float moveSpeed;

        private void Awake()
        {
            if (grid == null) grid = GetComponent<GridManager>();
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnSceneLoaded.AddListener(LoadScene);
            Events.OnGridChanged.AddListener(ReloadGrid);
            Events.OnSettingChanged.AddListener(ToggleUI);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSceneLoaded.RemoveListener(LoadScene);
            Events.OnGridChanged.RemoveListener(ReloadGrid);
            Events.OnSettingChanged.RemoveListener(ToggleUI);
        }
        private void Update()
        {
            if (!canvasGroup.blocksRaycasts) return;

            // Check if any of the arrow keys are pressed down
            if (!CheckArrowKeys()) return;

            MoveGrid();
        }
        private bool CheckArrowKeys()
        {
            // Check if up or down arrow is pressed
            if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)) return true;

            // Check if left or righ arrow is pressed
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) return true;
            return false;
        }
        private void MoveGrid()
        {
            // Get movement direction
            float inputX = Input.GetAxisRaw("Horizontal");
            float inputY = Input.GetAxisRaw("Vertical");

            corners[0].transform.position += new Vector3(inputX * moveSpeed, inputY * moveSpeed);
            corners[1].transform.position += new Vector3(inputX * moveSpeed, inputY * moveSpeed);
            corners[2].transform.position += new Vector3(inputX * moveSpeed, inputY * moveSpeed);
            corners[3].transform.position += new Vector3(inputX * moveSpeed, inputY * moveSpeed);

            gridData.position = corners[2].transform.position;
            UpdateGrid(gridData.cellSize);
        }

        private void ReloadGrid(GridData newData, bool reloadRequired, bool globalUpdate)
        {
            gridData = newData;
            if (reloadRequired) InstantiateGrid();
        }
        private void ToggleUI(Setting setting)
        {
            bool enabled = setting == Setting.Grid;
            canvasGroup.alpha = enabled ? 1.0f : 0.0f;
            canvasGroup.blocksRaycasts = enabled;
        }
        private void LoadScene(SceneData data)
        {
            gridData = data.grid;
            InstantiateGrid();
        }
        private void InstantiateGrid()
        {
            Vector2 worldSize = new Vector2(gridData.dimensions.x * gridData.cellSize, gridData.dimensions.y * gridData.cellSize);

            // Instantiate corners
            corners[0].transform.position = new Vector2(gridData.position.x, gridData.position.y + worldSize.y);
            corners[1].transform.position = new Vector2(gridData.position.x + worldSize.x, gridData.position.y + worldSize.y);
            corners[2].transform.position = new Vector2(gridData.position.x, gridData.position.y);
            corners[3].transform.position = new Vector2(gridData.position.x + worldSize.x, gridData.position.y);

            UpdateGrid(gridData.cellSize);
            line.color = gridData.color;
            line.Draw3DAuto();
        }
        private void UpdateGrid(float cellSize)
        {
            gridData.cellSize = cellSize;
            moveSpeed = cellSize * 0.001f;
            // Generate grid
            List<Vector3> points = new List<Vector3>();

            // Lines down X axis
            for (int i = 0; i <= gridData.dimensions.x; i++)
            {
                points.Add(gridData.position + new Vector2(i * gridData.cellSize, 0));
                points.Add(gridData.position + new Vector2(i * gridData.cellSize, (gridData.dimensions.y) * gridData.cellSize));
            }
            // Lines down Y axis
            for (int i = 0; i <= gridData.dimensions.y; i++)
            {
                points.Add(gridData.position + new Vector2(0, i * gridData.cellSize));
                points.Add(gridData.position + new Vector2(gridData.dimensions.x * gridData.cellSize, i * gridData.cellSize));
            }
            if (line == null) line = new VectorLine("Grid UI", points, lineWidth);
            else
            {
                line.points3 = points;
            }
            Events.OnGridChanged?.Invoke(gridData, false, false);
        }
        public void MoveCorner(CornerType type)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, -1);
            float cellSize = 0.0f;

            switch (type)
            {
                case CornerType.Top_Left:
                    cellSize = Mathf.Abs(mousePos.x - corners[1].transform.position.x) / gridData.dimensions.x;
                    corners[0].transform.position = new Vector3(mousePos.x, corners[2].transform.position.y + gridData.dimensions.y * cellSize);
                    corners[1].transform.position = new Vector3(corners[1].transform.position.x, corners[0].transform.position.y);
                    corners[2].transform.position = new Vector3(corners[0].transform.position.x, corners[2].transform.position.y);
                    break;
                case CornerType.Top_Right:
                    cellSize = Mathf.Abs(mousePos.x - corners[0].transform.position.x) / gridData.dimensions.x;
                    corners[1].transform.position = new Vector3(mousePos.x, corners[3].transform.position.y + gridData.dimensions.y * cellSize);
                    corners[0].transform.position = new Vector3(corners[0].transform.position.x, corners[1].transform.position.y);
                    corners[3].transform.position = new Vector3(corners[1].transform.position.x, corners[3].transform.position.y);
                    break;
                case CornerType.Bottom_Left:
                    cellSize = Mathf.Abs(mousePos.x - corners[1].transform.position.x) / gridData.dimensions.x;
                    corners[2].transform.position = new Vector3(mousePos.x, corners[0].transform.position.y - gridData.dimensions.y * cellSize);
                    corners[0].transform.position = new Vector3(corners[2].transform.position.x, corners[0].transform.position.y);
                    corners[3].transform.position = new Vector3(corners[3].transform.position.x, corners[2].transform.position.y);
                    break;
                case CornerType.Bottom_Right:
                    cellSize = Mathf.Abs(mousePos.x - corners[0].transform.position.x) / gridData.dimensions.x;
                    corners[3].transform.position = new Vector3(mousePos.x, corners[1].transform.position.y - gridData.dimensions.y * cellSize);
                    corners[1].transform.position = new Vector3(corners[3].transform.position.x, corners[1].transform.position.y);
                    corners[2].transform.position = new Vector3(corners[2].transform.position.x, corners[3].transform.position.y);
                    break;
            }

            gridData.position = corners[2].transform.position;
            UpdateGrid(cellSize);
        }

        private void OnDrawGizmos()
        {
            // Return if we are not in play mode
            if (!Application.isPlaying) return;

            // Return if drawing is disabled or grid has not been generated
            if (!drawGizmos || grid.Grid == null) return;

            DrawHover();
        }

        private void DrawHover()
        {
            Gizmos.color = hoveredColor;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 worldPos = grid.WorldPosToCell(mousePos).worldPosition;
            Vector3 cubeSize = new Vector3(grid.CellSize, grid.CellSize, grid.CellSize);
            Gizmos.DrawCube(worldPos, cubeSize);
        }
    }
}