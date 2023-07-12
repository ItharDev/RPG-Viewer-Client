using Cysharp.Threading.Tasks;
using Networking;
using Nobi.UiRoundedCorners;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class NoteUI : MonoBehaviour
    {
        [SerializeField] private RectTransform resize;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private ImageWithIndependentRoundedCorners corners;
        [SerializeField] private ImageWithIndependentRoundedCorners topCorners;

        [SerializeField] private TMP_InputField textInput;
        [SerializeField] private TMP_InputField headerInput;
        [SerializeField] private TMP_Text header;
        [SerializeField] private Image image;
        [SerializeField] private GameObject options;
        [SerializeField] private GameObject setPublic;
        [SerializeField] private GameObject showOthers;
        [SerializeField] private GameObject content;

        private RectTransform rect;
        private NoteInfo info;
        private NoteData data;

        private void Awake()
        {
            if (rect == null) rect = (RectTransform)transform;
        }

        public void LoadData(NoteInfo _info, NoteData _data)
        {
            info = _info;
            data = _data;

            textInput.text = data.text;
            headerInput.text = data.header;
            header.text = data.header;
            image.raycastTarget = info.IsOwner;
            textInput.readOnly = !info.IsOwner;
            setPublic.SetActive(info.owner == GameData.User.id);
            showOthers.SetActive(info.IsOwner);

            if (string.IsNullOrEmpty(data.image)) return;

            WebManager.Download(data.image, true, async (bytes) =>
            {
                // Return if image was not found
                if (bytes == null) return;

                // Generate texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            });
        }

        public void Close()
        {
            Debug.Log(GetData());
            Destroy(gameObject);
        }
        public void ToggleOptions()
        {
            options.SetActive(!options.activeInHierarchy);
        }

        public void ResizePanel(BaseEventData eventData)
        {
            PointerEventData pointerData = (PointerEventData)eventData;
            Debug.Log(pointerData.position);
            if (pointerData.position.x < rect.position.x || pointerData.position.y > rect.position.y) return;

            resize.position = new Vector3(pointerData.position.x, pointerData.position.y, 0.0f);
            float height = Mathf.Abs(resize.localPosition.y) + 30.0f;
            float width = Mathf.Abs(resize.localPosition.x) + 5.0f;
            height = Mathf.Clamp(height, 55.0f, Screen.height);
            width = Mathf.Clamp(width, 100.0f, Screen.width);

            rect.sizeDelta = new Vector2(width, height);

            resize.anchoredPosition = new Vector2(-5.0f, 5.0f);
            layoutElement.minHeight = height - 65.0f;

            content.SetActive(height != 55.0f);

            // Refresh corners
            corners.Validate();
            corners.Refresh();
            topCorners.Validate();
            topCorners.Refresh();
        }

        private string GetData()
        {
            return "";
        }
    }
}