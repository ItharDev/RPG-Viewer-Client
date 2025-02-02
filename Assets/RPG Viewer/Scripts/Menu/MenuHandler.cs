using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RPG
{
    public class MenuHandler : MonoBehaviour
    {
        [Header("Status")]
        [SerializeField] private Image statusIcon;
        [SerializeField] private Sprite connected;
        [SerializeField] private Sprite disconnected;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text versionText;

        [Header("Events")]

        public UnityEvent OpenSettings = new UnityEvent();

        public UnityEvent OpenSessions = new UnityEvent();

        public UnityEvent OpenSignIn = new UnityEvent();

        public UnityEvent OpenRegister = new UnityEvent();

        private void OnEnable()
        {
            // Add event listeners
            Events.OnConnected.AddListener(Connected);
            Events.OnDisconnected.AddListener(Disconnected);

            // Set version text
            versionText.text = "v" + Application.version;
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnConnected.RemoveListener(Connected);
            Events.OnDisconnected.RemoveListener(Disconnected);
        }

        private void Connected()
        {
            statusIcon.sprite = connected;
            statusText.text = "Connected";
        }
        private void Disconnected()
        {
            statusIcon.sprite = disconnected;
            statusText.text = "Disconnected";
        }
    }
}