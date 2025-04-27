using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Networking;
using SFB;
using UnityEngine;

namespace RPG
{
    public class EffectList : MonoBehaviour
    {
        [SerializeField] private Transform content;
        [SerializeField] private EffectHolder holderPrefab;
        [SerializeField] private EffectPanel configPanel;
        [SerializeField] private bool hideWhenMinimised;

        private Dictionary<string, EffectHolder> holders = new Dictionary<string, EffectHolder>();
        private float lastCount;
        private Action<EffectData> callback;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnEffectCreated.AddListener(LoadHolder);
            Events.OnEffectModified.AddListener(ModifyHolder);
            Events.OnEffectRemoved.AddListener(RemoveHolder);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnEffectCreated.RemoveListener(LoadHolder);
            Events.OnEffectModified.RemoveListener(ModifyHolder);
            Events.OnEffectRemoved.RemoveListener(RemoveHolder);
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
            foreach (var effect in EffectManager.Instance.Effects)
            {
                LoadHolder(effect.Key, effect.Value);
            }
        }
        private void LoadHolder(string id, EffectData data)
        {
            // Instantiate the holder and load its data
            EffectHolder holder = Instantiate(holderPrefab, content);
            data.id = id;
            holder.LoadData(data, callback, this);
            holders.Add(id, holder);
        }
        private void ModifyHolder(string id, EffectData data)
        {
            EffectHolder holder = holders[id];
            holder.LoadData(data, callback, this);
        }
        private void RemoveHolder(string id, EffectData data)
        {
            if (configPanel.Id == id) configPanel.ClosePanel(false);
            EffectHolder holder = holders[id];
            holders.Remove(id);
            Destroy(holder.gameObject);
        }
        private void RemoveHolders()
        {
            foreach (var key in holders.Keys.ToList())
            {
                if (configPanel.Id == key) configPanel.ClosePanel(false);
                EffectHolder holder = holders[key];
                holders.Remove(key);
                Destroy(holder.gameObject);
            }
        }

        public void LoadData(Action<EffectData> onSelected)
        {
            callback = onSelected;
            gameObject.SetActive(true);
            LeanTween.size((RectTransform)transform, new Vector2(160.0f, 268.0f), 0.2f);
            LoadHolders();
        }
        public void ClosePanel()
        {
            configPanel.ClosePanel(false);
            LeanTween.size((RectTransform)transform, new Vector2(160.0f, 0.0f), 0.2f).setOnComplete(() =>
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
        public async void CreateEffect()
        {
            await ImageTask((bytes) =>
            {
                if (bytes == null) return;
                configPanel.gameObject.SetActive(true);
                EffectData data = new EffectData(
                    string.Empty,
                    "New Effect",
                    string.Empty,
                    5.0f,
                    false,
                    new Color(1.0f, 1.0f, 1.0f, 1.0f),
                    new EffectAnimation(EffectAnimationType.None, 0, 0, 0)
                );
                configPanel.LoadData(data, bytes, (data, image) =>
                {
                    SocketManager.EmitAsync("create-effect", (callback) =>
                    {
                        // Check if the event was successful
                        if (callback.GetValue().GetBoolean()) return;

                        // Send error message
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    }, JsonUtility.ToJson(data), Convert.ToBase64String(image));
                });
            });
        }
        private async Task ImageTask(Action<byte[]> callback)
        {
            // Only allow image files
            ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg", "webp") };

            // Open file explorer
            StandaloneFileBrowser.OpenFilePanelAsync("Select file", "", extensions, false, (string[] paths) =>
            {
                // Return if no items are selected
                if (paths.Length == 0) callback(null);

                // Read bytes from selected file
                callback(File.ReadAllBytes(paths[0]));
            });
            await Task.Yield();
        }
        public void ModifyEffect(EffectData data, byte[] bytes)
        {
            configPanel.gameObject.SetActive(true);
            configPanel.LoadData(data, bytes, (newData, image) =>
            {
                bool imageChanged = !image.SequenceEqual(bytes);
                SocketManager.EmitAsync("modify-effect", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, data.id, JsonUtility.ToJson(newData), imageChanged ? Convert.ToBase64String(image) : null);
            });
        }

        private void SortContent()
        {
            List<EffectHolder> listOfHolders = holders.Values.ToList();

            listOfHolders.Sort(SortByName);

            for (int i = 0; i < listOfHolders.Count; i++)
            {
                listOfHolders[i].transform.SetSiblingIndex(i);
            }
        }
        private int SortByName(EffectHolder holderA, EffectHolder holderB)
        {
            return holderA.Data.name.CompareTo(holderB.Data.name);
        }
    }
}