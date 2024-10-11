using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class DragPanel : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform targetPanel;
        [SerializeField] private bool useWorldPos;

        private Vector2 offset;
        private Vector2 screenSize;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (targetPanel == null) return;

                screenSize = (GetComponentInParent<Canvas>().transform as RectTransform).sizeDelta;
                offset = useWorldPos ? targetPanel.anchoredPosition - (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) : targetPanel.anchoredPosition - (Vector2)Input.mousePosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            targetPanel.anchoredPosition = useWorldPos ? (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset : (Vector2)Input.mousePosition + offset;
            KeepOnScreen();
        }

        private void KeepOnScreen()
        {
            float targetX = targetPanel.anchoredPosition.x;
            float targetY = targetPanel.anchoredPosition.y;

            if (IsTooHigh(screenSize))
                targetY = (screenSize.y / 2f) - (targetPanel.sizeDelta.y * (1f - targetPanel.pivot.y));
            if (IsTooLow(screenSize))
                targetY = -(screenSize.y / 2f) + (targetPanel.sizeDelta.y * targetPanel.pivot.y);
            if (IsTooFarRight(screenSize))
                targetX = (screenSize.x / 2f) - (targetPanel.sizeDelta.x * (1f - targetPanel.pivot.x));
            if (IsTooFarLeft(screenSize))
                targetX = -(screenSize.x / 2f) + (targetPanel.sizeDelta.x * targetPanel.pivot.x);

            targetPanel.anchoredPosition = new Vector2(targetX, targetY);
        }
        private bool IsTooHigh(Vector2 refRes)
        {
            return targetPanel.anchoredPosition.y + (targetPanel.sizeDelta.y * (1f - targetPanel.pivot.y)) > refRes.y / 2f;
        }
        private bool IsTooLow(Vector2 refRes)
        {
            return targetPanel.anchoredPosition.y - (targetPanel.sizeDelta.y * targetPanel.pivot.y) < -refRes.y / 2f;
        }
        private bool IsTooFarRight(Vector2 refRes)
        {
            return targetPanel.anchoredPosition.x + (targetPanel.sizeDelta.x * (1f - targetPanel.pivot.x)) > refRes.x / 2f;
        }
        private bool IsTooFarLeft(Vector2 refRes)
        {
            return targetPanel.anchoredPosition.x - (targetPanel.sizeDelta.x * targetPanel.pivot.x) < -refRes.x / 2f;
        }
    }
}