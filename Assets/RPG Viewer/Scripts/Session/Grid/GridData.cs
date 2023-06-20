using System;
using UnityEngine;

namespace RPG
{
    [Serializable]
    public struct GridData
    {
        public bool snapToGrid;
        public Vector2Int dimensions;
        public float cellSize;
        public Vector2 position;
        public Color color;

        public GridData(bool _snapToGrid, Vector2Int _dimensions, float _cellSize, Vector2 _position, Color _color)
        {
            snapToGrid = _snapToGrid;
            dimensions = _dimensions;
            cellSize = _cellSize;
            position = _position;
            color = _color;
        }
    }
    
    [Serializable]
    public struct Cell
    {
        public Vector2 worldPosition;
        public Vector2Int gridPosition;

        public Cell(Vector2 _worldPosition, Vector2Int _gridPosition)
        {
            worldPosition = _worldPosition;
            gridPosition = _gridPosition;
        }
    }
}