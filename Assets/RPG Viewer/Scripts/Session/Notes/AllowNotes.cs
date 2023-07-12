using UnityEngine.EventSystems;

namespace RPG
{
    public class AllowNotes : EventTrigger
    {
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            NoteTools.Instance.MouseOver = true;
        }
        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            NoteTools.Instance.MouseOver = false;
        }
    }
}