using Networking;
using UnityEngine;

namespace RPG
{
    public class LightTools : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private PresetData defaultConfig;

        public static LightTools Instance { get; private set; }
        public bool MouseOver;

        private bool interactable;
        public LightMode Mode;

        private void Awake()
        {
            Instance = this;
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnSettingChanged.AddListener(ToggleUI);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSettingChanged.RemoveListener(ToggleUI);
        }
        private void Update()
        {
            if (!interactable || Mode == LightMode.Delete) return;
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonUp(0) && !MouseOver) CreateLight();
        }

        private void ToggleUI(Setting setting)
        {
            bool enabled = setting.ToString().ToLower().Contains("lighting");
            canvasGroup.alpha = enabled ? 1.0f : 0.0f;
            canvasGroup.blocksRaycasts = enabled;
            interactable = enabled;

            switch (setting)
            {
                case Setting.Lighting_Create:
                    Mode = LightMode.Create;
                    break;
                case Setting.Lighting_Delete:
                    Mode = LightMode.Delete;
                    break;
            }
        }

        private void CreateLight()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            PresetData data = defaultConfig;
            LightData info = new LightData("", mousePos, 0.0f, false);

            SocketManager.EmitAsync("create-light", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(data), JsonUtility.ToJson(info));
        }
    }

    public enum LightMode
    {
        Create,
        Delete
    }
}