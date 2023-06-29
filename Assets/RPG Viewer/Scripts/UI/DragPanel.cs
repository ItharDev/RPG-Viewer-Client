using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class DragPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private RectTransform targetPanel;
        [SerializeField] private bool useWorldPos;

        private bool dragging;
        private Vector2 offset;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (targetPanel == null) return;
                
                dragging = true;
                offset = useWorldPos ? targetPanel.anchoredPosition - (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) : targetPanel.anchoredPosition - (Vector2)Input.mousePosition;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragging) targetPanel.anchoredPosition = useWorldPos ? (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset : (Vector2)Input.mousePosition + offset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
        }
    }
}