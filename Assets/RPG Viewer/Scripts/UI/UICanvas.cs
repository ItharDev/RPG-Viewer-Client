using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class UICanvas : MonoBehaviour
    {
        public static UICanvas Instance { get; private set; }

        [SerializeField] private Image imagePanel;
        [SerializeField] private BlockPause pauseBlocker;

        private void OnEnable()
        {
            Events.OnImageShowed.AddListener(ShowImage);
        }
        private void OnDisable()
        {
            Events.OnImageShowed.RemoveListener(ShowImage);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && imagePanel.gameObject.activeInHierarchy)
            {
                imagePanel.gameObject.SetActive(false);
                PauseHandler.Instance.RemoveBlocker(pauseBlocker);
            }
        }

        private void ShowImage(string id, string uid)
        {
            if (uid == GameData.User.id) return;

            WebManager.Download(id, true, async (bytes) =>
            {
                await UniTask.SwitchToMainThread();

                // Create texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                if (sprite == null) return;

                imagePanel.gameObject.SetActive(true);
                imagePanel.sprite = sprite;
                PauseHandler.Instance.AddBlocker(pauseBlocker);
            });
        }

        private void Awake()
        {
            Instance = this;
        }
    }
}