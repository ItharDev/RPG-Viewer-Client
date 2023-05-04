using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RPG
{
    public static class Events
    {
        // Connection
        public static UnityEvent OnConnected = new UnityEvent();
        public static UnityEvent OnDisconnected = new UnityEvent();

        // Accounts
        public static UnityEvent<string, string> OnSignIn = new UnityEvent<string, string>();
        public static UnityEvent OnSignOut = new UnityEvent();
        public static UnityEvent OnRegister = new UnityEvent();

        // Scene state
        public static UnityEvent<string> OnUserConnected = new UnityEvent<string>();
        public static UnityEvent<string> OnUserDisconnected = new UnityEvent<string>();
        public static UnityEvent<SessionState, SessionState> OnStateChanged = new UnityEvent<SessionState, SessionState>();
        public static UnityEvent<SceneSettings> OnSceneLoaded = new UnityEvent<SceneSettings>();

        // Doors
        public static UnityEvent<string, bool> OnDoorOpened = new UnityEvent<string, bool>();
        public static UnityEvent<string, WallData> OnDoorModified = new UnityEvent<string, WallData>();

        // Lights
        // public static UnityEvent<LightData> OnLightCreated = new UnityEvent<LightData>();
        // public static UnityEvent<string, LightData> OnLightModified = new UnityEvent<string, LightData>();
        public static UnityEvent<string> OnLightRemoved = new UnityEvent<string>();
        public static UnityEvent<FogState> OnFogStateChanged = new UnityEvent<FogState>();
        public static UnityEvent<ToolState> OnToolChanged = new UnityEvent<ToolState>();

        // Presets
        // public static UnityEvent<string, LightPreset> OnPresetCreated = new UnityEvent<string, LightPreset>();
        // public static UnityEvent<string, LightPreset> OnPresetModified = new UnityEvent<string, LightPreset>();
        public static UnityEvent<string> OnPresetRemoved = new UnityEvent<string>();

        // Tokens
        public static UnityEvent<TokenData> OnTokenCreated = new UnityEvent<TokenData>();
        public static UnityEvent<string, MovementData> OnTokenMoved = new UnityEvent<string, MovementData>();
        public static UnityEvent<string, TokenData> OnTokenModified = new UnityEvent<string, TokenData>();
        public static UnityEvent<string> OnTokenRemoved = new UnityEvent<string>();
        public static UnityEvent<string, bool> OnTokenEnabled = new UnityEvent<string, bool>();
        public static UnityEvent<string, bool> OnTokenLocked = new UnityEvent<string, bool>();
        public static UnityEvent<string, int> OnConditionsModified = new UnityEvent<string, int>();
        public static UnityEvent<string, int> OnHealthModified = new UnityEvent<string, int>();
        public static UnityEvent<string, string> OnElevationModified = new UnityEvent<string, string>();
        public static UnityEvent<string, float> OnTokenRotated = new UnityEvent<string, float>();
        public static UnityEvent OnTokenSelected = new UnityEvent();

        // Notes
        // public static UnityEvent<NoteData> OnNoteCreated = new UnityEvent<NoteData>();
        public static UnityEvent<string, string> OnNoteTextModified = new UnityEvent<string, string>();
        public static UnityEvent<string, string> OnNoteImageModified = new UnityEvent<string, string>();
        public static UnityEvent<string, string> OnNoteHeaderModified = new UnityEvent<string, string>();
        public static UnityEvent<string, bool> OnNoteEnabled = new UnityEvent<string, bool>();
        public static UnityEvent<string, Vector2> OnNoteMoved = new UnityEvent<string, Vector2>();
        public static UnityEvent<string> OnNoteRemoved = new UnityEvent<string>();
        public static UnityEvent<string> OnNoteShown = new UnityEvent<string>();

        // Journals
        public static UnityEvent<string, string> OnJournalTextModified = new UnityEvent<string, string>();
        public static UnityEvent<string, string> OnJournalImageModified = new UnityEvent<string, string>();
        public static UnityEvent<string, string> OnJournalHeaderModified = new UnityEvent<string, string>();
        // public static UnityEvent<string, List<Collaborator>> OnCollaboratorsModified = new UnityEvent<string, List<Collaborator>>();
        public static UnityEvent<string> OnJournalRemoved = new UnityEvent<string>();
        // public static UnityEvent<JournalData> OnJournalShown = new UnityEvent<JournalData>();
    }
}