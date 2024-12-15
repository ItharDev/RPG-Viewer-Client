using TMPro;
using UnityEngine;

namespace RPG
{
    public class InviteHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text buttonText;

        private string id;
        private string email;
        private string username;
        private bool isRemoved;
        private bool hasAccepted;
        private bool isNew;
        private SessionEditor menu;

        public string GetEmail { get { return email; } }

        public void SetData(string _id, string _email, string _name, bool _hasAccepted, bool _isNew, SessionEditor _menu)
        {
            id = _id;
            email = _email;
            username = _name;
            nameText.text = $"{username} ({email})";
            hasAccepted = _hasAccepted;
            isNew = _isNew;

            statusText.text = isNew ? "Invited" : hasAccepted ? "Accepted" : "Pending";
            buttonText.text = hasAccepted ? "Remove" : "Cancel";
            menu = _menu;
        }

        public void HandleButtonClick()
        {
            if (isNew)
            {
                menu.CancelInvite(this);
                return;
            }

            isRemoved = !isRemoved;
            buttonText.text = isRemoved ? "Cancel" : "Remove";
            statusText.text = isRemoved ? hasAccepted ? "Removed" : "Removed" : "Accepted";
        }
    }
}