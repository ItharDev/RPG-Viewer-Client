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
        public List<LightData> lights;
        public List<NoteData> notes;

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
    }

    [Serializable]
    public struct FogOfWarData
    {
        public bool enabled;
        public bool globalLighting;
        public Color color;
        public float translucency;
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
    }
    [Serializable]
    public struct Cell
    {
        public Vector2 position;
        public Vector2Int cell;

        public Cell(Vector2 _position, Vector2Int _cell)
        {
            position = _position;
            cell = _cell;
        }
    }

    [Serializable]
    public struct WallData
    {
        public string wallId;
        public List<Vector2> points;
        public WallType model;
        public bool open;
        public bool locked;
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
    }
}