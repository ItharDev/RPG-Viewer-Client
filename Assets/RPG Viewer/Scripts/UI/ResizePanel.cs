using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class ResizePanel : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private RectTransform targetPanel;
        [SerializeField] private bool allowX;
        [SerializeField] private bool allowY;

        private Vector2 currentPointerPosition;
        private Vector2 previousPointerPosition;
        private Vector2 minSize;
        private Vector2 maxSize;

        public void UpdateRange(Vector2 _minSize, Vector2 _maxSize)
        {
            // Set new min and max values
            minSize = _minSize;
            maxSize = _maxSize;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Get current pointer position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetPanel, eventData.position, eventData.pressEventCamera, out previousPointerPosition);
        }
        public void OnDrag(PointerEventData eventData)
        {
            // Store panel's current size
            Vector2 sizeDelta = targetPanel.sizeDelta;

            // Get new pointer position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(targetPanel, eventData.position, eventData.pressEventCamera, out currentPointerPosition);

            // Calculate size difference
            Vector2 resizeValue = currentPointerPosition - previousPointerPosition;

            // Calculate new panel size
            sizeDelta += new Vector2(allowX ? resizeValue.x : 0.0f, allowY ? -resizeValue.y : 0.0f);
            sizeDelta = new Vector2(Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x), Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y));

            // Update panel's size
            targetPanel.sizeDelta = sizeDelta;
            previousPointerPosition = currentPointerPosition;
        }
    }
}