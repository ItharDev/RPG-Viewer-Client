using UnityEngine.EventSystems;

namespace RPG
{
    public class AllowLight : EventTrigger
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            LightTools.Instance.MouseOver = true;
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            LightTools.Instance.MouseOver = false;
        }
    }
}