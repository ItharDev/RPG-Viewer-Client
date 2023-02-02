using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class CollaboratorHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Toggle toggle;

        private string uid;

        public async void LoadData(Collaborator data)
        {
            uid = data.user;
            text.text = data.user;
            toggle.isOn = data.isCollaborator;

            await SocketManager.Socket.EmitAsync("get-user", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean()) text.text = callback.GetValue(1).GetProperty("name").GetString();
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, uid);

        }

        public Collaborator GetData()
        {
            return new Collaborator()
            {
                user = uid,
                isCollaborator = toggle.isOn
            };
        }
    }
}