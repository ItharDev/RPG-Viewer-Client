using UnityEngine;

namespace RPG
{
    public class GridManager : MonoBehaviour
    {
        public float CellSize { get { return cellSize; } }

        private Vector2Int dimensions;
        private Vector2 worldSize;
        private float cellSize;
        private Vector2 position;
        public Cell[,] Grid { get; private set; }

        private void OnEnable()
        {
            // Add event listeners
            Events.OnSceneLoaded.AddListener(LoadGrid);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSceneLoaded.RemoveListener(LoadGrid);
        }

        private void LoadGrid(SceneData data)
        {
            GridData gridData = data.grid;

            // Load parameters
            dimensions = gridData.dimensions;
            cellSize = gridData.cellSize;
            position = gridData.position;

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
                    Grid[x, y] = new Cell(cellPosition);
                }
            }
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

        public Cell GetCell(int row, int column)
        {
            // Find cell in the specified grid position
            return Grid[column, row];
        }

        public Vector2 SnapToGrid(Vector2 point, Vector2 tokenSize)
        {
            if (dimensions == Vector2Int.zero) return point;

            float closest = 0;
            Vector2 closestPoint = Vector2.zero;
            foreach (var cell in Grid)
            {
                Vector2 finalPoint = cell.worldPosition;
                if ((tokenSize.x % 10 < 2.5f || tokenSize.y % 10 < 2.5f) && (tokenSize.x >= 5.0f && tokenSize.y >= 5.0f)) finalPoint -= new Vector2(cellSize * 0.5f, cellSize * 0.5f);
                float distance = Vector2.Distance(point, finalPoint);

                if ((closest == 0) || distance < closest)
                {
                    closest = distance;
                    closestPoint = finalPoint;
                }
            }

            return closestPoint;
        }

        // TODO: 
        public void DistanceBetweenPoints(Vector2 start, Vector2 end)
        {
            // float feet = 0;
            // Cell startCell = WorldPosToCell(start);
            // Cell endCell = WorldPosToCell(end);

            // int diagonals = 0;
            // if (/* TODO: */start != end)
            // {
            //     int dX = Mathf.Abs(endCell.gridPosition.x - startCell.gridPosition.x);
            //     int dY = Mathf.Abs(endCell.gridPosition.y - startCell.gridPosition.y);

            //     if (dX > 0 && dY > 0)
            //     {
            //         if (dX > dY)
            //         {
            //             diagonals = dY;
            //             feet += (dX - dY) * 5;
            //         }
            //         else if (dY > dX)
            //         {
            //             diagonals = dX;
            //             feet += (dY - dX) * 5;
            //         }
            //         else diagonals = dY;
            //     }
            //     else feet += (dX + dY) * 5;
            // }
            // // else
            // // {
            // //     float dist = Vector2.Distance(end, start);
            // //     feet = dist / (CellSize * 0.2f);
            // // }

            // return new MeasurementResult()
            // {
            //     startPos = startCell.worldPosition,
            //     endPos = endCell.worldPosition,
            //     distance = feet,
            //     diagonals = diagonals
            // };
        }
    }

    [System.Serializable]
    public struct MeasurementResult
    {
        public Vector2 startPos;
        public Vector2 endPos;
        public float distance;
        public int diagonals;
    }
}