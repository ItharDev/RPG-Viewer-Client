using System.Collections.Generic;
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
        [SerializeField] private GameObject clickBlocker;

        private List<UIPanel> panels = new List<UIPanel>();

        private void OnEnable()
        {
            Events.OnImageShowed.AddListener(ShowImage);
        }
        private void OnDisable()
        {
            Events.OnImageShowed.RemoveListener(ShowImage);
        }
        private void Awake()
        {
            Instance = this;
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

        public void OpenPanel(Transform panel)
        {
            UIPanel uiPanel = new UIPanel
            {
                panel = panel,
                parent = panel.parent,
                isActive = panel.gameObject.activeSelf,
                index = panel.GetSiblingIndex()
            };

            // Move panel to canvas
            panels.Add(uiPanel);
            panel.gameObject.SetActive(true);
            panel.SetParent(transform);
            panel.SetAsLastSibling();
            PauseHandler.Instance.AddBlocker(pauseBlocker);
            clickBlocker.SetActive(true);
        }

        public void ClosePanel(Transform panel)
        {
            UIPanel uiPanel = panels.Find(p => p.panel == panel);
            if (uiPanel.panel == null) return;

            // Move panel back to parent
            uiPanel.panel.gameObject.SetActive(uiPanel.isActive);
            uiPanel.panel.SetParent(uiPanel.parent);
            uiPanel.panel.SetSiblingIndex(uiPanel.index);
            panels.Remove(uiPanel);

            if (panels.Count > 0) return;

            // Remove click blocker
            PauseHandler.Instance.RemoveBlocker(pauseBlocker);
            clickBlocker.SetActive(false);
        }

        private struct UIPanel
        {
            public Transform panel;
            public Transform parent;
            public bool isActive;
            public int index;
        }
    }
}