using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG
{
    public class SceneData
    {
        public SceneInfo info;
        public GridData grid;
        public LightingSettings darkness;
        public List<WallData> walls;
        public List<string> tokens;

        [NonSerialized] public string path;
        [NonSerialized] public string id;
        [NonSerialized] public Dictionary<string, LightData> lights;
        [NonSerialized] public Dictionary<string, NoteInfo> notes;

        public SceneData(SceneInfo _info, GridData _grid, LightingSettings _darkness)
        {
            info = _info;
            grid = _grid;
            darkness = _darkness;
        }
    }

    [Serializable]
    public struct SceneInfo
    {
        public string name;
        public string image;
        public float nightStrength;

        public SceneInfo(string _name, string _image, float _nightStrength)
        {
            name = _name;
            image = _image;
            nightStrength = _nightStrength;
        }
    }

    [Serializable]
    public struct WallData
    {
        public string id;
        public List<Vector2> points;
        public WallType type;
        public bool open;
        public bool locked;

        public WallData(string _id, List<Vector2> _points, WallType _type, bool _open, bool _locked)
        {
            id = _id;
            points = _points;
            type = _type;
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
        Fog,
        Environment
    }

    [Serializable]
    public struct InitiativeData
    {
        public string id;
        public string name;
        public int roll;
        public bool visible;

        public InitiativeData(string _id, string _name, int _roll, bool _visible)
        {
            id = _id;
            name = _name;
            roll = _roll;
            visible = _visible;
        }
    }
}