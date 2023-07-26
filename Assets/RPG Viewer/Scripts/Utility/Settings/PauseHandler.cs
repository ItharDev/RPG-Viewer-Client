using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG
{
    public class PauseHandler : MonoBehaviour
    {
        [SerializeField] private RectTransform optionsPanel;
        [SerializeField] private RectTransform resolutionPanel;
        [SerializeField] private ResolutionHandler resolutionHandler;

        public static PauseHandler Instance { get; private set; }

        private List<BlockPause> blockers = new List<BlockPause>();

        private void Awake()
        {
            // Create instance
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);

            resolutionHandler.LoadResolutions();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) CheckBlockers();
        }

        private void CheckBlockers()
        {
            for (int i = 0; i < blockers.Count; i++)
            {
                if (blockers[i] == null) blockers.RemoveAt(i);
            }

            if (blockers.Count == 0) TogglePanel();
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

        public void AddBlocker(BlockPause blocker)
        {
            if (!blockers.Contains(blocker)) blockers.Add(blocker);
        }
        public void RemoveBlocker(BlockPause blocker)
        {
            if (blockers.Contains(blocker)) blockers.Remove(blocker);
        }
    }
}