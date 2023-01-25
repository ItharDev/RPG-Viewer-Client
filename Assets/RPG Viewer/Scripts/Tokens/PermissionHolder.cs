using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;

namespace RPG
{
    public class PermissionHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private TMP_Dropdown dropdown;

        private string uid;

        public async void LoadData(Permission data)
        {
            uid = data.user;
            text.text = data.user;
            dropdown.value = (int)data.permission;

            await SocketManager.Socket.EmitAsync("get-user", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean()) text.text = callback.GetValue(1).GetProperty("name").GetString();
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, uid);

        }

        public Permission GetData()
        {
            return new Permission()
            {
                user = uid,
                permission = (PermissionType)dropdown.value
            };
        }
    }
}