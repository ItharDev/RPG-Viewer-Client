using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG
{
    public class PauseHandler : MonoBehaviour
    {
        [SerializeField] private RectTransform optionsPanel;
        [SerializeField] private RectTransform resolutionPanel;
        [SerializeField] private ResolutionHandler resolutionHandler;

        public PauseHandler Instance { get; private set; }

        private void Awake()
        {
            // Create instance
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            resolutionHandler.LoadResolutions();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) TogglePanel();
        }

        private void TogglePanel()
        {
            if (optionsPanel.gameObject.activeInHierarchy)
            {
                LeanTween.size(resolutionPanel, new Vector2(0.0f, 300.0f), 0.2f).setOnComplete(() =>
                {
                    resolutionPanel.gameObject.SetActive(false);
                });

                LeanTween.size(optionsPanel, new Vector2(280.0f, 0.0f), 0.2f).setOnComplete(() =>
                {
                    optionsPanel.gameObject.SetActive(false);
                });
            }

            else
            {
                optionsPanel.gameObject.SetActive(true);
                LeanTween.size(optionsPanel, new Vector2(280.0f, 300.0f), 0.2f);
            }
        }

        public void MainMenu()
        {
            TogglePanel();
            SceneManager.LoadScene("Menu");
        }
        public void Settings()
        {
            if (resolutionPanel.gameObject.activeInHierarchy)
            {
                LeanTween.size(resolutionPanel, new Vector2(0.0f, 300.0f), 0.2f).setOnComplete(() =>
                {
                    resolutionPanel.gameObject.SetActive(false);
                });
            }

            else
            {
                resolutionPanel.gameObject.SetActive(true);
                LeanTween.size(resolutionPanel, new Vector2(240.0f, 300.0f), 0.2f);
            }
        }
        public void Quit()
        {
            TogglePanel();
            Application.Quit();
        }
    }
}