using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class ConditionHolder : MonoBehaviour
    {
        public Condition Condition;
        [SerializeField] private Image icon;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color deselectedColor;
        [SerializeField] private Token token;
        [SerializeField] private GameObject hover;

        private void OnEnable()
        {
            if (Condition != null)
            {
                icon.sprite = Condition.icon;
                gameObject.name = Condition.name;
            }
        }

        public void Toggle(bool selected)
        {
            icon.color = selected ? selectedColor : deselectedColor;
        }
        public void Select()
        {
            token.Conditions.ToggleCondition(this);
        }

        public void Hover()
        {
            hover.SetActive(true);
            hover.GetComponentInChildren<TMP_Text>().text = Condition.name;
        }

        public void EndHover()
        {
            hover.SetActive(false);
        }
    }
}