using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace RPG
{
    public class TokenConditions : MonoBehaviour
    {
        [SerializeField] private List<ConditionHolder> holders = new List<ConditionHolder>();
        [SerializeField] private List<ConditionHolder> placeHolders = new List<ConditionHolder>();
        [SerializeField] private ConditionHolder deadCondition;
        [SerializeField] private RectTransform conditionsPanel;

        public bool IsDead
        {
            get { return ConditionFlags.HasFlag(deadCondition.Condition.flag); }
        }
        public bool MouseOver
        {
            get
            {
                return RectTransformUtility.RectangleContainsScreenPoint(conditionsPanel, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }

        private Token token;
        public bool IsOpen;
        public ConditionFlag ConditionFlags;

        private void OnEnable()
        {
            // Get reference of main class
            if (token == null) token = GetComponent<Token>();

            // Load condition holders
            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].transform.SetSiblingIndex(i);
            }
        }
        private void Update()
        {
            // Return if token is not selected or we are edting any fields
            if (!token.Selected || token.UI.Editing) return;

            if (Input.GetKeyDown(KeyCode.X)) ToggleCondition(deadCondition);
        }

        public void SetConditions(int conditions)
        {
            // Update conditions on main class
            token.Data.conditions = conditions;
            ConditionFlags = (ConditionFlag)conditions;

            // Activate / deactivate conditions based on flags
            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].gameObject.SetActive(ConditionFlags.HasFlag(holders[i].Condition.flag));
                placeHolders[i].Toggle(ConditionFlags.HasFlag(placeHolders[i].Condition.flag));
            }

            // Activate / deactivate dead condition
            deadCondition.gameObject.SetActive(IsDead);
        }
        public void ToggleConditions(bool instant = false)
        {
            IsOpen = !IsOpen;
            LeanTween.size(conditionsPanel, new Vector2(IsOpen ? 105.0f : 0.0f, 155.0f), instant ? 0.0f : 0.2f);
        }
        public void ToggleCondition(ConditionHolder holder)
        {
            if (ConditionFlags.HasFlag(holder.Condition.flag)) ConditionFlags &= ~holder.Condition.flag;
            else ConditionFlags |= holder.Condition.flag;

            SocketManager.EmitAsync("update-conditions", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Reload conditions
                ConditionFlags = (ConditionFlag)token.Data.conditions;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString(), MessageType.Error);
            }, token.Id, (int)ConditionFlags);
        }
    }
}