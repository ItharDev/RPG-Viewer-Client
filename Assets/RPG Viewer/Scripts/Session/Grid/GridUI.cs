using System.Collections.Generic;
using UnityEngine;
using Vectrosity;

namespace RPG
{
    public class GridUI : MonoBehaviour
    {
        [SerializeField] private Color hoveredColor;
        [SerializeField] private bool drawGizmos;
        [SerializeField] private float lineWidth;
        [SerializeField] private List<GridCorner> corners;

        private GridManager grid;

        private void Awake()
        {
            if (grid == null) grid = GetComponent<GridManager>();
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnSceneLoaded.AddListener(DrawGrid);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSceneLoaded.RemoveListener(DrawGrid);
        }

        private void DrawGrid(SceneData data)
        {
            GridData grid = data.grid;
            Vector2 worldSize = new Vector2(grid.dimensions.x * grid.cellSize, grid.dimensions.y * grid.cellSize);

            // Instantiate corners
            corners[0].transform.position = new Vector2(grid.position.x, grid.position.y);
            corners[1].transform.position = new Vector2(grid.position.x, grid.position.y + worldSize.y);
            corners[2].transform.position = new Vector2(grid.position.x + worldSize.x, grid.position.y);
            corners[3].transform.position = new Vector2(grid.position.x + worldSize.x, grid.position.y + worldSize.y);

            // Generate grid
            List<Vector3> points = new List<Vector3>();

            // Lines down X axis
            for (int i = 0; i <= grid.dimensions.x; i++)
            {
                points.Add(grid.position + (new Vector2(i * grid.cellSize, 0)));
                points.Add(grid.position + (new Vector2(i * grid.cellSize, (grid.dimensions.y) * grid.cellSize)));
            }
            // Lines down Y axis
            for (int i = 0; i <= grid.dimensions.y; i++)
            {
                points.Add(grid.position + (new Vector2(0, i * grid.cellSize)));
                points.Add(grid.position + (new Vector2((grid.dimensions.x) * grid.cellSize, i * grid.cellSize)));
            }
            VectorLine line = new VectorLine("Grid UI", points, lineWidth);
            line.Draw3DAuto();
        }
        public void MoveCorner(CornerType type)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, -1);

            switch (type)
            {
                case CornerType.Top_Left:
                    return;
                case CornerType.Top_Right:
                    return;
                case CornerType.Bottom_Left:
                    return;
                case CornerType.Bottom_Right:
                    return;
            }
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