using System;
using System.IO;
using System.Threading.Tasks;
using Networking;
using SFB;
using UnityEngine;
using UnityEngine.UI;

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

        [Space]
        [SerializeField] private GameObject imageButton;
        [SerializeField] private GameObject syncButton;
        [SerializeField] private GameObject pauseButton;
        [SerializeField] private Image syncIcon;
        [SerializeField] private Image pauseIcon;
        [SerializeField] private Sprite syncedSprite;
        [SerializeField] private Sprite desyncedSprite;
        [SerializeField] private Sprite pauseSprite;
        [SerializeField] private Sprite playSprite;
        [SerializeField] private GameObject tokensButton;
        [SerializeField] private GameObject scenesButton;

        [Space]
        [SerializeField] private PresetList presetList;
        [SerializeField] private EffectList effectList;

        private bool open;
        private bool isPaused;
        private float targetWidthOpen;
        private float targetWidthClose;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnStateChanged.AddListener(SetSynced);
            Events.OnGamePaused.AddListener(SetPaused);
        }
        private void OnDisable()
        {
            // Add event listeners
            Events.OnStateChanged.RemoveListener(SetSynced);
            Events.OnGamePaused.RemoveListener(SetPaused);
        }
        private void Start()
        {
            targetWidthOpen = 210.0f;
            targetWidthClose = 150.0f;
            if (!ConnectionManager.Info.isMaster)
            {
                imageButton.gameObject.SetActive(false);
                syncButton.gameObject.SetActive(false);
                pauseButton.gameObject.SetActive(false);
                scenesButton.gameObject.SetActive(false);
                targetWidthOpen = 160.0f;
                targetWidthClose = 100.0f;
            }

            ClosePanel();
        }

        private void SetSynced(SessionState oldState, SessionState newState)
        {
            syncIcon.sprite = newState.synced ? syncedSprite : desyncedSprite;
        }

        private void SetPaused(bool state)
        {
            pauseIcon.sprite = state ? pauseSprite : playSprite;
            isPaused = state;
        }

        public void SelectTokens()
        {
            Events.OnSidePanelChanged?.Invoke();

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
            Events.OnSidePanelChanged?.Invoke();

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
            Events.OnSidePanelChanged?.Invoke();

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

            LeanTween.size(buttonRect, new Vector2(targetWidthClose, 50.0f), 0.2f);
            LeanTween.size((RectTransform)transform, new Vector2(targetWidthClose, 50.0f), 0.2f);
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

            LeanTween.size(buttonRect, new Vector2(targetWidthOpen, 50.0f), 0.2f);
            LeanTween.size((RectTransform)transform, new Vector2(targetWidthOpen, 50.0f), 0.2f);
            LeanTween.size(panelRect, new Vector2(210.0f, 1000.0f), 0.2f);
        }

        public void OpenPresets()
        {
            if (presetList.gameObject.activeInHierarchy) return;

            presetList.LoadData(null);
            effectList.ClosePanel();
        }
        public void OpenEffects()
        {
            if (effectList.gameObject.activeInHierarchy) return;

            effectList.LoadData(null);
            presetList.ClosePanel();
        }
        public void Sync()
        {
            SocketManager.EmitAsync("set-state", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, ConnectionManager.State.scene, !ConnectionManager.State.synced);
        }
        public void Pause()
        {
            SocketManager.EmitAsync("pause-game", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, !isPaused);
        }
        public async void SelectImage()
        {
            await ImageTask((bytes) =>
            {
                if (bytes != null) SocketManager.EmitAsync("change-landing-page", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error mesage
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Convert.ToBase64String(bytes)); ;
            });
        }
        private async Task ImageTask(Action<byte[]> callback)
        {
            // Only allow image files
            ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg", "webp") };

            // Open file explorer
            StandaloneFileBrowser.OpenFilePanelAsync("Select file", "", extensions, false, (string[] paths) =>
            {
                // Return if no items are selected
                if (paths.Length == 0) callback(null);

                // Read bytes from selected file
                callback(File.ReadAllBytes(paths[0]));
            });
            await Task.Yield();
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