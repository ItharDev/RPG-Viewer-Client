using System;
using UnityEngine;

namespace RPG
{
    [Serializable]
    public struct LightingSettings
    {
        public bool enabled;
        public Color globalLighting;
        public Color color;
        public float visionRange;

        public LightingSettings(bool _enabled, Color _globalLighting, Color _color, float _visionRange)
        {
            enabled = _enabled;
            globalLighting = _globalLighting;
            color = _color;
            visionRange = _visionRange;
        }
    }

    [Serializable]
    public struct LightData
    {
        public string id;
        public Vector2 position;
        public int rotation;
        public bool enabled;

        public LightData(string _id, Vector2 _position, int _rotation, bool _enabled)
        {
            id = _id;
            position = _position;
            rotation = _rotation;
            enabled = _enabled;
        }
    }

    [Serializable]
    public struct NightVisionData
    {
        public float strength;
        public float radius;

        public NightVisionData(float _strength, float _radius)
        {
            strength = _strength;
            radius = _radius;
        }
    }

    [Serializable]
    public struct PresetData
    {
        public string id;
        public string name;
        public LightValues primary;
        public LightValues secondary;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(PresetData data_1, PresetData data_2)
        {
            return JsonUtility.ToJson(data_1) == JsonUtility.ToJson(data_2);
        }
        public static bool operator !=(PresetData data_1, PresetData data_2)
        {
            return JsonUtility.ToJson(data_1) != JsonUtility.ToJson(data_2);
        }
    }

    [Serializable]
    public struct LightValues
    {
        public float radius;
        public float angle;
        public Color color;
        public LightEffect effect;
    }

    [Serializable]
    public struct LightEffect
    {
        public int type;
        public float strength;
        public float frequency;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        public static bool operator ==(LightEffect data_1, LightEffect data_2)
        {
            return JsonUtility.ToJson(data_1) == JsonUtility.ToJson(data_2);
        }
        public static bool operator !=(LightEffect data_1, LightEffect data_2)
        {
            return JsonUtility.ToJson(data_1) != JsonUtility.ToJson(data_2);
        }
    }
}