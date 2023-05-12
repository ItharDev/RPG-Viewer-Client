using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class ConditionHolder : MonoBehaviour
    {
        public Condition condition;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text text;
        [SerializeField] private Color enabledColor;
        [SerializeField] private Color disabledIconColor;
        [SerializeField] private Color disabledTextColor;

        private Token token;

        private void OnEnable()
        {
            if (text == null) text = GetComponentInChildren<TMP_Text>(true);
            if (condition != null)
            {
                icon.sprite = condition.icon;
                gameObject.name = condition.name;
                if (text != null) text.text = condition.name;
            }
        }

        /* TODO: 
        private void Update()
        {
            if (token == null) token = GetComponentInParent<Token>(true);

            if (token != null)
            {
                gameObject.SetActive(token.conditionFlags.HasFlag(condition.flag));
            }

            if (Config != null)
            {
                icon.color = Config.ConditionFlags.HasFlag(condition.flag) ? enabledColor : disabledIconColor;
                text.color = Config.ConditionFlags.HasFlag(condition.flag) ? enabledColor : disabledTextColor;
            }
        }

        public void ToggleCondition()
        {
            Config.ToggleCondition(condition);
        }

        */
    }
}