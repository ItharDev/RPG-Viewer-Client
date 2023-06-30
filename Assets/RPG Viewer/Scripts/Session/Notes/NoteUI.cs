using System.Collections.Generic;
using Nobi.UiRoundedCorners;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class NoteUI : MonoBehaviour
    {
        [SerializeField] private List<NoteSection> sections = new List<NoteSection>();
        [SerializeField] private RectTransform resize;
        [SerializeField] private ImageWithIndependentRoundedCorners corners;
        [SerializeField] private ImageWithIndependentRoundedCorners topCorners;

        private RectTransform rect;

        private void Awake()
        {
            if (rect == null) rect = (RectTransform)transform;

            for (int i = 0; i < sections.Count; i++)
            {
                sections[i].Initialise(this);
            }
        }

        public void ResizePanel(BaseEventData eventData)
        {
            PointerEventData pointerData = (PointerEventData)eventData;

            resize.position = new Vector3(pointerData.position.x, pointerData.position.y, 0.0f);
            float height = Mathf.Abs(resize.localPosition.y) + 30.0f;
            float width = Mathf.Abs(resize.localPosition.x) + 5.0f;
            height = Mathf.Clamp(height, 30.0f, Screen.currentResolution.height);
            width = Mathf.Clamp(width, 65.0f, Screen.currentResolution.width);
            rect.sizeDelta = new Vector2(width, height);

            resize.anchoredPosition = new Vector2(-5.0f, 5.0f);
            for (int i = 0; i < sections.Count; i++)
            {
                sections[i].Resize();
            }

            // Refresh corners
            corners.Validate();
            corners.Refresh();
            topCorners.Validate();
            topCorners.Refresh();
        }
    }
}