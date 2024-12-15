using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class SessionCard : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image image;
        [SerializeField] private GameObject settingsPanel;

        public Sprite GetSprite { get { return image.sprite; } }
        public string GetId { get { return id; } }
        public string GetName { get { return nameText.text; } }

        private string id;
        private bool isMaster;
        private SessionPanel sessionPanel;

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1)) return;
            if (settingsPanel.activeSelf && !RectTransformUtility.RectangleContainsScreenPoint((RectTransform)settingsPanel.transform, Input.mousePosition)) ToggleSettings();
        }

        public void SetData(string _id, bool _isMaster, string name, string imageId, SessionPanel _sessionPanel)
        {
            id = _id;
            isMaster = _isMaster;
            sessionPanel = _sessionPanel;
            nameText.text = name;

            WebManager.Download(imageId, true, async (bytes) =>
            {
                // Check if landing page exists
                if (bytes == null)
                {
                    MessageManager.QueueMessage("Failed to load landing page, please try again", MessageType.Error);
                    return;
                }

                await UniTask.SwitchToMainThread();

                // Create landing page texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                image.sprite = sprite;
            });
        }

        public void OnClick(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            if (pointerEventData.button == PointerEventData.InputButton.Left) JoinSession();
            else if (pointerEventData.button == PointerEventData.InputButton.Right && isMaster) ToggleSettings();
        }

        public void JoinSession()
        {
            sessionPanel.JoinSession(id);
        }

        public void EditSession()
        {
            sessionPanel.EditSession(id, this);
        }

        public void DeleteSession()
        {
            sessionPanel.DeleteSession(id, this);
        }

        public void ToggleSettings()
        {
            settingsPanel.SetActive(!settingsPanel.activeSelf);
        }
    }
}