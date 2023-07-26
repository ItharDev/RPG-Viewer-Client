using System.Collections.Generic;
using UnityEngine;

namespace RPG
{
    public class JournalManager : MonoBehaviour
    {
        [SerializeField] private Journal journalPrefab;

        private Dictionary<string, Journal> openJournals = new Dictionary<string, Journal>();

        private void OnEnable()
        {
            // Add event listeners
            Events.OnJournalTextModified.AddListener(ModifyText);
            Events.OnJournalHeaderModified.AddListener(ModifyHeader);
            Events.OnJournalImageModified.AddListener(ModifyImage);
            Events.OnJournalRemoved.AddListener(RemoveJournal);
            Events.OnJournalShowed.AddListener(ShowJournal);
            Events.OnCollaboratorsUpdated.AddListener(UpdateCollaborators);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnJournalTextModified.RemoveListener(ModifyText);
            Events.OnJournalHeaderModified.RemoveListener(ModifyHeader);
            Events.OnJournalImageModified.RemoveListener(ModifyImage);
            Events.OnJournalRemoved.RemoveListener(RemoveJournal);
            Events.OnJournalShowed.RemoveListener(ShowJournal);
            Events.OnCollaboratorsUpdated.RemoveListener(UpdateCollaborators);
        }

        private void ModifyText(string id, string text, string uid)
        {
            // Load new info
            if (openJournals.ContainsKey(id)) openJournals[id].ReloadText(text, uid != GameData.User.id);

        }
        private void ModifyHeader(string id, string text, string uid)
        {
            // Load new info
            if (openJournals.ContainsKey(id)) openJournals[id].ReloadHeader(text, uid != GameData.User.id);
        }
        private void ModifyImage(string id, string text)
        {
            // Load new info
            if (openJournals.ContainsKey(id)) openJournals[id].ReloadImage(text);
        }
        private void RemoveJournal(string id)
        {
            if (!openJournals.ContainsKey(id)) return;
            Destroy(openJournals[id]);
            RemoveJournal(id);
        }
        private void ShowJournal(string id)
        {
            OpenJournal(id);
        }
        private void UpdateCollaborators(string id, string owner, List<Collaborator> collaborators)
        {
            if (!openJournals.ContainsKey(id)) return;
            openJournals[id].UpdateCollaborators(collaborators);
        }

        public void OpenJournal(string id)
        {
            if (openJournals.ContainsKey(id)) return;

            Journal journal = Instantiate(journalPrefab);
            journal.transform.SetParent(UICanvas.Instance.transform);
            journal.transform.SetAsLastSibling();
            journal.transform.localPosition = Vector3.zero;

            journal.Instantiate(id);
            openJournals.Add(id, journal);
        }
        public void CloseJournal(string id)
        {
            if (!openJournals.ContainsKey(id)) return;
            openJournals.Remove(id);
        }
    }
}
