using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class Session : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer sceneSprite;
        [SerializeField] private Image landingPage;

        public LightingManager LightingManager { get; private set; }
        public TokenManager TokenManager { get; private set; }
        public GridManager Grid { get; private set; }
        public SceneData Settings { get; private set; }
        public static Session Instance { get; private set; }

        private void OnEnable()
        {
            // Add event listeners
            Events.OnStateChanged.AddListener(ChangeState);
            Events.OnLandingPageChanged.AddListener(UpdateLandingPage);
            Events.OnGridChanged.AddListener(UpdateGrid);
            Events.OnLightingChanged.AddListener(UpdateLighting);
            Events.OnUserConnected.AddListener(ConnectUser);
            Events.OnUserDisconnected.AddListener(DisconnectUser);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnStateChanged.RemoveListener(ChangeState);
            Events.OnLandingPageChanged.RemoveListener(UpdateLandingPage);
            Events.OnGridChanged.RemoveListener(UpdateGrid);
            Events.OnLightingChanged.RemoveListener(UpdateLighting);
            Events.OnUserConnected.AddListener(ConnectUser);
            Events.OnUserDisconnected.AddListener(DisconnectUser);
        }
        private void Awake()
        {
            // Create instance
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Get reference of the managers
            LightingManager = FindObjectOfType<LightingManager>();
            TokenManager = FindObjectOfType<TokenManager>();
            Grid = FindObjectOfType<GridManager>();
        }
        private void Start()
        {
            // Update landing page
            if (ConnectionManager.Info.background != null) landingPage.sprite = ConnectionManager.Info.background;
        }

        private void ChangeState(SessionState oldState, SessionState newState)
        {
            if (newState.scene == null)
            {
                // Return if there's no scene active
                landingPage.gameObject.SetActive(true);
                return;
            }

            // Check if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                // Activate or deactivate landing page based on state
                landingPage.gameObject.SetActive(string.IsNullOrEmpty(newState.scene));

                // Return if no scene was changed
                if (oldState.scene == newState.scene) return;

                // Load new scene
                if (!string.IsNullOrEmpty(newState.scene)) LoadScene(newState.scene);
            }
            else
            {
                // Return if syncing is disabled
                if (!newState.synced)
                {
                    landingPage.gameObject.SetActive(true);
                    return;
                }

                // Load new scene
                if (!string.IsNullOrEmpty(newState.scene)) LoadScene(newState.scene);
            }
        }
        private void UpdateGrid(GridData gridData, bool reloadRequired, bool globalUpdate)
        {
            if (globalUpdate) Settings.grid = gridData;
        }
        private void UpdateLighting(LightingSettings lightingData, bool globalUpdate)
        {
            if (globalUpdate) Settings.darkness = lightingData;
        }
        private void UpdateLandingPage(string id)
        {
            MessageManager.QueueMessage("Loading new landing page");

            WebManager.Download(id, true, async (bytes) =>
            {
                // Check if landing page exists
                if (bytes == null)
                {
                    MessageManager.QueueMessage("Failed to load landing page, please try again");
                    return;
                }

                await UniTask.SwitchToMainThread();

                // Create landing page texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                ConnectionManager.Info.background = sprite;
                landingPage.sprite = sprite;
                MessageManager.RemoveMessage("Loading new landing page");
            });
        }

        private void LoadScene(string id)
        {
            landingPage.gameObject.SetActive(true);
            SocketManager.EmitAsync("get-scene", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Load scene data
                    SceneData settings = JsonUtility.FromJson<SceneData>(callback.GetValue(1).ToString());
                    settings.id = id;

                    // Load lights
                    settings.lights = new Dictionary<string, LightData>();
                    System.Text.Json.JsonElement lights;
                    if (callback.GetValue(1).TryGetProperty("lights", out lights)) settings.lights = GetLights(callback.GetValue(1).GetProperty("lights").EnumerateObject().ToArray());

                    // Load notes
                    settings.notes = new Dictionary<string, NoteInfo>();
                    System.Text.Json.JsonElement notes;
                    if (callback.GetValue(1).TryGetProperty("notes", out notes)) settings.notes = GetNotes(callback.GetValue(1).GetProperty("notes").EnumerateObject().ToArray());

                    MessageManager.QueueMessage("Loading scene");
                    Settings = settings;

                    // Send load event
                    WebManager.Download(settings.info.image, true, async (bytes) =>
                    {
                        await UniTask.SwitchToMainThread();

                        // Generate and apply texture
                        Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                        sceneSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                        Events.OnSceneLoaded?.Invoke(settings);

                        // Remove message when loading is completed
                        MessageManager.RemoveMessage("Loading scene");

                        landingPage.gameObject.SetActive(false);
                    });

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }

        private void ConnectUser(string user)
        {
            MessageManager.QueueMessage($"{user} connected");
        }
        private void DisconnectUser(string user)
        {
            MessageManager.QueueMessage($"{user} disconnected");
        }

        private Dictionary<string, LightData> GetLights(System.Text.Json.JsonProperty[] lights)
        {
            Dictionary<string, LightData> dictionary = new Dictionary<string, LightData>();
            for (int i = 0; i < lights.Length; i++)
            {
                dictionary.Add(lights[i].Name, JsonUtility.FromJson<LightData>(lights[i].Value.ToString()));
            }

            return dictionary;
        }
        private Dictionary<string, NoteInfo> GetNotes(System.Text.Json.JsonProperty[] notes)
        {
            Dictionary<string, NoteInfo> dictionary = new Dictionary<string, NoteInfo>();
            for (int i = 0; i < notes.Length; i++)
            {
                dictionary.Add(notes[i].Name, JsonUtility.FromJson<NoteInfo>(notes[i].Value.ToString()));
            }

            return dictionary;
        }
    }
}