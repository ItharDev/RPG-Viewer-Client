using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RPG
{
    public class PointController : MonoBehaviour
    {

        public void Initialise(Color color, LineController controller)
        {

        }

        public void OnDrag(BaseEventData eventData)
        {
            // Return if dragging with other than left click
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.localPosition = mousePos;
            Events.OnPointDragged?.Invoke(this);
        }
        public void OnEndDrag(BaseEventData eventData)
        {
            // Return if dragging with other than left click
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;

            Events.OnPointDragged?.Invoke(null);
        }
    }
}