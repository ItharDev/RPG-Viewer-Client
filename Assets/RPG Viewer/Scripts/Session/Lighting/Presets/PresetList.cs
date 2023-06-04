using System;
using System.Collections.Generic;
using System.Linq;
using Networking;
using UnityEngine;

namespace RPG
{
    public class PresetList : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private PresetHolder holderPrefab;
        [SerializeField] private PresetPanel configPanel;
        [SerializeField] private bool hideWhenMinimised;

        private Dictionary<string, PresetHolder> holders = new Dictionary<string, PresetHolder>();
        private float lastCount;
        private Action<PresetData> callback;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnPresetCreated.AddListener(LoadHolder);
            Events.OnPresetModified.AddListener(ModifyHolder);
            Events.OnPresetRemoved.AddListener(RemoveHolder);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnPresetCreated.RemoveListener(LoadHolder);
            Events.OnPresetModified.RemoveListener(ModifyHolder);
            Events.OnPresetRemoved.RemoveListener(RemoveHolder);
        }
        private void Update()
        {
            if (lastCount != content.childCount)
            {
                lastCount = content.childCount;
                SortContent();
            }
        }

        private void LoadHolders()
        {
            foreach (var preset in PresetManager.Instance.Presets)
            {
                LoadHolder(preset.Key, preset.Value);
            }
        }
        private void LoadHolder(string id, PresetData data)
        {
            // Instantiate the holder and load its data
            PresetHolder holder = Instantiate(holderPrefab, content);
            data.id = id;
            holder.LoadData(data, callback, this);
            holders.Add(id, holder);
        }
        private void ModifyHolder(string id, PresetData data)
        {
            PresetHolder holder = holders[id];
            holder.LoadData(data, callback, this);
        }
        private void RemoveHolder(string id)
        {
            if (configPanel.Id == id) configPanel.ClosePanel(false);
            PresetHolder holder = holders[id];
            holders.Remove(id);
            Destroy(holder.gameObject);
        }
        private void RemoveHolders()
        {
            foreach (var key in holders.Keys.ToList())
            {
                RemoveHolder(key);
            }
        }

        public void LoadData(Action<PresetData> onSelected)
        {
            callback = onSelected;
            gameObject.SetActive(true);
            LeanTween.size((RectTransform)transform, new Vector2(120.0f, 286.0f), 0.2f);
            LoadHolders();
        }
        public void ClosePanel()
        {
            configPanel.ClosePanel(false);
            LeanTween.size((RectTransform)transform, new Vector2(120.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                if (hideWhenMinimised)
                {
                    RemoveHolders();
                    gameObject.SetActive(false);
                    return;
                }

                Destroy(gameObject);
            });
        }
        public void CreatePreset()
        {
            configPanel.gameObject.SetActive(true);
            configPanel.LoadData(default, (data) =>
            {
                SocketManager.EmitAsync("create-preset", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, JsonUtility.ToJson(data));
            });
        }
        public void ModifyPreset(PresetData data)
        {
            configPanel.gameObject.SetActive(true);
            configPanel.LoadData(data, (data) =>
            {
                SocketManager.EmitAsync("modify-preset", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, data.id, JsonUtility.ToJson(data));
            });
        }

        private void SortContent()
        {
            List<PresetHolder> listOfHolders = holders.Values.ToList();

            listOfHolders.Sort(SortByName);

            for (int i = 0; i < listOfHolders.Count; i++)
            {
                listOfHolders[i].transform.SetSiblingIndex(i);
            }
        }
        private int SortByName(PresetHolder holderA, PresetHolder holderB)
        {
            return holderA.Data.name.CompareTo(holderB.Data.name);
        }
    }
}