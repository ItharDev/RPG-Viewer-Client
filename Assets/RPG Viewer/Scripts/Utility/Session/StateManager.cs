using UnityEngine;

namespace RPG
{
    public class StateManager : MonoBehaviour
    {
        
    }

    public enum ToolState
    {
        Pan,
        Light,
        Measure,
        Ping,
        Notes
    }
    public enum FogState
    {
        Player,
        Vision,
        NoFog
    }
    public enum LightState
    {
        Create,
        Delete
    }
    public enum NoteState
    {
        Create,
        Delete
    }

}