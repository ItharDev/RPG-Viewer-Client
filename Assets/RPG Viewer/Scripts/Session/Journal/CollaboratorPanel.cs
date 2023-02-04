using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class CollaboratorPanel : MonoBehaviour
    {
        [SerializeField] private CollaboratorHolder collaboratorHolder;
        [SerializeField] private Transform permissionParent;

        private JournalHolder journal;
        private List<CollaboratorHolder> holders = new List<CollaboratorHolder>();

        public void LoadCollaborators(List<Collaborator> data, JournalHolder journal)
        {
            this.journal = journal;

            for (int i = 0; i < holders.Count; i++)
            {
                Destroy(holders[i].gameObject);
            }
            holders.Clear();

            for (int i = 0; i < data.Count; i++)
            {
                var obj = Instantiate(collaboratorHolder, permissionParent);
                obj.LoadData(data[i]);
                holders.Add(obj);
            }
        }

        public void SaveCollaborators()
        {
            var list = new List<Collaborator>();

            for (int i = 0; i < holders.Count; i++)
            {
                list.Add(holders[i].GetData());
            }

            journal.CloseSharing(list);
        }
        public async void RefreshCollaborators()
        {
            await SocketManager.Socket.EmitAsync("refresh-collaborators", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    var data = JsonUtility.FromJson<JournalData>(callback.GetValue(1).ToString());
                    LoadCollaborators(data.collaborators, journal);
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, journal.Id);
        }
    }
}