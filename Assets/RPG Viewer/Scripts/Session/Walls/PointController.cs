using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RPG
{
    public class PointController : MonoBehaviour
    {
        public UnityEvent OnDragEvent = new UnityEvent();

        public void Initialise(Color color, LineController controller)
        {

        }

        public void OnDrag(BaseEventData eventData)
        {
            PointerEventData pointerData = (PointerEventData)eventData;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.localPosition = mousePos;
            OnDragEvent?.Invoke();
        }
    }
}