using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class VisibilityHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private Toggle toggle;

        public string Name;
        private Visible data;

        public Visible SaveData()
        {
            return new Visible(data.user, toggle.isOn);
        }
        public void LoadData(Visible _data)
        {
            data = _data;
            header.text = data.user;
            toggle.isOn = data.visible;
            GetUser();
        }

        private void GetUser()
        {
            SocketManager.EmitAsync("get-user", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    header.text = callback.GetValue(1).GetString();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, data.user);
        }
    }
}