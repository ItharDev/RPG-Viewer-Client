using UnityEngine;
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
    }
}