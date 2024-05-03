using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class JournalsPanel : MonoBehaviour
    {
        [SerializeField] private JournalFolder folderPrefab;
        [SerializeField] private JournalHolder journalPrefab;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private CollaboratorPanel collaboratorPanel;

        private Dictionary<string, JournalFolder> folders = new Dictionary<string, JournalFolder>();
        private Dictionary<string, JournalHolder> journals = new Dictionary<string, JournalHolder>();

        private bool loaded;
        private JournalFolder selectedFolder;
        private JournalHolder selectedJournal;
        private float lastColor;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnSidePanelChanged.AddListener(DeselectJournal);
            Events.OnJournalHeaderModified.AddListener(ModifyHeader);
            Events.OnJournalRemoved.AddListener(RemoveCollaboration);
            Events.OnCollaboratorsUpdated.AddListener(UpdateCollaboration);

            if (!loaded)
            {
                loaded = true;
                LoadJournals();
            }
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSidePanelChanged.RemoveListener(DeselectJournal);
            Events.OnJournalHeaderModified.RemoveListener(ModifyHeader);
            Events.OnJournalRemoved.RemoveListener(RemoveCollaboration);
            Events.OnCollaboratorsUpdated.RemoveListener(UpdateCollaboration);
        }

        private void ModifyHeader(string id, string text, string uid)
        {
            if (!journals.ContainsKey(id)) return;
            journals[id].UpdateHeader(text);
        }
        private void RemoveCollaboration(string id)
        {
            if (!journals.ContainsKey(id)) return;
            RemoveJournal(journals[id]);
        }
        private void UpdateCollaboration(string id, string owner, List<Collaborator> collaborators)
        {
            if (!journals.ContainsKey(id))
            {
                if (collaborators.FirstOrDefault(x => x.user == GameData.User.id).isCollaborator)
                {
                    // Check if the collaboration folder exists
                    JournalFolder target = GetDirectoryByPath($"shared/{owner}");
                    if (string.IsNullOrEmpty(target.Id))
                    {
                        SocketManager.EmitAsync("get-user", async (callback) =>
                        {
                            await UniTask.SwitchToMainThread();

                            // Check if the event was successful
                            if (callback.GetValue().GetBoolean())
                            {
                                // Load folder's data
                                string name = callback.GetValue().GetString();
                                Folder data = new Folder(owner, $"shared/{owner}", name, GetColor());

                                // Instantiate folder
                                JournalFolder targetFolder = GetDirectoryByPath("shared");
                                JournalFolder folder = Instantiate(folderPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
                                folder.LoadData(data, this, targetFolder == null ? SortContent : targetFolder.SortContent);

                                // Add folder to dictionary
                                this.folders.Add(owner, folder);
                                LoadJournal(id, $"shared/{owner}");
                                return;
                            }

                            // Send error message
                            MessageManager.QueueMessage(callback.GetValue(1).GetString());
                        }, owner);
                    }
                    else
                    {
                        LoadJournal(id, $"shared/{owner}");
                    }
                }
                return;
            }
            journals[id].UpdateCollaborators(collaborators);
        }

        private void SortContent()
        {
            List<JournalFolder> listOfFolders = folders.Values.ToList();
            List<JournalHolder> listOfJournals = journals.Values.ToList();

            listOfFolders.Sort(SortByName);
            listOfJournals.Sort(SortByName);

            for (int i = 0; i < folders.Count; i++)
            {
                listOfFolders[i].transform.SetSiblingIndex(i);
            }
            for (int i = 0; i < listOfJournals.Count; i++)
            {
                listOfJournals[i].transform.SetSiblingIndex(i + folders.Count);
            }
        }
        private int SortByName(JournalFolder folderA, JournalFolder folderB)
        {
            return folderA.Data.name.CompareTo(folderB.Data.name);
        }
        private int SortByName(JournalHolder holderA, JournalHolder holderB)
        {
            return holderA.Data.header.CompareTo(holderB.Data.header);
        }
        private void LoadJournals()
        {
            SocketManager.EmitAsync("get-journals", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Enumerate journals array
                    var folders = callback.GetValue(1).GetProperty("folders").EnumerateObject().ToArray();
                    var contents = callback.GetValue(1).GetProperty("contents").EnumerateArray().ToArray();

                    // Load folders
                    for (int i = 0; i < folders.Length; i++)
                    {
                        LoadDirectory(folders[i].Value, folders[i].Name, "");
                    }

                    // Load journals
                    for (int i = 0; i < contents.Length; i++)
                    {
                        LoadJournal(contents[i].GetString(), "");
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        public void LoadJournal(string id, string path)
        {
            // Find target folder
            JournalFolder targetFolder = GetDirectoryByPath(path);

            // Instantiate journal
            JournalHolder journal = Instantiate(journalPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            journal.transform.SetAsLastSibling();
            journals.Add(id, journal);
            journal.LoadData(id, path, this, targetFolder == null ? SortContent : targetFolder.SortContent);
        }
        private void LoadDirectory(System.Text.Json.JsonElement json, string id, string path)
        {
            // Load folder's data
            string name = json.GetProperty("name").GetString();
            var folders = json.GetProperty("folders").EnumerateObject().ToArray();
            var contents = json.GetProperty("contents").EnumerateArray().ToArray();

            // Create path to this directory
            string pathToThisFolder = string.IsNullOrEmpty(path) ? id : $"{path}/{id}";
            Folder data = new Folder(id, path, name, GetColor());

            // Instantiate folder
            JournalFolder targetFolder = GetDirectoryByPath(path);
            JournalFolder folder = Instantiate(folderPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            this.folders.Add(id, folder);
            folder.LoadData(data, this, targetFolder == null ? SortContent : targetFolder.SortContent);

            // Load folders
            for (int i = 0; i < folders.Length; i++)
            {
                LoadDirectory(folders[i].Value, folders[i].Name, pathToThisFolder);
            }

            // Load journals
            for (int i = 0; i < contents.Length; i++)
            {
                LoadJournal(contents[i].GetString(), pathToThisFolder);
            }
        }
        public Color GetColor()
        {
            // Generate random color with 13 variants
            float randomHue = GetRandomHue();

            // Prevent selecting the same color twice in a row
            while (randomHue == lastColor)
            {
                randomHue = GetRandomHue();
            }
            lastColor = randomHue;
            return Color.HSVToRGB(randomHue, 0.6f, 1.0f);
        }
        private float GetRandomHue()
        {
            return Random.Range(0, 12) * (1.0f / 12.0f);
        }

        public void CreateJournal(string path)
        {
            JournalData data = new JournalData("", "New page", GameData.User.id, "", "", new List<Collaborator>());
            SocketManager.EmitAsync("create-journal", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    string id = callback.GetValue(1).GetString();

                    LoadJournal(id, path);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, path, JsonUtility.ToJson(data));
        }

        public JournalFolder CreateFolder(string id, string path)
        {
            // Create path to this directory
            string pathToThisFolder = string.IsNullOrEmpty(path) ? id : $"{path}/{id}";
            Folder data = new Folder(id, path, "New folder", GetColor());

            // Instantiate folder
            JournalFolder targetFolder = GetDirectoryByPath(path);
            JournalFolder folder = Instantiate(folderPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            folders.Add(id, folder);
            folder.LoadData(data, this, targetFolder == null ? SortContent : targetFolder.SortContent);

            return folder;
        }
        public void RemoveFolder(JournalFolder folder)
        {
            List<JournalFolder> subFolders = GetFolders(folder);
            List<JournalHolder> journal = GetJournals(folder);

            for (int i = 0; i < subFolders.Count; i++)
            {
                RemoveFolder(subFolders[i]);
            }

            for (int i = 0; i < journal.Count; i++)
            {
                RemoveJournal(journal[i]);
            }

            folders.Remove(folder.Id);
            Destroy(folder.gameObject);
        }
        public void CreateFolder()
        {
            SocketManager.EmitAsync("create-journal-folder", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Create the folder
                    string id = callback.GetValue(1).GetString();
                    JournalFolder createdFolder = CreateFolder(id, "");

                    // Activate rename field
                    createdFolder.Rename();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, "", "New folder");
        }
        public void RemoveJournal(JournalHolder journal)
        {
            journals.Remove(journal.Id);
            Destroy(journal.gameObject);
        }
        public JournalFolder GetDirectoryByPath(string path)
        {
            // Find folder with specified path
            return folders.FirstOrDefault(item => item.Value.Path == path).Value;
        }
        public bool IsSubFolderOf(JournalFolder folderToCheck, JournalFolder parentFolder)
        {
            if (parentFolder == null || folderToCheck == null) return false;

            List<JournalFolder> subFolders = parentFolder.GetComponentsInChildren<JournalFolder>(true).ToList();
            subFolders.Remove(parentFolder);
            return subFolders.Contains(folderToCheck);
        }
        public List<JournalFolder> GetFolders(JournalFolder folder, bool deepSearch = false)
        {
            List<JournalFolder> listOfFolders = new List<JournalFolder>();
            JournalFolder[] folders = folder.Content.GetComponentsInChildren<JournalFolder>(false);
            for (int i = 0; i < folders.Length; i++)
            {
                if (deepSearch)
                {
                    listOfFolders.Add(folders[i]);
                    continue;
                }

                if (folders[i].transform.parent == folder.Content) listOfFolders.Add(folders[i]);
            }

            return listOfFolders;
        }
        public List<JournalHolder> GetJournals(JournalFolder folder, bool deepSearch = false)
        {
            List<JournalHolder> listOfJournals = new List<JournalHolder>();
            JournalHolder[] holders = folder.Content.GetComponentsInChildren<JournalHolder>(false);
            for (int i = 0; i < holders.Length; i++)
            {
                if (deepSearch)
                {
                    listOfJournals.Add(holders[i]);
                    continue;
                }

                if (holders[i].transform.parent == folder.Content) listOfJournals.Add(holders[i]);
            }

            return listOfJournals;
        }
        public void SelectFolder(JournalFolder folder)
        {
            // Deselect journal
            selectedJournal = null;

            // Store selected folder
            selectedFolder = folder;
            Events.OnJournalFolderSelected?.Invoke(folder);
            Events.OnJournalSelected?.Invoke(null);
        }
        public void MoveFolderRoot()
        {
            if (selectedFolder == null) return;

            SocketManager.EmitAsync("move-journal-folder", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    // Set new transform
                    selectedFolder.transform.SetParent(rootTransform);
                    selectedFolder.transform.SetAsFirstSibling();

                    // Calculate new path
                    selectedFolder.CalculatePath("");

                    Events.OnJournalFolderMoved?.Invoke();
                    selectedFolder = null;

                    SortContent();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                selectedFolder = null;
            }, selectedFolder.Path, "");
        }
        public void DeselectFolder()
        {
            // Send cancel event
            selectedFolder = null;
            Events.OnJournalFolderDeselected?.Invoke();
            Events.OnJournalDeselected?.Invoke();
        }
        public void MoveSelected(JournalFolder folder)
        {
            if (selectedFolder != null)
            {
                SocketManager.EmitAsync("move-journal-folder", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();

                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean())
                    {
                        // Set new transform
                        selectedFolder.transform.SetParent(folder.Content);
                        selectedFolder.transform.SetAsFirstSibling();

                        // Calculate new path
                        selectedFolder.CalculatePath(folder.Path);

                        Events.OnJournalFolderMoved?.Invoke();
                        selectedFolder = null;

                        folder.SortContent();
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    selectedFolder = null;
                }, selectedFolder.Path, folder.Path);
            }
            else
            {
                SocketManager.EmitAsync("move-journal", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();

                    if (callback.GetValue().GetBoolean())
                    {
                        // Set new transform
                        selectedJournal.transform.SetParent(folder.Content);
                        selectedJournal.transform.SetAsLastSibling();

                        // Calculate new path
                        selectedJournal.UpdatePath(folder.Path);

                        Events.OnJournalMoved?.Invoke();
                        selectedJournal = null;

                        folder.SortContent();
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    selectedJournal = null;
                }, selectedJournal.Id, selectedJournal.Path, folder.Path);
            }
        }

        public void SelectJournal(JournalHolder journal)
        {
            // Deselect folder
            selectedFolder = null;

            // Store selected journal
            selectedJournal = journal;
            Events.OnJournalSelected?.Invoke(journal);
            Events.OnJournalFolderSelected?.Invoke(null);
        }
        public void MoveJournalRoot()
        {
            if (selectedJournal == null) return;

            SocketManager.EmitAsync("move-journal", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    // Set new transform
                    selectedJournal.transform.SetParent(rootTransform);
                    selectedJournal.transform.SetAsLastSibling();

                    // Calculate new path
                    selectedJournal.UpdatePath("");

                    Events.OnJournalMoved?.Invoke();
                    selectedJournal = null;

                    SortContent();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                selectedJournal = null;
            }, selectedJournal.Id, selectedJournal.Path, "");
        }
        public void DeselectJournal()
        {
            // Send cancel event
            selectedJournal = null;
            Events.OnJournalDeselected?.Invoke();
            Events.OnJournalFolderDeselected?.Invoke();
        }

        public void SharePage(JournalData data)
        {
            collaboratorPanel.LoadData(data.collaborators, (collaborators) =>
            {
                JournalData newData = new JournalData(data.id, data.header, data.owner, data.text, data.image, collaborators);
                SocketManager.EmitAsync("share-journal", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, data.id, JsonUtility.ToJson(newData));
            });
        }
    }
}