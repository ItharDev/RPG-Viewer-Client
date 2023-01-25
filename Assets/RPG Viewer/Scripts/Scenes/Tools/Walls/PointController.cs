using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class PointController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerClickHandler
    {
        public Action<PointController> OnDragEvent;
        public Action<PointController> OnBeginDragEvent;
        public Action<PointController> OnEndDragEvent;

        public Action<PointController> OnClickEvent;

        public Action<PointController> OnEnterEvent;
        public Action<PointController> OnExitEvent;

        public LineController Line;
        public Image Image;
        public Image Outline;

        private bool mouseOver;

        private void Update()
        {
            if (!Line.Tools.Enabled) return;

            if (RectTransformUtility.RectangleContainsScreenPoint(Image.GetComponent<RectTransform>(), Camera.main.ScreenToWorldPoint(Input.mousePosition)) && !mouseOver)
            {
                mouseOver = true;
                OnEnterEvent?.Invoke(this);
                SelectDot();
            }
            else if (!RectTransformUtility.RectangleContainsScreenPoint(Image.GetComponent<RectTransform>(), Camera.main.ScreenToWorldPoint(Input.mousePosition)) && mouseOver)
            {
                mouseOver = false;
                OnExitEvent?.Invoke(this);
                DeselectDot();
            }
        }

        public void SelectDot()
        {
            LeanTween.scale(Image.transform.parent.gameObject, new Vector3(1.3f, 1.3f, 1), 0.10f);
        }
        public void DeselectDot()
        {
            LeanTween.scale(Image.transform.parent.gameObject, new Vector3(1, 1, 1), 0.10f);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Line.Tools.Enabled) return;

            if (eventData.button == PointerEventData.InputButton.Left && !eventData.dragging)
            {
                OnClickEvent?.Invoke(this);
            }
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (!Line.Tools.Enabled) return;

            if (eventData.pointerId == -1 && Line.SelectedPoint == this) OnDragEvent?.Invoke(this);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!Line.Tools.Enabled) return;

            if (eventData.pointerId == -1 && !Input.GetKey(KeyCode.LeftControl)) OnBeginDragEvent?.Invoke(this);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (!Line.Tools.Enabled) return;

            if (eventData.pointerId == -1 && !Input.GetKey(KeyCode.LeftControl)) OnEndDragEvent?.Invoke(this);
        }
    }
}