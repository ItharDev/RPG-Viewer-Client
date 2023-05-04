using System.Collections.Generic;
using UnityEngine;

namespace RPG
{
    public class TokenConditions : MonoBehaviour
    {
        [SerializeField] private List<ConditionHolder> holders = new List<ConditionHolder>();
        [SerializeField] private ConditionHolder deadCondition;

        public bool IsDead
        {
            get { return conditionFlags.HasFlag(deadCondition.condition.flag); }
        }

        private Token token;
        private ConditionFlag conditionFlags;

        private void OnValidate()
        {
            // Get reference of main class
            if (token == null) token = GetComponent<Token>();

            // Load condition holders
            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].transform.SetSiblingIndex(i);
            }
        }

        public void SetConditions(int conditions)
        {
            // Update conditions on main class
            token.Data.conditions = conditions;
            conditionFlags = (ConditionFlag)conditions;

            // Activate / deactivate conditions based on flags
            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].gameObject.SetActive(conditionFlags.HasFlag(holders[i].condition.flag));
            }

            // Activate / deactivate dead condition
            deadCondition.gameObject.SetActive(IsDead);
        }
    }
}