using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class CollaboratorPanel : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private CollaboratorHolder holderPrefab;

        private Dictionary<string, CollaboratorHolder> holders = new Dictionary<string, CollaboratorHolder>();
        private Action<List<Collaborator>> callback;
        private float lastCount;

        private void Update()
        {
            if (lastCount != content.childCount)
            {
                lastCount = content.childCount;
                SortContent();
            }
        }

        private void LoadHolder(string id, Collaborator collaborator)
        {
            // Instantiate the holder and load its data
            CollaboratorHolder holder = Instantiate(holderPrefab, content);
            holders.Add(id, holder);
            holder.LoadData(collaborator);
        }
        private void LoadHolders(List<Collaborator> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                LoadHolder(data[i].user, data[i]);
            }
        }
        public void RefreshUsers()
        {
            SocketManager.EmitAsync("get-users", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    if (!ConnectionManager.Info.isMaster) LoadHolder(ConnectionManager.Info.master, new Collaborator(ConnectionManager.Info.master, false));
                    var users = callback.GetValue(1).EnumerateArray().ToList();

                    for (int i = 0; i < users.Count; i++)
                    {
                        string id = users[i].GetString();
                        if (holders.ContainsKey(id) || id == GameData.User.id) continue;
                        LoadHolder(id, new Collaborator(id, false));
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            });
        }

        public void LoadData(List<Collaborator> data, Action<List<Collaborator>> onSelected)
        {
            RemoveHolders();
            LoadHolders(data);
            callback = onSelected;
            gameObject.SetActive(true);
            LeanTween.size((RectTransform)transform, new Vector2(180.0f, 146.0f), 0.2f);
        }
        private void RemoveHolders()
        {
            foreach (var key in holders.Keys.ToList())
            {
                CollaboratorHolder holder = holders[key];
                holders.Remove(key);
                Destroy(holder.gameObject);
            }
        }
        public void ClosePanel()
        {
            LeanTween.size((RectTransform)transform, new Vector2(180.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                SaveData();
                gameObject.SetActive(false);
            });
        }
        private void SaveData()
        {
            // Create list of permissions
            List<Collaborator> list = new List<Collaborator>();
            for (int i = 0; i < holders.Count; i++)
            {
                list.Add(holders.ElementAt(i).Value.SaveData());
            }

            if (callback != null) callback?.Invoke(list);
        }

        private void SortContent()
        {
            List<CollaboratorHolder> listOfHolders = holders.Values.ToList();

            listOfHolders.Sort(SortByName);

            for (int i = 0; i < listOfHolders.Count; i++)
            {
                listOfHolders[i].transform.SetSiblingIndex(i);
            }
        }
        private int SortByName(CollaboratorHolder holderA, CollaboratorHolder holderB)
        {
            return holderA.Name.CompareTo(holderB.Name);
        }
    }
}
