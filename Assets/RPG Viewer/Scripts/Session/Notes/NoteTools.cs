using Networking;
using UnityEngine;

namespace RPG
{
    public class NoteTools : MonoBehaviour
    {
        public static NoteTools Instance { get; private set; }
        public bool MouseOver;

        private bool interactable;
        public NoteMode Mode;

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
            if (!interactable || Mode == NoteMode.Delete) return;
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonUp(0) && !MouseOver) CreateNote();
        }

        private void ToggleUI(Setting setting)
        {
            bool enabled = setting.ToString().ToLower().Contains("notes");
            interactable = enabled;

            switch (setting)
            {
                case Setting.Notes_Create:
                    Mode = NoteMode.Create;
                    break;
                case Setting.Notes_Delete:
                    Mode = NoteMode.Delete;
                    break;
            }
        }

        private void CreateNote()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            NoteData data = new NoteData("", "New note", "", "");
            NoteInfo info = new NoteInfo("", mousePos, "", GameData.User.id, false);

            SocketManager.EmitAsync("create-note", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(data), JsonUtility.ToJson(info));
        }
    }

    public enum NoteMode
    {
        Create,
        Delete
    }
}