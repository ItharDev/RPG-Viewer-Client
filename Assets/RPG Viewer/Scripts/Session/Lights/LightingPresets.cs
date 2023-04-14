using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPG
{
    public static class LightingPresets
    {
        public static Dictionary<string, LightPreset> Presets = new Dictionary<string, LightPreset>();
        public static Dictionary<object, string> PresetActors = new Dictionary<object, string>();

        public static void AddPreset(string id, LightPreset preset)
        {
            preset.id = id;
            Presets.Add(id, preset);
        }
        public static void ModifyPreset(string id, LightPreset preset)
        {
            Presets[id] = preset;
            var actors = PresetActors.Keys.ToArray();

            for (int i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                if (actor.GetType() == typeof(Token))
                {
                    (actor as Token).LoadLights();
                }
                else if (actor.GetType() == typeof(LightHolder))
                {
                    (actor as LightHolder).UpdatePreset(preset);
                }
            }
        }
        public static void RemovePreset(string id)
        {
            Presets.Remove(id);
        }

        public static void MoveActor(object obj, string id)
        {
            if (!PresetActors.ContainsKey(obj)) PresetActors.Add(obj, id);
            else PresetActors[obj] = id;
        }
        public static void RemoveActor(object obj)
        {
            PresetActors.Remove(obj);
        }
    }

    [Serializable]
    public struct LightPreset
    {
        public string id;
        public string name;
        public float radius;
        public Color color;
        public float intensity;
        public float flickerFrequency;
        public float flickerAmount;
        public float pulseInterval;
        public float pulseAmount;
        public LightEffect effect;
    }
}