using UnityEngine;
using UnityEngine.Events;

namespace RPG
{
    public class DragPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform dragRect;
        [SerializeField] private bool useWorldPos;

        private bool dragging;
        private Vector2 offset;
        private bool firstClick;
        private float clickTime;

        public UnityEvent minimise = new UnityEvent();

        private void Update()
        {
            if (firstClick) clickTime += Time.deltaTime;
            if (RectTransformUtility.RectangleContainsScreenPoint(dragRect, useWorldPos ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : Input.mousePosition))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    if (!firstClick)
                    {
                        firstClick = true;
                        clickTime = 0f;
                    }
                    else
                    {
                        if (clickTime < 0.2f) minimise?.Invoke();
                        firstClick = false;
                        clickTime = 0f;
                    }
                    dragging = true;
                    offset = useWorldPos ? transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition) : transform.position - Input.mousePosition;
                }
            }

            if (dragging)
            {
                transform.position = useWorldPos ? (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset : (Vector2)Input.mousePosition + offset;
                if (Input.GetMouseButtonUp(0)) dragging = false;
            }
        }
    }
}
