using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG
{
    public class SceneSettings
    {
        public SceneData data;
        public GridData grid;
        public FogOfWarData fogOfWar;
        public List<WallData> walls;
        public List<string> tokens;
        public List<InitiativeData> initiatives;
        // TODO: public List<LightData> lights;
        // TODO: public List<NoteData> notes;

        [NonSerialized] public string path;
        [NonSerialized] public string id;
        [NonSerialized] public byte[] bytes;
    }

    [Serializable]
    public struct SceneData
    {
        public string name;
        public string image;
        public float nightStrength;

        public SceneData(string _name, string _image, float _nightStrength)
        {
            name = _name;
            image = _image;
            nightStrength = _nightStrength;
        }
    }

    [Serializable]
    public struct FogOfWarData
    {
        public bool enabled;
        public bool globalLighting;
        public Color color;
        public float translucency;
        public float nightVisionStrength;

        public FogOfWarData(bool _enabled, bool _globalLighting, Color _color, float _translucency, float _nightVisionStrength)
        {
            enabled = _enabled;
            globalLighting = _globalLighting;
            color = _color;
            translucency = _translucency;
            nightVisionStrength = _nightVisionStrength;
        }
    }

    [Serializable]
    public struct GridData
    {
        public bool enabled;
        public bool snapToGrid;
        public Vector2Int dimensions;
        public float cellSize;
        public Vector2 position;
        public Color color;

        public GridData(bool _enabled, bool _snapToGrid, Vector2Int _dimensions, float _cellSize, Vector2 _position, Color _color)
        {
            enabled = _enabled;
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

    [Serializable]
    public struct WallData
    {
        public string id;
        public List<Vector2> points;
        public WallType model;
        public bool open;
        public bool locked;

        public WallData(string _id, List<Vector2> _points, WallType _model, bool _open, bool _locked)
        {
            id = _id;
            points = _points;
            model = _model;
            open = _open;
            locked = _locked;
        }
    }
    [Serializable]
    public enum WallType
    {
        Wall,
        Door,
        Invisible,
        Hidden_Door,
        Environmental
    }

    [Serializable]
    public struct InitiativeData
    {
        public int index;
        public string name;
        public string roll;
        public bool visible;

        public InitiativeData(int _index, string _name, string _roll, bool _visible)
        {
            index = _index;
            name = _name;
            roll = _roll;
            visible = _visible;
        }
    }
}