using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPG
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SessionGrid : MonoBehaviour
    {
        private Mesh mesh;
        private Vector3[] vertices;
        private Vector2[] uv;
        private int[] triangles;

        public Color Color;
        public Vector2Int Dimensions;
        public float CellSize;
        public Vector2 Position;
        [SerializeField] private Material GridMaterial;
        [HideInInspector] public List<Cell> Cells = new();

        private bool oddDiagonals;

        private void Awake()
        {
            if (mesh == null) mesh = GetComponent<MeshFilter>().mesh;
            if (GridMaterial != null)
            {
                GetComponent<MeshRenderer>().material = GridMaterial;
                GetComponent<MeshRenderer>().material.color = Color;
            }
        }

        public void GenerateGrid(Vector2Int dimensions, Vector2 position, float cellSize)
        {
            Cells.Clear();
            vertices = new Vector3[4 * (dimensions.x * dimensions.y)];
            uv = new Vector2[4 * (dimensions.x * dimensions.y)];
            triangles = new int[6 * (dimensions.x * dimensions.y)];

            for (int x = 0; x < dimensions.x; x++)
            {
                for (int y = 0; y < dimensions.y; y++)
                {
                    int index = x * dimensions.y + y;

                    Vector2 pos = new(x * cellSize + cellSize * 0.5f + position.x, y * cellSize + cellSize * 0.5f + position.y);
                    Cells.Add(new Cell(pos, new Vector2Int(x, y)));

                    vertices[index * 4 + 0] = new Vector3(cellSize * x + position.x, cellSize * y + position.y);
                    vertices[index * 4 + 1] = new Vector3(cellSize * x + position.x, cellSize * (y + 1) + position.y);
                    vertices[index * 4 + 2] = new Vector3(cellSize * (x + 1) + position.x, cellSize * (y + 1) + position.y);
                    vertices[index * 4 + 3] = new Vector3(cellSize * (x + 1) + position.x, cellSize * y + position.y);

                    uv[index * 4 + 0] = new Vector2(0, 0);
                    uv[index * 4 + 1] = new Vector2(0, 1);
                    uv[index * 4 + 2] = new Vector2(1, 1);
                    uv[index * 4 + 3] = new Vector2(1, 0);

                    triangles[index * 6 + 0] = index * 4 + 0;
                    triangles[index * 6 + 1] = index * 4 + 1;
                    triangles[index * 6 + 2] = index * 4 + 2;

                    triangles[index * 6 + 3] = index * 4 + 0;
                    triangles[index * 6 + 4] = index * 4 + 2;
                    triangles[index * 6 + 5] = index * 4 + 3;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            Dimensions = dimensions;
            Position = position;
            CellSize = cellSize;

            var scene = FindObjectOfType<ScenePanel>();
            if (scene == null) return;
            scene.Data.grid.position = position;
            scene.Data.grid.cellSize = cellSize;
        }
        public void UpdateColor(Color color)
        {
            this.Color = color;
            GetComponent<MeshRenderer>().sharedMaterial.color = color;
        }

        public Cell PointToCell(Vector2 point)
        {
            if (Dimensions == Vector2Int.zero) return new Cell(point, Vector2Int.zero);
            Cell c = default;
            float closest = 0;
            for (int i = 0; i < Cells.Count; i++)
            {
                float distance = Vector2.Distance(point, Cells[i].position);
                if (i == 0 || distance < closest)
                {
                    closest = distance;
                    c = Cells[i];
                }
            }
            return c;
        }
        public Cell GetCell(int row, int column)
        {
            var cell = Cells.FirstOrDefault(x => x.cell == new Vector2Int(column, row));
            return cell;
        }

        public Vector2 SnapToGrid(Vector2 point, Vector2 tokenSize)
        {
            if (Dimensions == Vector2Int.zero) return point;

            float closest = 0;
            Vector2 closestPoint = Vector2.zero;
            foreach (var cell in Cells)
            {
                Vector2 finalPoint = cell.position;
                if ((tokenSize.x % 10 < 2.5f || tokenSize.y % 10 < 2.5f) && (tokenSize.x >= 5.0f && tokenSize.y >= 5.0f)) finalPoint -= new Vector2(CellSize * 0.5f, CellSize * 0.5f);
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
            float feet = 0;
            Cell startCell = PointToCell(start);
            Cell endCell = PointToCell(end);

            int diagonals = 0;
            if (type == MeasurementType.Grid)
            {
                int dX = Mathf.Abs(endCell.cell.x - startCell.cell.x);
                int dY = Mathf.Abs(endCell.cell.y - startCell.cell.y);

                if (dX > 0 && dY > 0)
                {
                    if (dX > dY)
                    {
                        diagonals = dY;
                        feet += (dX - dY) * 5;
                    }
                    else if (dY > dX)
                    {
                        diagonals = dX;
                        feet += (dY - dX) * 5;
                    }
                    else diagonals = dY;
                }
                else feet += (dX + dY) * 5;
            }
            else
            {
                float dist = Vector2.Distance(end, start);
                feet = dist / (CellSize * 0.2f);
            }

            return new MeasurementResult()
            {
                startPos = type == MeasurementType.Grid ? startCell.position : start,
                endPos = type == MeasurementType.Grid ? endCell.position : end,
                distance = feet,
                diagonals = diagonals
            };
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