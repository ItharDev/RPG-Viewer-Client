using System;
using UnityEngine;

namespace RPG
{
    [CreateAssetMenu(fileName = "New Condition", menuName = "Condition")]
    public class Condition : ScriptableObject
    {
        public Sprite icon;
        public ConditionFlag flag;
    }
}

[Flags]
public enum ConditionFlag
{
    Blinded = 1,
    Bloodied = 2,
    Charmed = 4,
    Confused = 8,
    Dead = 16,
    Frightened = 32,
    Grappled = 64,
    Hasted = 128,
    Hex = 256,
    Incapacitated = 512,
    Invisible = 1024,
    Mirror_Image = 2048,
    Paralyzed = 4096,
    Petrified = 8192,
    Poisoned = 16384,
    Prone = 32768,
    Raging = 65536,
    Restrained = 131072,
    Stunned = 262144,
    Unconscious = 524288,
}