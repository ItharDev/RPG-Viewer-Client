using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class PointController : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Image selection;

        private bool dragging;
        private bool selected;
        private PointController hoveredPoint;
        private LineController controller;
        private bool Hovered { get { return RectTransformUtility.RectangleContainsScreenPoint((RectTransform)image.transform, Camera.main.ScreenToWorldPoint(Input.mousePosition)); } }
        private Vector3 MousePos
        {
            get
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = -1.0f;
                return mousePos;
            }
        }

        private void OnEnable()
        {
            // Add event listeners
            Events.OnPointHovered.AddListener(HoverPoint);
            Events.OnPointClicked.AddListener(ClickPoint);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnPointHovered.RemoveListener(HoverPoint);
            Events.OnPointClicked.RemoveListener(ClickPoint);
        }
        private void Update()
        {
            if (selected)
            {
                if (!Hovered && Input.GetMouseButtonDown(0)) Events.OnPointClicked?.Invoke(null);
                if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) DeletePoint();
                if (Input.GetKeyDown(KeyCode.Space)) WallTools.Instance.SplitWall(controller, this);
            }
            if (Hovered && !dragging)
            {
                if (hoveredPoint == null)
                {
                    Events.OnPointHovered?.Invoke(this);
                    return;
                }
            }
            if (!Hovered && hoveredPoint == this) Events.OnPointHovered?.Invoke(null);
            if (dragging)
            {
                if (Input.GetMouseButtonUp(0)) dragging = false;
                transform.localPosition = MousePos;

                if (hoveredPoint == null) return;
                if (hoveredPoint != this) transform.localPosition = hoveredPoint.transform.localPosition;
            }
        }

        private void DeletePoint()
        {
            Events.OnPointDeleted?.Invoke(this);

            if (hoveredPoint == null) return;
            if (hoveredPoint == this) Events.OnPointHovered?.Invoke(null);
        }

        private void HoverPoint(PointController point)
        {
            if (point == this)
            {
                transform.localScale = new Vector3(0.020f, 0.020f, 1.0f);
                Events.OnLineHovered?.Invoke(controller);
            }
            else transform.localScale = new Vector3(0.015f, 0.015f, 1.0f);
            hoveredPoint = point;
        }
        private void ClickPoint(PointController point)
        {
            if (point == null) selected = false;

            if (point == this && selected) selected = false;
            else selected = point == this;
            selection.gameObject.SetActive(selected);
        }

        public void Initialise(Color color, LineController _controller, bool stationary)
        {
            image.color = color;
            controller = _controller;
            dragging = !stationary;
            if (dragging) Events.OnPointDragged?.Invoke(this);
        }
        public void UpdateColor(Color color)
        {
            image.color = color;
        }

        public void OnBeginDrag(BaseEventData eventData)
        {
            // Return if dragging with other than right click
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;

            if (hoveredPoint == this) Events.OnPointHovered?.Invoke(null);

            if (Input.GetKey(KeyCode.LeftControl))
            {
                dragging = false;
                Events.OnPointContinued?.Invoke(this);
            }
            else
            {
                Events.OnPointDragged?.Invoke(this);
                dragging = true;
            }
        }
        public void OnEndDrag(BaseEventData eventData)
        {
            dragging = false;
            Events.OnPointHovered?.Invoke(this);

            // Return if dragging with other than right click
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.button != PointerEventData.InputButton.Left) return;

            Events.OnPointDragged?.Invoke(null);
        }
        public void OnClick(BaseEventData eventData)
        {
            PointerEventData pointerData = (PointerEventData)eventData;
            if (dragging) return;

            if (pointerData.button == PointerEventData.InputButton.Left) Events.OnPointClicked?.Invoke(this);
            if (pointerData.button == PointerEventData.InputButton.Right) controller.ConfigureWall();
        }
    }
}