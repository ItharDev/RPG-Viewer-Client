using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG
{
    public class GridVisualisation : MonoBehaviour
    {
        [SerializeField] private GameObject cornerPrefab;
        [SerializeField] private Transform cornerParent;
        private readonly List<GameObject> corners = new();

        private SessionGrid grid;
        private int corner = -1;

        public float CellSize;
        public Vector2Int Dimensions;

        private void Awake()
        {
            if (grid == null) grid = GetComponent<SessionGrid>();
        }
        private void Update()
        {
            CheckDrag();
        }

        private void CheckDrag()
        {
            if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            {
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.LeftAlt)) return;
                if (corners.Count != 4) return;

                Vector2 dist = new(Input.GetAxis("Horizontal") * 0.01f, Input.GetAxis("Vertical") * 0.01f);
                foreach (var corner in corners)
                {
                    corner.transform.localPosition += new Vector3(dist.x, dist.y);
                }

                grid.GenerateGrid(Dimensions, corners[2].transform.localPosition, CellSize);
            }

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos = new Vector3(mousePos.x, mousePos.y, -1);

                for (int i = 0; i < corners.Count; i++)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(corners[i].GetComponent<RectTransform>(), mousePos)) corner = i;
                }
            }
            if (Input.GetMouseButtonUp(0)) corner = -1;
            if (Input.GetMouseButton(0) && corners.Count == 4 && corner != -1) DragCorner(corner);
        }
        private void DragCorner(int corner)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos = new Vector3(mousePos.x, mousePos.y, -1);

            if (corner == 0)
            {
                corners[0].transform.position = new Vector3(mousePos.x, corners[0].transform.position.y, -1);

                CellSize = Math.Abs(corners[0].transform.position.x - corners[1].transform.position.x) / Dimensions.x;

                corners[0].transform.position = new Vector3(corners[0].transform.position.x, corners[2].transform.position.y + (CellSize * Dimensions.y), -1);

                corners[2].transform.position = new Vector3(corners[0].transform.position.x, corners[2].transform.position.y, -1);
                corners[1].transform.position = new Vector3(corners[1].transform.position.x, corners[0].transform.position.y, -1);
            }
            else if (corner == 1)
            {
                corners[1].transform.position = new Vector3(mousePos.x, corners[1].transform.position.y, -1);

                CellSize = Math.Abs(corners[0].transform.position.x - corners[1].transform.position.x) / Dimensions.x;

                corners[1].transform.position = new Vector3(corners[1].transform.position.x, corners[3].transform.position.y + (CellSize * Dimensions.y), -1);

                corners[3].transform.position = new Vector3(corners[1].transform.position.x, corners[3].transform.position.y, -1);
                corners[0].transform.position = new Vector3(corners[0].transform.position.x, corners[1].transform.position.y, -1);
            }
            else if (corner == 2)
            {
                corners[2].transform.position = new Vector3(mousePos.x, corners[2].transform.position.y, -1);

                CellSize = Math.Abs(corners[0].transform.position.x - corners[1].transform.position.x) / Dimensions.x;

                corners[2].transform.position = new Vector3(corners[2].transform.position.x, corners[1].transform.position.y - (CellSize * Dimensions.y), -1);

                corners[3].transform.position = new Vector3(corners[3].transform.position.x, corners[2].transform.position.y, -1);
                corners[0].transform.position = new Vector3(corners[2].transform.position.x, corners[0].transform.position.y, -1);
            }
            else if (corner == 3)
            {
                corners[3].transform.position = new Vector3(mousePos.x, corners[3].transform.position.y, -1);

                CellSize = Math.Abs(corners[0].transform.position.x - corners[1].transform.position.x) / Dimensions.x;

                corners[3].transform.position = new Vector3(corners[3].transform.position.x, corners[1].transform.position.y - (CellSize * Dimensions.y), -1);

                corners[2].transform.position = new Vector3(corners[2].transform.position.x, corners[3].transform.position.y, -1);
                corners[1].transform.position = new Vector3(corners[3].transform.position.x, corners[1].transform.position.y, -1);
            }
            grid.GenerateGrid(Dimensions, corners[2].transform.localPosition, CellSize);
        }

        public void GenerateGrid(GridData data)
        {
            foreach (var corner in corners) Destroy(corner);
            corners.Clear();
            for (int i = 0; i < 4; i++) corners.Add(Instantiate(cornerPrefab, cornerParent));

            corners[2].transform.localPosition = data.position;
            corners[0].transform.localPosition = new Vector3(corners[2].transform.localPosition.x, corners[2].transform.localPosition.y + data.cellSize * data.dimensions.y, -1);
            corners[1].transform.localPosition = new Vector3(corners[0].transform.localPosition.x + data.dimensions.x * data.cellSize, corners[0].transform.localPosition.y, -1);
            corners[3].transform.localPosition = new Vector3(corners[2].transform.localPosition.x + data.dimensions.x * data.cellSize, corners[2].transform.localPosition.y, -1);

            Dimensions = data.dimensions;
            CellSize = data.cellSize;
        }
        public void ResetGrid(Vector2 point, Vector2Int dimensions, float cellSize)
        {
            foreach (var corner in corners) Destroy(corner);
            corners.Clear();
            for (int i = 0; i < 4; i++) corners.Add(Instantiate(cornerPrefab, cornerParent));


            corners[0].transform.localPosition = new Vector3(point.x, point.y, -1);
            corners[1].transform.localPosition = new Vector3(point.x + dimensions.x * cellSize, point.y, -1); ;
            corners[2].transform.localPosition = new Vector3(point.x, point.y - dimensions.y * cellSize, -1);
            corners[3].transform.localPosition = new Vector3(point.x + dimensions.x * cellSize, point.y - dimensions.y * cellSize, -1);
        }
        public void UpdateCorners(Vector2Int dimensions, float cellSize, ref Vector2 position)
        {
            corners[1].transform.localPosition = new Vector3(corners[0].transform.localPosition.x + dimensions.x * cellSize, corners[0].transform.localPosition.y, -1);
            corners[2].transform.localPosition = new Vector3(corners[0].transform.localPosition.x, corners[0].transform.localPosition.y - dimensions.y * cellSize, -1);
            corners[3].transform.localPosition = new Vector3(corners[1].transform.localPosition.x, corners[2].transform.localPosition.y, -1);

            Dimensions = dimensions;
            position = corners[2].transform.localPosition;
        }
        public void ResetCorners(Texture2D texture, Vector2Int dimensions, ref Vector2 position, ref float cellSize)
        {
            Vector2 textureDimensions = new Vector2(texture.width * 0.01f, texture.height * 0.01f);
            float cs = textureDimensions.x / dimensions.x;

            foreach (var corner in corners) Destroy(corner);
            corners.Clear();
            for (int i = 0; i < 4; i++) corners.Add(Instantiate(cornerPrefab, cornerParent));


            corners[0].transform.localPosition = new Vector3(-textureDimensions.x * 0.5f, textureDimensions.y * 0.5f, -1);
            corners[1].transform.localPosition = new Vector3(corners[0].transform.localPosition.x + (dimensions.x * cs), corners[0].transform.localPosition.y, -1); ;
            corners[2].transform.localPosition = new Vector3(corners[0].transform.localPosition.x, corners[0].transform.localPosition.y - (dimensions.y * cs), -1);
            corners[3].transform.localPosition = new Vector3(corners[1].transform.localPosition.x, corners[2].transform.localPosition.y, -1);

            position = corners[2].transform.localPosition;
            cellSize = cs;
            CellSize = cs;
            Dimensions = dimensions;
        }
    }
}