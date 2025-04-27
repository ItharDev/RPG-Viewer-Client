using System;
using UnityEngine;

namespace RPG
{
    [Serializable]
    public struct EffectData
    {
        public string id;
        public string name;
        public string image;
        public float radius;
        public bool overTokens;
        public Color color;
        public EffectAnimation animation;

        public EffectData(string _id, string _name, string _image, float _radius, bool _overTokens, Color _color, EffectAnimation _animation)
        {
            id = _id;
            name = _name;
            image = _image;
            radius = _radius;
            overTokens = _overTokens;
            color = _color;
            animation = _animation;
        }
    }

    public enum EffectAnimationType
    {
        None = 0,
        Pulse = 1,
        Rotate = 2,
        Both = 3
    }

    [Serializable]
    public struct EffectAnimation
    {
        public EffectAnimationType type;
        public float pulseFrequency;
        public float pulseStrength;
        public float rotationSpeed;

        public EffectAnimation(EffectAnimationType _type, float _pulseFrequency, float _pulseStrength, float _rotationSpeed)
        {
            type = _type;
            pulseFrequency = _pulseFrequency;
            pulseStrength = _pulseStrength;
            rotationSpeed = _rotationSpeed;
        }
    }
}