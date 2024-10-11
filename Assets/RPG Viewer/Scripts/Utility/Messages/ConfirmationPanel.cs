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
            // Accept or cancel based on what key is pressed
            if (Input.GetKeyDown(KeyCode.Return)) Accept();
            if (Input.GetKeyDown(KeyCode.Escape)) Cancel();
        }

        public void Accept()
        {
            LeanTween.size((RectTransform)transform, new Vector2(240.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                // Confirmation is accepted
                if (confirmation.callback != null) confirmation.callback(true);
                Destroy(gameObject);
            });
        }
        public void Cancel()
        {
            LeanTween.size((RectTransform)transform, new Vector2(240.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                // Confirmation is declined
                if (confirmation.callback != null) confirmation.callback(false);
                Destroy(gameObject);
            });
        }

        public void UpdateUI(Confirmation _confirmation)
        {
            confirmation = _confirmation;

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