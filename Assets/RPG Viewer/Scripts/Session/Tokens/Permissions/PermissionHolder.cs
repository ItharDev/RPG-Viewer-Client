using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;

namespace RPG
{
    public class PermissionHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;
        [SerializeField] private TMP_Dropdown dropdown;

        public string Name;
        private Permission data;

        public Permission SaveData()
        {
            return new Permission(data.user, (PermissionType)dropdown.value);
        }
        public void LoadData(Permission _data)
        {
            data = _data;
            header.text = data.user;
            dropdown.value = (int)data.type;
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