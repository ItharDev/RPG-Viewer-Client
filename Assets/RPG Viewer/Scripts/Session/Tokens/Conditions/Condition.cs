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
    Deafened = 32,
    Frightened = 64,
    Grappled = 128,
    Hasted = 256,
    Hex = 512,
    Incapacitated = 1024,
    Invisible = 2048,
    Mirror_Image = 4096,
    Paralyzed = 8192,
    Petrified = 16384,
    Poisoned = 32768,
    Prone = 65536,
    Raging = 131072,
    Restrained = 262144,
    Stunned = 524288,
    Unconscious = 1048576,
}