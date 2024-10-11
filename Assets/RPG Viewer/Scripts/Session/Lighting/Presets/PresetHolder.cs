using System;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class PresetHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text header;

        public PresetData Data;

        private Action<PresetData> callback;
        private PresetList list;

        public void LoadData(PresetData data, Action<PresetData> onSelected, PresetList _list)
        {
            Data = data;
            callback = onSelected;
            header.text = data.name;
            list = _list;
        }

        public void OnPointerClick(BaseEventData eventData)
        {
            callback?.Invoke(Data);
        }
        public void Modify()
        {
            list.ModifyPreset(Data);
        }
        public void Remove()
        {
            MessageManager.AskConfirmation(new Confirmation("Delete preset", "Delete", "Cancel", (result) =>
            {
                if (result) SocketManager.EmitAsync("remove-preset", (callback) =>
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