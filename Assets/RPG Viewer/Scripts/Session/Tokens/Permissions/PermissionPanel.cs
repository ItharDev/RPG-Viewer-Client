using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class PermissionPanel : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private PermissionHolder holderPrefab;

        private Dictionary<string, PermissionHolder> holders = new Dictionary<string, PermissionHolder>();
        private Action<List<Permission>> callback;
        private float lastCount;

        private void Update()
        {
            if (lastCount != content.childCount)
            {
                lastCount = content.childCount;
                SortContent();
            }
        }

        private void LoadHolder(string id, Permission permission)
        {
            // Instantiate the holder and load its data
            PermissionHolder holder = Instantiate(holderPrefab, content);
            holders.Add(id, holder);
            holder.LoadData(permission);
        }
        private void LoadHolders(List<Permission> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                LoadHolder(data[i].user, data[i]);
            }
        }
        public void RefreshPermissions()
        {
            SocketManager.EmitAsync("get-users", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    var users = callback.GetValue(1).EnumerateArray().ToList();

                    for (int i = 0; i < users.Count; i++)
                    {
                        string id = users[i].GetString();
                        if (holders.ContainsKey(id)) continue;
                        LoadHolder(id, new Permission(id, PermissionType.None));
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }

        public void LoadData(List<Permission> data, Action<List<Permission>> onSelected)
        {
            RemoveHolders();
            LoadHolders(data);
            callback = onSelected;
            gameObject.SetActive(true);
            LeanTween.size((RectTransform)transform, new Vector2(180.0f, 146.0f), 0.2f);
        }
        public void LoadData(List<Permission> data)
        {
            RemoveHolders();
            LoadHolders(data);
        }
        private void RemoveHolders()
        {
            foreach (var key in holders.Keys.ToList())
            {
                PermissionHolder holder = holders[key];
                holders.Remove(key);
                Destroy(holder.gameObject);
            }
        }
        public void ClosePanel()
        {
            LeanTween.size((RectTransform)transform, new Vector2(180.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                if (callback != null) callback?.Invoke(GetData());
                gameObject.SetActive(false);
            });
        }

        private void SortContent()
        {
            List<PermissionHolder> listOfHolders = holders.Values.ToList();

            listOfHolders.Sort(SortByName);

            for (int i = 0; i < listOfHolders.Count; i++)
            {
                listOfHolders[i].transform.SetSiblingIndex(i);
            }
        }
        private int SortByName(PermissionHolder holderA, PermissionHolder holderB)
        {
            return holderA.Name.CompareTo(holderB.Name);
        }

        public List<Permission> GetData()
        {
            // Create list of permissions
            List<Permission> permissions = new List<Permission>();
            for (int i = 0; i < holders.Count; i++)
            {
                permissions.Add(holders.ElementAt(i).Value.SaveData());
            }

            return permissions;
        }
    }
}