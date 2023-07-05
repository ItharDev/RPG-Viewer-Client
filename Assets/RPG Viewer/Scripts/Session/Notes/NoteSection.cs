using Nobi.UiRoundedCorners;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class NoteSection : MonoBehaviour
    {
        [SerializeField] private TMP_InputField textInput;
        [SerializeField] private RectTransform image;
        [SerializeField] private RectTransform text;
        [SerializeField] private RectTransform separator;
        [SerializeField] private RectTransform resize;
        [SerializeField] private ImageWithRoundedCorners corners;

        public float AvailableSize { get { return rect.sizeDelta.x - image.sizeDelta.x - 35.0f; } }

        private RectTransform rect;
        private NoteUI noteUI;
        private RectTransform noteRect;
        private int allowedDirections = 0;

        public void Initialise(NoteUI _noteUI)
        {
            noteUI = _noteUI;
            noteRect = noteUI.GetComponent<RectTransform>();

            if (rect == null) rect = (RectTransform)transform;
            Resize();
        }
        public void Resize()
        {
            float position = Mathf.Clamp(separator.anchoredPosition.x, 5.0f, rect.sizeDelta.x - 30.0f);
            separator.anchoredPosition = new Vector2(position, separator.anchoredPosition.y);

            float imageWidth = separator.anchoredPosition.x - 10.0f;
            image.sizeDelta = new Vector2(imageWidth, -10.0f);
            text.sizeDelta = new Vector2(AvailableSize, rect.sizeDelta.y - 10.0f);
            // Refresh corners
            corners.Validate();
            corners.Refresh();
        }

        public void ResizeImage(BaseEventData eventData)
        {
            PointerEventData pointerData = (PointerEventData)eventData;
            separator.position = new Vector3(pointerData.position.x, separator.position.y, 0.0f);
            float position = Mathf.Clamp(separator.anchoredPosition.x, 5.0f, rect.sizeDelta.x - 30.0f);
            separator.anchoredPosition = new Vector2(position, separator.anchoredPosition.y);
            float imageWidth = separator.anchoredPosition.x - 10.0f;
            image.sizeDelta = new Vector2(imageWidth, -10.0f);
            text.sizeDelta = new Vector2(AvailableSize, rect.sizeDelta.y - 10.0f);
        }
        public void ResizePanel(BaseEventData eventData)
        {
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.position.y > resize.position.y && allowedDirections == 2) return;
            if (pointerData.position.y < resize.position.y && allowedDirections == 1) return;

            resize.position = new Vector3(resize.position.x, pointerData.position.y, 0.0f);
            float height = Mathf.Abs(resize.localPosition.y) + 5.0f;
            height = Mathf.Clamp(height, 30.0f, noteRect.sizeDelta.y - 35.0f);
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
            image.sizeDelta = new Vector2(image.sizeDelta.x, -10.0f);
            text.sizeDelta = new Vector2(text.sizeDelta.x, rect.sizeDelta.y - 10.0f);

            if (height == 30.0f) allowedDirections = 2;
            else if (height == noteRect.sizeDelta.y - 35.0f) allowedDirections = 1;
            else allowedDirections = 0;

            resize.anchoredPosition = new Vector2(-2.5f, 5.0f);

            // Refresh corners
            corners.Validate();
            corners.Refresh();
        }

        public SectionData GetData()
        {
            return new SectionData(textInput.text, "{image id}");
        }
    }
}