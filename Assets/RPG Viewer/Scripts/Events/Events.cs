using UnityEngine;
using UnityEngine.Events;

namespace RPG
{
    public static class Events
    {
        // Connection
        public static UnityEvent OnConnected = new UnityEvent();
        public static UnityEvent OnDisconnected = new UnityEvent();
        public static UnityEvent OnSessionJoined = new UnityEvent();

        // Accounts
        public static UnityEvent<string, string> OnSignIn = new UnityEvent<string, string>();
        public static UnityEvent OnSignOut = new UnityEvent();
        public static UnityEvent OnRegister = new UnityEvent();

        // Scene state
        public static UnityEvent<string> OnUserConnected = new UnityEvent<string>();
        public static UnityEvent<string> OnUserDisconnected = new UnityEvent<string>();
        public static UnityEvent<SessionState, SessionState> OnStateChanged = new UnityEvent<SessionState, SessionState>();
        public static UnityEvent<SceneData> OnSceneLoaded = new UnityEvent<SceneData>();

        // Side panel
        public static UnityEvent<SceneFolder> OnSceneFolderClicked = new UnityEvent<SceneFolder>();
        public static UnityEvent<SceneFolder> OnSceneFolderSelected = new UnityEvent<SceneFolder>();
        public static UnityEvent OnSceneFolderDeselected = new UnityEvent();
        public static UnityEvent OnSceneFolderMoved = new UnityEvent();
        public static UnityEvent<SceneHolder> OnSceneClicked = new UnityEvent<SceneHolder>();
        public static UnityEvent<SceneHolder> OnSceneSelected = new UnityEvent<SceneHolder>();
        public static UnityEvent OnSceneDeselected = new UnityEvent();
        public static UnityEvent OnSceneMoved = new UnityEvent();

        public static UnityEvent<TokenFolder> OnBlueprintFolderClicked = new UnityEvent<TokenFolder>();
        public static UnityEvent<TokenFolder> OnBlueprintFolderSelected = new UnityEvent<TokenFolder>();
        public static UnityEvent OnBlueprintFolderDeselected = new UnityEvent();
        public static UnityEvent OnBlueprintFolderMoved = new UnityEvent();
        public static UnityEvent<TokenHolder> OnBlueprintClicked = new UnityEvent<TokenHolder>();
        public static UnityEvent<TokenHolder> OnBlueprintSelected = new UnityEvent<TokenHolder>();
        public static UnityEvent OnBlueprintDeselected = new UnityEvent();
        public static UnityEvent OnBlueprintMoved = new UnityEvent();

        // Doors
        public static UnityEvent<string, bool> OnDoorOpened = new UnityEvent<string, bool>();
        public static UnityEvent<string, WallData> OnDoorModified = new UnityEvent<string, WallData>();

        // Lights
        public static UnityEvent<LightData> OnLightCreated = new UnityEvent<LightData>();
        public static UnityEvent<string, LightData> OnLightModified = new UnityEvent<string, LightData>();
        public static UnityEvent<string> OnLightRemoved = new UnityEvent<string>();

        // Tools
        public static UnityEvent<Tool> OnToolChanged = new UnityEvent<Tool>();
        public static UnityEvent<Setting> OnSettingChanged = new UnityEvent<Setting>();

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