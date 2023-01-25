using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class PermissionPanel : MonoBehaviour
    {
        [SerializeField] private PermissionHolder permissionHolder;
        [SerializeField] private Transform permissionParent;

        private BlueprintHolder blueprint;
        private List<PermissionHolder> holders = new List<PermissionHolder>();

        public void LoadPermissions(List<Permission> data, BlueprintHolder blueprint)
        {
            this.blueprint = blueprint;
            
            for (int i = 0; i < holders.Count; i++)
            {
                Destroy(holders[i].gameObject);
            }
            holders.Clear();

            for (int i = 0; i < data.Count; i++)
            {
                var obj = Instantiate(permissionHolder, permissionParent);
                obj.LoadData(data[i]);
                holders.Add(obj);
            }
        }

        public void SavePermissions()
        {
            var list = new List<Permission>();

            for (int i = 0; i < holders.Count; i++)
            {
                list.Add(holders[i].GetData());
            }

            blueprint.ClosePermissions(list);
        }
        public async void RefreshPermissions()
        {
            await SocketManager.Socket.EmitAsync("refresh-permissions", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    var data = JsonUtility.FromJson<TokenData>(callback.GetValue(1).ToString());
                    LoadPermissions(data.permissions, blueprint);
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, blueprint.Id);
        }
    }
}