using UnityEngine;

namespace RPG
{
    public class GridUI : MonoBehaviour
    {
        [SerializeField] private Color hover;
        [SerializeField] private Color measureStart;
        [SerializeField] private Color measureEnd;

        [Space]
        [SerializeField] private bool draw;

        private GridManager grid;

        private void Awake()
        {
            if (grid == null) grid = GetComponent<GridManager>();
        }

        private void OnDrawGizmos()
        {
            // Return if we are not in play mode
            if (!Application.isPlaying) return;

            // Return if drawin is disabled or grid has not been generated
            if (!draw || grid.Grid == null) return;

            DrawHover();
        }

        private void DrawHover()
        {
            Gizmos.color = hover;
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 worldPos = grid.WorldPosToCell(mousePos).worldPosition;
            Vector3 cubeSize = new Vector3(grid.CellSize, grid.CellSize, grid.CellSize);
            Gizmos.DrawCube(worldPos, cubeSize);
        }
    }
}