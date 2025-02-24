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
        public List<PortalData> portals;
        public List<string> tokens;
        public TokenGroup groupOne;
        public TokenGroup groupTwo;
        public TokenGroup groupThree;

        [NonSerialized] public string path;
        [NonSerialized] public string id;
        [NonSerialized] public Dictionary<string, LightData> lights;
        [NonSerialized] public Dictionary<string, NoteInfo> notes;

        public SceneData(SceneInfo _info, GridData _grid, LightingSettings _darkness)
        {
            info = _info;
            grid = _grid;
            darkness = _darkness;
            walls = new List<WallData>();
            portals = new List<PortalData>();
            tokens = new List<string>();
            groupOne = new TokenGroup(false, new List<string>());
            groupTwo = new TokenGroup(false, new List<string>());
            groupThree = new TokenGroup(false, new List<string>());
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
        Environment,
        Darkness
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

    [Serializable]
    public struct TokenGroup
    {
        public bool selected;
        public List<string> tokens;

        public TokenGroup(bool _selected, List<string> _tokens)
        {
            selected = _selected;
            tokens = _tokens;
        }
    }
}