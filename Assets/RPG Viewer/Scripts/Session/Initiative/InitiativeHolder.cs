using TMPro;
using UnityEngine;

namespace RPG
{
    public class InitiativeHolder : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameInput;
        [SerializeField] private TMP_InputField valueInput;

        [SerializeField] private Color visibleColor;
        [SerializeField] private Color hiddenColor;

        public InitiativeData Data;

        public void LoadData(InitiativeData _data)
        {
            Data = _data;

            nameInput.text = _data.name;
            valueInput.text = _data.roll == 0 ? "0" : _data.roll.ToString();

            nameInput.placeholder.color = _data.visible ? visibleColor : hiddenColor; 
            nameInput.textComponent.color = _data.visible ? visibleColor : hiddenColor;
        }
    }
}