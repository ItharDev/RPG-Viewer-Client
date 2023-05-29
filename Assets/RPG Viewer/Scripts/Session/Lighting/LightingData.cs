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
        public float radius;
        public bool enabled;
        public Vector2 position;
        public float intensity;
        public float flickerFrequency;
        public float flickerAmount;
        public float pulseInterval;
        public float pulseAmount;
        public int effect;
        public Color color;
        public string preset;
    }
}