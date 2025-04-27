using System;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class EffectHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;

        public EffectData Data;

        private Action<EffectData> callback;
        private EffectList list;
        private byte[] image;

        public void LoadData(EffectData data, Action<EffectData> onSelected, EffectList _list)
        {
            Data = data;
            callback = onSelected;
            header.text = data.name;
            list = _list;

            WebManager.Download(data.image, true, (bytes) =>
            {
                // Return if image couldn't be loaded
                if (bytes == null) return;
                image = bytes;
            });
        }

        public void OnPointerClick(BaseEventData eventData)
        {
            callback?.Invoke(Data);
        }
        public void Modify()
        {
            if (image == null)
            {
                MessageManager.QueueMessage("Data not loaded yet, please try again later.");
                return;
            }
            list.ModifyEffect(Data, image);
        }
        public void Remove()
        {
            MessageManager.AskConfirmation(new Confirmation("Delete effect", "Delete", "Cancel", (result) =>
            {
                if (result) SocketManager.EmitAsync("remove-effect", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Data.id);
            }));
        }
    }
}