using System.Collections.Generic;
using System.Linq;
using Networking;
using UnityEngine;

namespace RPG
{
    public class JournalManager : MonoBehaviour
    {
        [SerializeField] private MasterPanel masterPanel;
        [SerializeField] private JournalPanel panelPrefab;

        private Dictionary<string, JournalPanel> openPanels = new Dictionary<string, JournalPanel>();

        public void ShowJournal(JournalData data)
        {
            if (openPanels.ContainsKey(data.id))
            {
                openPanels[data.id].Show();
            }
            else
            {
                var panel = Instantiate(panelPrefab, GameObject.Find("Main Canvas").transform);
                panel.LoadData(data, this);
                panel.GetComponent<RectTransform>().sizeDelta = new Vector2(600.0f, 750.0f);
                panel.GetComponent<RectTransform>().anchoredPosition = new Vector2(660.0f, -165.0f);
                panel.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                panel.transform.SetAsLastSibling();
                openPanels.Add(data.id, panel);
            }
        }
        public void CloseJournal(JournalPanel panel)
        {
            if (openPanels.ContainsValue(panel)) openPanels.Remove(panel.Data.id);
            Destroy(panel.gameObject);
        }
        public void RemoveJournal(string id)
        {
            masterPanel.RemoveJournal(id);

            if (openPanels.ContainsKey(id))
            {
                Destroy(openPanels[id].gameObject);
                openPanels.Remove(id);
            }
        }
        public void ModifyText(string id, string text)
        {
            masterPanel.UpdateJournalText(id, text);

            if (openPanels.ContainsKey(id)) openPanels[id].UpdateText(text);
        }
        public void ModifyHeader(string id, string text)
        {
            masterPanel.UpdateJournalHeader(id, text);

            if (openPanels.ContainsKey(id)) openPanels[id].UpdateHeader(text);
        }
        public void ModifyImage(string id, string text)
        {
            masterPanel.UpdateJournalImage(id, text);

            if (openPanels.ContainsKey(id)) openPanels[id].UpdateImage(text);
        }
        public void SetCollaborators(string id, List<Collaborator> collaborators)
        {
            var collaborator = collaborators.FirstOrDefault(x => x.user == SocketManager.UserId);
            if (collaborator.user != SocketManager.UserId) RemoveJournal(id);
            else
            {
                if (!collaborator.isCollaborator) RemoveJournal(id);
                else masterPanel.LoadJournal(id, "shared");
            }
        }
    }
}