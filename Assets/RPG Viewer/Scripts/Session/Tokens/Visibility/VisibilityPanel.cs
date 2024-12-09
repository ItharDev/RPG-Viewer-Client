using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class VisibilityPanel : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private VisibilityHolder holderPrefab;

        private Dictionary<string, VisibilityHolder> holders = new Dictionary<string, VisibilityHolder>();
        private Action<List<Visible>> callback;
        private float lastCount;

        private void Update()
        {
            if (lastCount != content.childCount)
            {
                lastCount = content.childCount;
                SortContent();
            }
        }

        private void LoadHolder(string id, Visible visible)
        {
            // Instantiate the holder and load its data
            VisibilityHolder holder = Instantiate(holderPrefab, content);
            holders.Add(id, holder);
            holder.LoadData(visible);
        }
        private void LoadHolders(List<Visible> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                LoadHolder(data[i].user, data[i]);
            }
        }
        public void RefreshVisibility()
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
                        LoadHolder(id, new Visible(id, true));
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            });
        }

        public void LoadData(List<Visible> data, Action<List<Visible>> onSelected)
        {
            RemoveHolders();
            LoadHolders(data);
            callback = onSelected;
            gameObject.SetActive(true);
            LeanTween.size((RectTransform)transform, new Vector2(180.0f, 146.0f), 0.2f);
        }
        public void LoadData(List<Visible> data)
        {
            RemoveHolders();
            LoadHolders(data);
        }
        private void RemoveHolders()
        {
            foreach (var key in holders.Keys.ToList())
            {
                VisibilityHolder holder = holders[key];
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
            List<VisibilityHolder> listOfHolders = holders.Values.ToList();

            listOfHolders.Sort(SortByName);

            for (int i = 0; i < listOfHolders.Count; i++)
            {
                listOfHolders[i].transform.SetSiblingIndex(i);
            }
        }
        private int SortByName(VisibilityHolder holderA, VisibilityHolder holderB)
        {
            return holderA.Name.CompareTo(holderB.Name);
        }

        public List<Visible> GetData()
        {
            // Create list of permissions
            List<Visible> list = new List<Visible>();
            for (int i = 0; i < holders.Count; i++)
            {
                list.Add(holders.ElementAt(i).Value.SaveData());
            }

            return list;
        }
    }
}