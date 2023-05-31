using System;
using UnityEngine;

namespace RPG
{
    [Serializable]
    public struct LightingSettings
    {
        public bool enabled;
        public bool globalLighting;
        public Color color;
        public float translucency;

        public LightingSettings(bool _enabled, bool _globalLighting, Color _color, float _translucency)
        {
            enabled = _enabled;
            globalLighting = _globalLighting;
            color = _color;
            translucency = _translucency;
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
    public struct LightData
    {
        public string id;
        public string name;
        public float radius;
        public float intensity;
        public bool enabled;
        public Color color;
        public Vector2 position;
        public LightEffect effect;

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

        public static bool operator ==(LightData data_1, LightData data_2)
        {
            return data_1.Equals(data_2);
        }
        public static bool operator !=(LightData data_1, LightData data_2)
        {
            return !data_1.Equals(data_2);
        }
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
            return data_1.Equals(data_2);

        }
        public static bool operator !=(LightEffect data_1, LightEffect data_2)
        {
            return !data_1.Equals(data_2);

        }
    }
}