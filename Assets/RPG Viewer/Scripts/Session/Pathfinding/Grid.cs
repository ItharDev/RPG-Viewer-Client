using System.Collections.Generic;
using RPG;
using UnityEngine;

namespace Pathfinding
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private LayerMask unwalkableMask;

        public Vector2[] Path;
        public int MaxSize { get { return gridSizeX * gridSizeY; } }

        private Node[,] grid;
        private float nodeRadius;
        private float nodeDiameter;
        private Vector2 gridWorldSize;
        private Vector2 worldBottomLeft;
        private int gridSizeX, gridSizeY;
        private List<Vector2> fewNeighbours = new List<Vector2>();

        private void OnEnable()
        {
            Events.OnSceneLoaded.AddListener(LoadData);
            Events.ReloadPathfinder.AddListener(GenerateGrid);
        }
        private void OnDisable()
        {
            Events.OnSceneLoaded.RemoveListener(LoadData);
            Events.ReloadPathfinder.RemoveListener(GenerateGrid);
        }

        private void LoadData(SceneData data)
        {
            nodeRadius = data.grid.cellSize;
            nodeDiameter = nodeRadius * 2;

            gridSizeX = data.grid.dimensions.x;
            gridSizeY = data.grid.dimensions.y;

            gridWorldSize = new Vector2(gridSizeX * nodeRadius, gridSizeY * nodeRadius);
            grid = new Node[gridSizeX, gridSizeY];

            worldBottomLeft = data.grid.position;
        }
        private void GenerateGrid()
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    int index = x * gridSizeY + y;

                    Vector2 worldPoint = new(x * nodeRadius + nodeRadius * 0.5f + worldBottomLeft.x, y * nodeRadius + nodeRadius * 0.5f + worldBottomLeft.y);

                    grid[x, y] = new Node(worldPoint, x, y);
                }
            }

            foreach (var node in grid)
            {
                node.neighbours = GetNeighbours(node);
            }
        }

        public Node NodeFromWorldPoint(Vector2 worldPos)
        {
            float percentX = (worldPos.x - worldBottomLeft.x) / gridWorldSize.x;
            float percentY = (worldPos.y - worldBottomLeft.y) / gridWorldSize.y;

            // Clamp percentages between 0 - 1
            percentX = Mathf.Clamp01(percentX);
            percentY = Mathf.Clamp01(percentY);

            // Calculate cell position from percentage
            int x = Mathf.FloorToInt(gridSizeX * percentX);
            int y = Mathf.FloorToInt(gridSizeY * percentY);

            // Clamp cell position to grid size
            x = Mathf.Clamp(x, 0, gridSizeX - 1);
            y = Mathf.Clamp(y, 0, gridSizeY - 1);

            return grid[x, y];
        }
        public List<Node> GetNeighbours(Node node)
        {
            List<Node> neighbours = new List<Node>();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int checkX = node.gridX + x;
                    int checkY = node.gridY + y;

                    if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                    {
                        if (!CheckCollision(node, grid[checkX, checkY]))
                        {
                            neighbours.Add(grid[checkX, checkY]);
                        }
                    }
                }
            }

            if (neighbours.Count < 8) fewNeighbours.Add(node.worldPosition);
            return neighbours;
        }
        private bool CheckCollision(Node nodeA, Node nodeB)
        {
            float distance = Vector2.Distance(nodeA.worldPosition, nodeB.worldPosition);
            Vector2 direction = (nodeB.worldPosition - nodeA.worldPosition).normalized;

            Collider2D collision = Physics2D.Raycast(nodeA.worldPosition, direction, distance, unwalkableMask).collider;
            if (collision == null) return false;
            return collision.enabled;
        }

        private void OnDrawGizmos()
        {
            if (fewNeighbours.Count == 0) return;

            Gizmos.color = Color.red;

            foreach (var point in fewNeighbours)
            {
                Vector3 cubeSize = new Vector3(nodeRadius * 0.5f, nodeRadius * 0.5f, nodeRadius * 0.5f);
                Gizmos.DrawCube(point, cubeSize);
            }
        }
    }
}