using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG
{
    public class SceneSettings
    {
        public SceneData data;
        public GridData grid;
        public LightingSettings fogOfWar;
        public List<WallData> walls;
        public List<string> tokens;
        public List<InitiativeData> initiatives;
        public List<LightData> lights;
        // TODO: public List<NoteData> notes;

        [NonSerialized] public string path;
        [NonSerialized] public string id;
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