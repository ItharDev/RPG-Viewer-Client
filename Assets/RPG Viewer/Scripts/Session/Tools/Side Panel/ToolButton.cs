using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class ToolButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private GameObject outline;
        [SerializeField] private GameObject tooltip;
        [SerializeField] private int buttonColumns;

        public void Select()
        {
            outline.SetActive(true);
            tooltip.GetComponent<RectTransform>().anchoredPosition = new Vector2(5.0f + 35.0f * buttonColumns, 0.0f);
        }
        public void Deselect()
        {
            outline.SetActive(false);
            tooltip.GetComponent<RectTransform>().anchoredPosition = new Vector2(5.0f, 0.0f);
        }

        public void Activate(bool active)
        {
            if (icon == null) return;

            Color color = icon.color;
            color.a = active ? 1.0f : 0.5f;
            icon.color = color;
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