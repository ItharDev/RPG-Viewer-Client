using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class JournalHolder : MonoBehaviour
    {
        [SerializeField] private TMP_Text headerText;

        [Header("Options")]
        [SerializeField] private RectTransform optionsPanel;
        [SerializeField] private GameObject selectButton;
        [SerializeField] private GameObject rootButton;
        [SerializeField] private GameObject deselectButton;

        [Header("Colors")]
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor;

        public string Path { get { return _path; } }
        public string Id { get { return Data.id; } }

        public JournalData Data;

        private string _path;
        private JournalsPanel journalsPanel;
        private bool optionsOpen;
        private Color selectedColor;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnJournalClicked.AddListener(HandleClick);
            Events.OnJournalFolderClicked.AddListener(HandleClick);
            Events.OnJournalSelected.AddListener(HandleSelect);
            Events.OnJournalDeselected.AddListener(HandleDeselect);
            Events.OnJournalMoved.AddListener(HandleMoved);
            Events.OnSidePanelChanged.AddListener(CloseOptions);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnJournalClicked.RemoveListener(HandleClick);
            Events.OnJournalFolderClicked.RemoveListener(HandleClick);
            Events.OnJournalSelected.RemoveListener(HandleSelect);
            Events.OnJournalDeselected.RemoveListener(HandleDeselect);
            Events.OnJournalMoved.RemoveListener(HandleMoved);
            Events.OnSidePanelChanged.RemoveListener(CloseOptions);
        }

        private void HandleClick(JournalHolder journal)
        {
            // Close options panel if it's open and not ours
            if (optionsOpen && journal != this) ToggleOptions();
        }
        private void HandleClick(JournalFolder folder)
        {
            // Close options panel if it's open
            if (optionsOpen) ToggleOptions();
        }
        private void HandleSelect(JournalHolder journal)
        {
            // This journal was selected
            if (journal == this)
            {
                selectButton.SetActive(false);
                rootButton.SetActive(true);
                deselectButton.SetActive(true);

                background.color = selectedColor;

                return;
            }

            // Allow to select another journal
            rootButton.SetActive(false);
            selectButton.SetActive(true);
            deselectButton.SetActive(false);
            background.color = normalColor;
        }
        private void HandleDeselect()
        {
            // Allow journal selection
            selectButton.SetActive(true);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

            // Reset background color
            background.color = normalColor;
        }
        private void HandleMoved()
        {
            // Allow journal selection
            selectButton.SetActive(true);
            deselectButton.SetActive(false);
            rootButton.SetActive(false);

            // Reset background color
            background.color = normalColor;
        }

        public void ClickJournal(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;

            if (pointerData.button == PointerEventData.InputButton.Left) OpenJournal();
            else if (pointerData.button == PointerEventData.InputButton.Right) ToggleOptions();

            // Send journal toggled event
            Events.OnJournalClicked?.Invoke(this);
        }
        private void OpenJournal()
        {
            Session.Instance.JournalManager.OpenJournal(Id);
        }
        private void ToggleOptions()
        {
            // Prevent clicking shared journals
            if (Path.Contains("shared")) return;

            // Toggle open state
            optionsOpen = !optionsOpen;

            // Set panel on top before opening it
            if (optionsOpen)
            {
                optionsPanel.transform.SetParent(UICanvas.Instance.transform, true);
                optionsPanel.SetAsLastSibling();
            }

            // Calculate panel's target height
            float targetSize = 60.0f;
            if (selectButton.activeInHierarchy) targetSize += 30.0f;
            if (deselectButton.activeInHierarchy) targetSize += 30.0f;
            if (rootButton.activeInHierarchy) targetSize += 30.0f;

            LeanTween.size(optionsPanel, new Vector2(110.0f, optionsOpen ? targetSize : 0.0f), 0.2f).setOnComplete(() =>
            {
                // Set panel's transform to this after closing the panel
                if (!optionsOpen)
                {
                    optionsPanel.transform.SetParent(transform, true);
                    optionsPanel.anchoredPosition = new Vector2(15.0f, -35.0f);
                    optionsPanel.SetAsLastSibling();
                }

                // Enable / disable content size fitter
                optionsPanel.GetComponent<ContentSizeFitter>().enabled = optionsOpen;
            });
        }
        private void CloseOptions()
        {
            if (!optionsOpen) return;

            optionsOpen = false;

            LeanTween.size(optionsPanel, new Vector2(115.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                optionsPanel.transform.SetParent(transform, true);
                optionsPanel.anchoredPosition = new Vector2(15.0f, -45.0f);
                optionsPanel.SetAsLastSibling();

                // Enable / disable content size fitter
                optionsPanel.GetComponent<ContentSizeFitter>().enabled = false;
            });
        }

        public void Share()
        {
            journalsPanel.SharePage(Data);
        }
        public void Delete()
        {
            ToggleOptions();
            MessageManager.AskConfirmation(new Confirmation("Delete journal", "Delete", "Cancel", (result) =>
            {
                if (result) SocketManager.EmitAsync("remove-journal", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (callback.GetValue().GetBoolean())
                    {
                        journalsPanel.RemoveJournal(this);
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Path, Id);

            }));
        }
        public void Select()
        {
            ToggleOptions();
            journalsPanel.SelectJournal(this);
        }
        public void MoveRoot()
        {
            ToggleOptions();
            journalsPanel.MoveJournalRoot();
        }
        public void Deselect()
        {
            ToggleOptions();
            journalsPanel.DeselectJournal();
        }

        public void LoadData(string id, string path, JournalsPanel panel, Action onComplete)
        {
            Data = new JournalData
            {
                id = id,
                header = name
            };
            SocketManager.EmitAsync("get-journal", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Load Data
                    JournalData data = JsonUtility.FromJson<JournalData>(callback.GetValue(1).ToString());
                    data.id = id;
                    _path = path;
                    journalsPanel = panel;
                    selectedColor = string.IsNullOrEmpty(path) ? journalsPanel.GetColor() : journalsPanel.GetDirectoryByPath(path).Data.color;
                    selectedColor.a = 0.5f;
                    LoadData(data);
                    onComplete?.Invoke();
                    return;
                }

                panel.RemoveJournal(this);
            }, id);
        }
        public void UpdatePath(string newPath)
        {
            _path = newPath;
            selectedColor = string.IsNullOrEmpty(newPath) ? journalsPanel.GetColor() : journalsPanel.GetDirectoryByPath(newPath).Data.color;
        }

        private void LoadData(JournalData data)
        {
            Data = data;
            headerText.text = data.header;
        }

        public void UpdateHeader(string header)
        {
            Data.header = header;
            headerText.text = header;
            GetComponentInParent<JournalFolder>(true).SortContent();
        }
        public void UpdateCollaborators(List<Collaborator> collaborators)
        {
            Data.collaborators = collaborators;
            if (Data.IsOwner) return;

            if (!Data.IsCollaborator && Path.Contains("shared"))
            {
                journalsPanel.RemoveJournal(this);
            }
        }
    }
}