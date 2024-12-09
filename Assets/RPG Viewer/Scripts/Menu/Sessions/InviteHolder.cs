using TMPro;
using UnityEngine;

namespace RPG
{
    public class InviteHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text buttonText;

        private bool isRemoved;
        private bool isInvited;
        private SessionEditor menu;

        public void SetData(string id, string name, string status, bool _isInvited, SessionEditor _menu)
        {
            nameText.text = name;
            statusText.text = status;
            isInvited = _isInvited;
            menu = _menu;
        }

        public void HandleButtonClick()
        {
            if (isInvited)
            {
                menu.CancelInvite(this);
                return;
            }

            isRemoved = !isRemoved;
            buttonText.text = isRemoved ? "Cancel" : "Remove";
        }
    }
}