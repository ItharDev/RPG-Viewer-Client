using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RPG
{
    public class DragPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform dragRect;
        [SerializeField] private bool useWorldPos;

        private bool dragging;
        private Vector2 offset;

        public UnityEvent minimise = new UnityEvent();

        public void OnBeginDrag(BaseEventData baseEventData)
        {
            PointerEventData eventData = baseEventData as PointerEventData;
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                dragging = true;
                offset = useWorldPos ? transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition) : transform.position - Input.mousePosition;
            }
        }

        public void OnDrag(BaseEventData baseEventData)
        {
            PointerEventData eventData = baseEventData as PointerEventData;
            if (dragging) transform.position = useWorldPos ? (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset : (Vector2)Input.mousePosition + offset;
        }

        public void OnEndDrag(BaseEventData baseEventData)
        {
            PointerEventData eventData = baseEventData as PointerEventData;
            dragging = false;
        }

        public void OnPointerClick(BaseEventData baseEventData)
        {
            PointerEventData eventData = baseEventData as PointerEventData;
            if (eventData.button != PointerEventData.InputButton.Left || eventData.clickCount < 2) return;
            minimise.Invoke();
        }
    }
}
