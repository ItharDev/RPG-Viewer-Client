using UnityEngine.EventSystems;

namespace RPG
{
    public class AllowMeasure : EventTrigger
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            MeasurementManager.Instance.MouseOver = true;
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            MeasurementManager.Instance.MouseOver = false;
        }
    }
}