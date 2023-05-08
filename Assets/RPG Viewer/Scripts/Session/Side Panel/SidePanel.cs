using UnityEngine;
using UnityEngine.Events;

namespace RPG
{
    public class SidePanel : MonoBehaviour
    {
        [SerializeField] private RectTransform tokens;
        [SerializeField] private RectTransform scenes;
        [SerializeField] private RectTransform journals;

        [Space]
        [SerializeField] private RectTransform buttonRect;
        [SerializeField] private RectTransform panelRect;

        [Space]
        [SerializeField] private GameObject tokensPanel;
        [SerializeField] private GameObject scenesPanel;
        [SerializeField] private GameObject journalsPanel;

        private bool open;

        public void SelectTokens()
        {
            if (tokens.sizeDelta.x == 110.0f)
            {
                ClosePanel();
                return;
            }
            else if (!open) OpenPanel();

            tokensPanel.SetActive(true);
            scenesPanel.SetActive(false);
            journalsPanel.SetActive(false);

            LeanTween.size(tokens, new Vector2(110.0f, 50.0f), 0.2f);
            LeanTween.size(scenes, new Vector2(50.0f, 50.0f), 0.2f);
            LeanTween.size(journals, new Vector2(50.0f, 50.0f), 0.2f);
        }
        public void SelectScenes()
        {
            if (scenes.sizeDelta.x == 110.0f)
            {
                ClosePanel();
                return;
            }
            else if (!open) OpenPanel();

            scenesPanel.SetActive(true);
            tokensPanel.SetActive(false);
            journalsPanel.SetActive(false);

            LeanTween.size(scenes, new Vector2(110.0f, 50.0f), 0.2f);
            LeanTween.size(tokens, new Vector2(50.0f, 50.0f), 0.2f);
            LeanTween.size(journals, new Vector2(50.0f, 50.0f), 0.2f);
        }
        public void SelectJournals()
        {
            if (journals.sizeDelta.x == 110.0f)
            {
                ClosePanel();
                return;
            }
            else if (!open) OpenPanel();

            journalsPanel.SetActive(true);
            tokensPanel.SetActive(false);
            scenesPanel.SetActive(false);

            LeanTween.size(journals, new Vector2(110.0f, 50.0f), 0.2f);
            LeanTween.size(scenes, new Vector2(50.0f, 50.0f), 0.2f);
            LeanTween.size(tokens, new Vector2(50.0f, 50.0f), 0.2f);
        }

        private void ClosePanel()
        {
            open = false;

            LeanTween.size(buttonRect, new Vector2(150.0f, 50.0f), 0.2f);
            LeanTween.size(panelRect, new Vector2(0.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                journalsPanel.SetActive(false);
                tokensPanel.SetActive(false);
                scenesPanel.SetActive(false);
            });

            LeanTween.size(tokens, new Vector2(50.0f, 50.0f), 0.2f);
            LeanTween.size(scenes, new Vector2(50.0f, 50.0f), 0.2f);
            LeanTween.size(journals, new Vector2(50.0f, 50.0f), 0.2f);
        }
        private void OpenPanel()
        {
            open = true;

            LeanTween.size(buttonRect, new Vector2(210.0f, 50.0f), 0.2f);
            LeanTween.size(panelRect, new Vector2(210.0f, 1000.0f), 0.2f);
        }
    }

    public struct Folder
    {
        public string id;
        public string path;
        public string name;
        public Color color;

        public Folder(string _id, string _path, string _name, Color _color)
        {
            id = _id;
            path = _path;
            name = _name;
            color = _color;
        }
    }
}