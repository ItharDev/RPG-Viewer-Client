using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class InitiativeHolder : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField valueInput;
        [SerializeField] private Image visibilityImage;

        [SerializeField] private Sprite visibleImage;
        [SerializeField] private Sprite hiddenImage;

        [SerializeField] private Color visibleColor;
        [SerializeField] private Color hiddenColor;


        private InitiativeController controller;
        private Vector2 offset;

        public InitiativeData Data;

        public void LoadData(InitiativeController _controller, InitiativeData _data)
        {
            controller = _controller;
            Data = _data;

            nameInput.text = _data.name;
            valueInput.text = _data.roll;
            visibilityImage.sprite = Data.visible ? visibleImage : hiddenImage;

            nameInput.placeholder.color = Data.visible ? new Color(visibleColor.r, visibleColor.g, visibleColor.b, 0.5f) : new Color(hiddenColor.r, hiddenColor.g, hiddenColor.b, 0.5f); nameInput.placeholder.color = Data.visible ? new Color(visibleColor.r, visibleColor.g, visibleColor.b, 0.5f) : new Color(hiddenColor.r, hiddenColor.g, hiddenColor.b, 0.5f);
            nameInput.textComponent.color = Data.visible ? visibleColor : hiddenColor;

            transform.SetSiblingIndex(_data.index);
        }
        public void UpdateIndex()
        {
            Data.index = transform.GetSiblingIndex();
        }

        [System.Obsolete]
        public void UpdateVisibility()
        {
            Data.visible = !Data.visible;
            visibilityImage.sprite = Data.visible ? visibleImage : hiddenImage;
            nameInput.placeholder.color = Data.visible ? new Color(visibleColor.r, visibleColor.g, visibleColor.b, 0.5f) : new Color(hiddenColor.r, hiddenColor.g, hiddenColor.b, 0.5f); nameInput.placeholder.color = Data.visible ? new Color(visibleColor.r, visibleColor.g, visibleColor.b, 0.5f) : new Color(hiddenColor.r, hiddenColor.g, hiddenColor.b, 0.5f);
            nameInput.textComponent.color = Data.visible ? visibleColor : hiddenColor;
            controller.UpdateHolder();
        }

        [System.Obsolete]
        public void ModifyData()
        {
            Data.name = nameInput.text;
            Data.roll = valueInput.text;
            controller.UpdateHolder();
        }

        [System.Obsolete]
        public void RemoveHolder()
        {
            controller.RemoveHolder(this);
        }
    }
}
