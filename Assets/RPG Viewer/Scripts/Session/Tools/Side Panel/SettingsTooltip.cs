using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RPG
{
    public class SettingsTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private GameObject tooltip;
        [SerializeField] private bool expandOnSelect;

        public void Select()
        {
            if (expandOnSelect) tooltip.GetComponent<RectTransform>().anchoredPosition = new Vector2(40.0f, 0.0f);
        }
        public void Deselect()
        {
            if (expandOnSelect) tooltip.GetComponent<RectTransform>().anchoredPosition = new Vector2(5.0f, 0.0f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            tooltip.SetActive(true);
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            tooltip.SetActive(false);
        }
    }
}