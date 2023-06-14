using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private MenuHandler menu;
        [SerializeField] private TMP_InputField addressInput;
        [SerializeField] private Slider fpsSlider;
        [SerializeField] private TMP_InputField fpsInput;

        private RectTransform rect;

        private void OnEnable()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();
            
            // Add event listeners
            Events.OnConnected.AddListener(OnConnected);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnConnected.RemoveListener(OnConnected);
        }
        private void Start()
        {
            // Connect automatically if there's address available
            string address = PlayerPrefs.GetString("Address");
            if (!string.IsNullOrEmpty(address)) SocketManager.Connect(address);

            int fps = PlayerPrefs.GetInt("FPS");
            if (fps != 0) UpdateFrameRate(fps);
        }

        private void UpdateFrameRate(int value)
        {
            fpsSlider.value = value;
            fpsInput.text = value.ToString();
            GameData.FrameRate = value;
            Application.targetFrameRate = value;
        }
        private void OnConnected()
        {
            // Clear address bar
            if (!string.IsNullOrEmpty(addressInput.text)) PlayerPrefs.SetString("Address", addressInput.text);
            addressInput.text = "";
        }

        public void OpenPanel()
        {
            // Close panel if it's open
            if (rect.sizeDelta.x != 0)
            {
                ClosePanel();
                return;
            }

            // Open panel
            menu.OpenSettings?.Invoke();
            LeanTween.size(rect, new Vector2(430.0f, 150.0f), 0.2f);
        }
        public void ClosePanel()
        {
            // Close panel
            LeanTween.size(rect, new Vector2(0.0f, 0.0f), 0.2f);
        }
        public void Connect()
        {
            // Send connection event
            string address = addressInput.text;
            SocketManager.Connect(address);
        }

        public void ChangeSlider()
        {
            fpsInput.text = fpsSlider.value.ToString();
        }
        public void ChangeInput()
        {
            float value = Mathf.Clamp(float.Parse(fpsInput.text), fpsSlider.minValue, fpsSlider.maxValue);
            fpsSlider.value = value;
            fpsInput.text = fpsSlider.value.ToString();
        }
        public void ApplyChanges()
        {
            GameData.FrameRate = (int)fpsSlider.value;
            Application.targetFrameRate = (int)fpsSlider.value;
            PlayerPrefs.SetInt("FPS", (int)fpsSlider.value);

            MessageManager.QueueMessage("Frame rate updated");
        }
    }
}