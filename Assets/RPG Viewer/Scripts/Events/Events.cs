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
        public static UnityEvent<SceneData> OnSceneLoaded = new UnityEvent<SceneData>();
        public static UnityEvent<string> OnLandingPageChanged = new UnityEvent<string>();

        // Side panel (Scenes)
        public static UnityEvent<SceneFolder> OnSceneFolderClicked = new UnityEvent<SceneFolder>();
        public static UnityEvent<SceneFolder> OnSceneFolderSelected = new UnityEvent<SceneFolder>();
        public static UnityEvent OnSceneFolderDeselected = new UnityEvent();
        public static UnityEvent OnSceneFolderMoved = new UnityEvent();
        public static UnityEvent<SceneHolder> OnSceneClicked = new UnityEvent<SceneHolder>();
        public static UnityEvent<SceneHolder> OnSceneSelected = new UnityEvent<SceneHolder>();
        public static UnityEvent OnSceneDeselected = new UnityEvent();
        public static UnityEvent OnSceneMoved = new UnityEvent();

        // Side panel (Blueprints)
        public static UnityEvent<TokenFolder> OnBlueprintFolderClicked = new UnityEvent<TokenFolder>();
        public static UnityEvent<TokenFolder> OnBlueprintFolderSelected = new UnityEvent<TokenFolder>();
        public static UnityEvent OnBlueprintFolderDeselected = new UnityEvent();
        public static UnityEvent OnBlueprintFolderMoved = new UnityEvent();
        public static UnityEvent<TokenHolder> OnBlueprintClicked = new UnityEvent<TokenHolder>();
        public static UnityEvent<TokenHolder> OnBlueprintSelected = new UnityEvent<TokenHolder>();
        public static UnityEvent OnBlueprintDeselected = new UnityEvent();
        public static UnityEvent OnBlueprintMoved = new UnityEvent();

        // Lights
        public static UnityEvent<KeyValuePair<string, LightData>, PresetData> OnLightCreated = new UnityEvent<KeyValuePair<string, LightData>, PresetData>();
        public static UnityEvent<string, LightData, PresetData> OnLightModified = new UnityEvent<string, LightData, PresetData>();
        public static UnityEvent<string, LightData> OnLightMoved = new UnityEvent<string, LightData>();
        public static UnityEvent<string, bool> OnLightToggled = new UnityEvent<string, bool>();
        public static UnityEvent<string> OnLightRemoved = new UnityEvent<string>();
        public static UnityEvent<LightingSettings, bool> OnLightingChanged = new UnityEvent<LightingSettings, bool>();

        // Grid
        public static UnityEvent<GridData, bool, bool> OnGridChanged = new UnityEvent<GridData, bool, bool>();

        // Tools
        public static UnityEvent<Tool> OnToolChanged = new UnityEvent<Tool>();
        public static UnityEvent<Setting> OnSettingChanged = new UnityEvent<Setting>();
        public static UnityEvent<GameView> OnViewChanged = new UnityEvent<GameView>();

        // Walls
        public static UnityEvent<WallData, bool> OnWallCreated = new UnityEvent<WallData, bool>();
        public static UnityEvent<string, WallData> OnWallModified = new UnityEvent<string, WallData>();
        public static UnityEvent<string> OnWallRemoved = new UnityEvent<string>();
        public static UnityEvent<PointController> OnPointContinued = new UnityEvent<PointController>();
        public static UnityEvent<PointController> OnPointDragged = new UnityEvent<PointController>();
        public static UnityEvent<PointController> OnPointClicked = new UnityEvent<PointController>();
        public static UnityEvent<PointController> OnPointDeleted = new UnityEvent<PointController>();
        public static UnityEvent<PointController> OnPointHovered = new UnityEvent<PointController>();
        public static UnityEvent<LineController> OnLineHovered = new UnityEvent<LineController>();

        // Presets
        public static UnityEvent<string, PresetData> OnPresetCreated = new UnityEvent<string, PresetData>();
        public static UnityEvent<string, PresetData> OnPresetModified = new UnityEvent<string, PresetData>();
        public static UnityEvent<string, PresetData> OnPresetRemoved = new UnityEvent<string, PresetData>();

        // Tokens
        public static UnityEvent<TokenData> OnTokenCreated = new UnityEvent<TokenData>();
        public static UnityEvent<string, MovementData> OnTokenMoved = new UnityEvent<string, MovementData>();
        public static UnityEvent<string, TokenData> OnTokenModified = new UnityEvent<string, TokenData>();
        public static UnityEvent<string> OnTokenRemoved = new UnityEvent<string>();
        public static UnityEvent<string, bool> OnTokenEnabled = new UnityEvent<string, bool>();
        public static UnityEvent<string, bool> OnTokenLocked = new UnityEvent<string, bool>();
        public static UnityEvent<string, int> OnConditionsModified = new UnityEvent<string, int>();
        public static UnityEvent<string, int> OnHealthModified = new UnityEvent<string, int>();
        public static UnityEvent<string, int> OnElevationModified = new UnityEvent<string, int>();
        public static UnityEvent<string, float> OnTokenRotated = new UnityEvent<string, float>();
        public static UnityEvent<Token> OnTokenSelected = new UnityEvent<Token>();

        // Initiatives
        public static UnityEvent<InitiativeData> OnInitiativeCreated = new UnityEvent<InitiativeData>();
        public static UnityEvent<string, InitiativeData> OnInitiativeModified = new UnityEvent<string, InitiativeData>();
        public static UnityEvent<string> OnInitiativeRemoved = new UnityEvent<string>();
        public static UnityEvent OnInitiativeSorted = new UnityEvent();

        // Notes
        public static UnityEvent<NoteInfo, NoteData> OnNoteCreated = new UnityEvent<NoteInfo, NoteData>();
        public static UnityEvent<string, Vector2> OnNoteMoved = new UnityEvent<string, Vector2>();
        public static UnityEvent<string, string, string> OnNoteTextModified = new UnityEvent<string, string, string>();
        public static UnityEvent<string, string, string> OnNoteHeaderModified = new UnityEvent<string, string, string>();
        public static UnityEvent<string, string> OnNoteImageModified = new UnityEvent<string, string>();
        public static UnityEvent<string, bool> OnNoteSetToGlobal = new UnityEvent<string, bool>();
        public static UnityEvent<string> OnNoteRemoved = new UnityEvent<string>();
        public static UnityEvent<string> OnNoteShowed = new UnityEvent<string>();

        // ping
        public static UnityEvent<string, Vector2> OnPointerStarted = new UnityEvent<string, Vector2>();
        public static UnityEvent<string, Vector2> OnPointerUpdated = new UnityEvent<string, Vector2>();
        public static UnityEvent<string> OnPointerStopped = new UnityEvent<string>();
        public static UnityEvent<Vector2, bool> OnPing = new UnityEvent<Vector2, bool>();

        // Pathfinder
        public static UnityEvent ReloadPathfinder = new UnityEvent();
    }
}