using System;
using TMPro;
using UnityEngine;

namespace RPG
{
    public class ConfirmationPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text question;
        [SerializeField] private TMP_Text accept;
        [SerializeField] private TMP_Text cancel;
        private Confirmation confirmation;

        private void Update()
        {
            // Return if no callback event is defined
            if (confirmation.callback == null) return;

            // Accept or cancel based on what key is pressed
            if (Input.GetKeyDown(KeyCode.Return)) Accept();
            if (Input.GetKeyDown(KeyCode.Escape)) Cancel();
        }

        public void Accept()
        {
            // Confirmation is accepted
            confirmation.callback(true);
            Destroy(gameObject);
        }
        public void Cancel()
        {
            // Confirmation is declined
            confirmation.callback(false);
            Destroy(gameObject);
        }

        public void UpdateUI(Confirmation confirmation)
        {
            // Update header and buttons
            question.text = confirmation.question;
            accept.text = confirmation.acceptText;
            cancel.text = confirmation.cancelText;
        }
    }

    public struct Confirmation
    {
        public string question;
        public string acceptText;
        public string cancelText;
        public Action<bool> callback;

        public Confirmation(string _question, string _acceptText, string _cancelText, Action<bool> _callback)
        {
            question = _question;
            acceptText = _acceptText;
            cancelText = _cancelText;
            callback = _callback;
        }
    }
}