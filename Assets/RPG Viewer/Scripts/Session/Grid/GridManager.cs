using UnityEngine;

namespace RPG
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class GridManager : MonoBehaviour
    {
        public float CellSize { get { return cellSize; } }

        private Color gridColor;
        private Vector2Int dimensions;
        private Vector2 worldSize;
        private float cellSize;
        private Vector2 position;
        private Material GridMaterial;
        public Cell[,] Grid { get; private set; }

        private Mesh mesh;
        private MeshRenderer meshRenderer;
        private Vector3[] vertices;
        private Vector2[] uv;
        private int[] triangles;

        private void OnEnable()
        {
            // Get reference of the mesh
            if (mesh == null) mesh = GetComponent<MeshFilter>().mesh;
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();

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
            gridColor = gridData.color;

            // Generate grid
            GenerateGrid(dimensions, position, cellSize);
        }

        public void GenerateGrid(Vector2Int dimensions, Vector2 position, float cellSize)
        {
            // Calculate the number of vertices, uvs and triangles
            vertices = new Vector3[4 * (dimensions.x * dimensions.y)];
            uv = new Vector2[4 * (dimensions.x * dimensions.y)];
            triangles = new int[6 * (dimensions.x * dimensions.y)];
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

                    // Generate vertices
                    vertices[index * 4 + 0] = new Vector3(cellSize * x + position.x, cellSize * y + position.y);
                    vertices[index * 4 + 1] = new Vector3(cellSize * x + position.x, cellSize * (y + 1) + position.y);
                    vertices[index * 4 + 2] = new Vector3(cellSize * (x + 1) + position.x, cellSize * (y + 1) + position.y);
                    vertices[index * 4 + 3] = new Vector3(cellSize * (x + 1) + position.x, cellSize * y + position.y);

                    // Generate uvs
                    uv[index * 4 + 0] = new Vector2(0, 0);
                    uv[index * 4 + 1] = new Vector2(0, 1);
                    uv[index * 4 + 2] = new Vector2(1, 1);
                    uv[index * 4 + 3] = new Vector2(1, 0);

                    // Generate triangles
                    triangles[index * 6 + 0] = index * 4 + 0;
                    triangles[index * 6 + 1] = index * 4 + 1;
                    triangles[index * 6 + 2] = index * 4 + 2;

                    triangles[index * 6 + 3] = index * 4 + 0;
                    triangles[index * 6 + 4] = index * 4 + 2;
                    triangles[index * 6 + 5] = index * 4 + 3;
                }
            }

            // Clear mesh
            mesh.Clear();

            // Apply new data to mesh, and recalculate it
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            meshRenderer.sharedMaterial.color = gridColor;
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

        private void OnDrawGizmosSelected()
        {
            if (Grid == null) return;
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.4f);

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