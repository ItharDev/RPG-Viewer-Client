using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class InitiativeHolder : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField valueInput;

        [SerializeField] private Color visibleColor;
        [SerializeField] private Color hiddenColor;
        [SerializeField] private Image visibilityImage;
        [SerializeField] private Sprite visibleSprite;
        [SerializeField] private Sprite hiddenSprite;

        public InitiativeData Data;
        private bool visible;

        public void LoadData(InitiativeData _data)
        {
            Data = _data;
            visible = Data.visible;

            nameInput.text = _data.name;
            valueInput.text = _data.roll == 0 ? "0" : _data.roll.ToString();

            nameInput.placeholder.color = _data.visible ? visibleColor : hiddenColor;
            nameInput.textComponent.color = _data.visible ? visibleColor : hiddenColor;
            visibilityImage.sprite = _data.visible ? visibleSprite : hiddenSprite;

            gameObject.SetActive(_data.visible || ConnectionManager.Info.isMaster);
        }
        public void SetVisible()
        {
            visible = !visible;
            Modify();
        }
        public void Modify()
        {
            InitiativeData data = new InitiativeData(Data.id, nameInput.text, int.Parse(valueInput.text), visible);
            SocketManager.EmitAsync("modify-initiative", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, Data.id, JsonUtility.ToJson(data));
        }
        public void Remove()
        {
            SocketManager.EmitAsync("remove-initiative", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, Data.id);
        }
    }
}