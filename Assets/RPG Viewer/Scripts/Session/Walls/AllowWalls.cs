using UnityEngine.EventSystems;

namespace RPG
{
    public class AllowWalls : EventTrigger
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            WallTools.Instance.MouseOver = true;
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            WallTools.Instance.MouseOver = false;
        }
    }
}