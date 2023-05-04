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

        // TODO: public LightingManager LightingManager { get; private set; }
        public TokenManager TokenManager { get; private set; }
        // TODO: public WallManager WallManager { get; private set; }
        public Grid Grid { get; private set; }
        public SceneSettings Settings { get; private set; }
        public static Session Instance { get; private set; }

        private void OnEnable()
        {
            // Add event listeners
            Events.OnStateChanged.AddListener(ChangeState);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnStateChanged.RemoveListener(ChangeState);
        }
        private void Awake()
        {
            // Create instance
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Get reference of managers
            // TODO: LightingManager = GetComponent<LightingManager>();
            TokenManager = GetComponent<TokenManager>();
            // TODO: WallManager = GetComponent<WallManager>();
            Grid = FindObjectOfType<Grid>();
        }
        private void Start()
        {
            // Update landing page
            landingPage.sprite = SessionManager.Info.background;
        }

        private void ChangeState(SessionState oldState, SessionState newState)
        {
            // Check if we are the master client
            if (SessionManager.Info.isMaster)
            {
                // Activate or deactivate landing page based on state
                landingPage.gameObject.SetActive(!string.IsNullOrEmpty(newState.scene));
                return;
            }
            else if (!newState.synced)
            {
                // Return if syncing is disabled
                landingPage.gameObject.SetActive(true);
                return;
            }

            if (!string.IsNullOrEmpty(newState.scene)) LoadScene(newState.scene);
        }

        private void LoadScene(string id)
        {
            SocketManager.EmitAsync("get-scene", (callback) =>
            {
                if (callback.GetValue().GetBoolean())
                {
                    // Load scene data
                    SceneSettings settings = JsonUtility.FromJson<SceneSettings>(callback.GetValue(1).ToString());
                    settings.id = id;

                    MessageManager.QueueMessage("Loading scene");
                    Settings = settings;

                    // Send load event
                    Events.OnSceneLoaded?.Invoke(settings);
                    WebManager.Download(settings.data.image, true, async (bytes) =>
                    {
                        await UniTask.SwitchToMainThread();

                        // Generate and apply texture
                        Texture2D texture = new Texture2D(1, 1);
                        texture.LoadImage(bytes);
                        sceneSprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                        // Remove message when loading is completed
                        MessageManager.RemoveMessage("Loading scene");
                    });

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
    }
}