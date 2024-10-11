using UnityEngine;

namespace RPG
{
    public class GridManager : MonoBehaviour
    {
        public float CellSize { get { return cellSize; } }
        public GridUnit Unit { get { return unit; } }

        private GridData gridData;
        private Vector2Int dimensions;
        private Vector2 worldSize;
        private float cellSize;
        private Vector2 position;
        private GridUnit unit;
        public Cell[,] Grid { get; private set; }

        private void OnEnable()
        {
            // Add event listeners
            Events.OnSceneLoaded.AddListener(LoadScene);
            Events.OnGridChanged.AddListener(LoadGrid);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSceneLoaded.RemoveListener(LoadScene);
            Events.OnGridChanged.AddListener(LoadGrid);
        }
        private void LoadGrid(GridData data, bool reloadRequired, bool globalUpdate)
        {
            if (globalUpdate)
            {
                gridData = data;

                // Load parameters
                dimensions = gridData.dimensions;
                cellSize = gridData.cellSize;
                position = gridData.position;
                unit = data.unit;

                // Generate grid
                worldSize = new Vector2(dimensions.x * cellSize, dimensions.y * cellSize);
                Grid = new Cell[dimensions.x, dimensions.y];

                // Loop through colums
                for (int x = 0; x < dimensions.x; x++)
                {
                    // Loop through rows
                    for (int y = 0; y < dimensions.y; y++)
                    {
                        // Calculate corner index
                        int index = x * dimensions.y + y;

                        // Calculate cell position and add it to the list
                        Vector2 cellPosition = new(x * cellSize + cellSize * 0.5f + position.x, y * cellSize + cellSize * 0.5f + position.y);
                        Grid[x, y] = new Cell(cellPosition, new Vector2Int(x, y));
                    }
                }
            }
        }

        private void LoadScene(SceneData data)
        {
            gridData = data.grid;

            LoadGrid(gridData, true, true);
        }

        public Cell WorldPosToCell(Vector2 worldPos)
        {
            // Calculate the percentage the position is in grid size
            float percentX = (worldPos.x - position.x) / worldSize.x;
            float percentY = (worldPos.y - position.y) / worldSize.y;

            // Clamp percentages between 0 - 1
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            // Calculate cell position from percentage
            int x = Mathf.FloorToInt(dimensions.x * percentX);
            int y = Mathf.FloorToInt(dimensions.y * percentY);

            // Clamp cell position to grid size
            x = Mathf.Clamp(x, 0, dimensions.x - 1);
            y = Mathf.Clamp(y, 0, dimensions.y - 1);

            return Grid[x, y];
        }

        public Cell GetCell(int column, int row)
        {
            if (column >= gridData.dimensions.x || row >= gridData.dimensions.y || column < 0 || row < 0) return new Cell(Vector2.zero, Vector2Int.zero);

            // Find cell in the specified grid position
            return Grid[column, row];
        }

        public Vector2 SnapToGrid(Vector2 point, Vector2 tokenSize)
        {
            if (dimensions == Vector2Int.zero || !gridData.snapToGrid) return point;

            float closest = 0;
            Vector2 closestPoint = Vector2.zero;
            foreach (var cell in Grid)
            {
                Vector2 finalPoint = cell.worldPosition;
                if ((tokenSize.x % (unit.scale * 2) < (unit.scale / 2) || tokenSize.y % (unit.scale * 2) < (unit.scale / 2)) && (tokenSize.x >= unit.scale && tokenSize.y >= unit.scale)) finalPoint -= new Vector2(cellSize * 0.5f, cellSize * 0.5f);
                float distance = Vector2.Distance(point, finalPoint);

                if ((closest == 0) || distance < closest)
                {
                    closest = distance;
                    closestPoint = finalPoint;
                }
            }

            return closestPoint;
        }

        public MeasurementResult DistanceBetweenPoints(Vector2 start, Vector2 end, MeasurementType type)
        {
            if (type == MeasurementType.Grid)
            {
                float distance = 0;
                Cell startCell = WorldPosToCell(start);
                Cell endCell = WorldPosToCell(end);

                int diagonals = 0;
                if (start != end)
                {
                    int dstX = Mathf.Abs(endCell.gridPosition.x - startCell.gridPosition.x);
                    int dstY = Mathf.Abs(endCell.gridPosition.y - startCell.gridPosition.y);

                    if (dstX >= dstY)
                    {
                        diagonals = dstY;
                        distance += (dstX - dstY) * unit.scale;
                    }
                    else if (dstY > dstX)
                    {
                        diagonals = dstX;
                        distance += (dstY - dstX) * unit.scale;
                    }
                    else distance += (dstX + dstY) * unit.scale;
                }

                return new MeasurementResult(startCell.worldPosition, endCell.worldPosition, distance, diagonals);
            }

            float finalDistance = Vector2.Distance(end, start) / (CellSize / unit.scale);
            return new MeasurementResult(start, end, finalDistance, 0);
        }
    }
}