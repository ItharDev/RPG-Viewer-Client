using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Networking;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG
{
    public class MasterPanel : MonoBehaviour
    {
        [Header("Blueprints")]
        public GameObject BlueprintPanel;
        public Transform BlueprintList;
        [SerializeField] private TMP_InputField blueprintSearch;
        [SerializeField] private TokenConfiguration blueprintConfiguration;

        [SerializeField] private BlueprintHolder blueprintHolder;
        [SerializeField] private FolderBlueprint blueprintFolder;

        [SerializeField] private List<BlueprintHolder> blueprints = new List<BlueprintHolder>();
        [SerializeField] private List<FolderBlueprint> blueprintFolders = new List<FolderBlueprint>();

        [Header("Scenes")]
        public GameObject ScenePanel;
        public Transform SceneList;
        [SerializeField] private TMP_InputField sceneSearch;
        [SerializeField] private GameObject createScenePanel;

        [SerializeField] private SceneHolder sceneHolder;
        [SerializeField] private FolderScene sceneFolder;

        [SerializeField] private List<SceneHolder> scenes = new List<SceneHolder>();
        [SerializeField] private List<FolderScene> sceneFolders = new List<FolderScene>();

        [Header("Journal")]
        public GameObject JournalPanel;
        public Transform JournalList;
        [SerializeField] private TMP_InputField journalSearch;
        [SerializeField] private GameObject createJournalPanel;
        [SerializeField] private JournalManager journalManager;

        [SerializeField] private JournalHolder journalHolder;
        [SerializeField] private FolderJournal journalFolder;

        [SerializeField] private List<JournalHolder> journals = new List<JournalHolder>();
        [SerializeField] private List<FolderJournal> journalFolders = new List<FolderJournal>();

        [Header("Buttons")]
        [SerializeField] private GameObject toggleButton;
        [SerializeField] private GameObject tokensButton;
        [SerializeField] private GameObject scenesButton;
        [SerializeField] private GameObject journalButton;

        [Header("Panels")]
        [SerializeField] private FolderPanel folderPanel;
        [SerializeField] private GameObject confirmPanel;
        [SerializeField] private GameObject lastOpen;

        private bool toggle;
        private bool loaded;

        private string scenePath;
        private string journalPath;
        private byte[] bytes;

        private Action<bool> confirmCallback;

        private void Update()
        {
            if (!loaded && SessionManager.Session != null)
            {
                loaded = true;

                LoadJournals();

                if (!SessionManager.IsMaster)
                {
                    tokensButton.SetActive(false);
                    scenesButton.SetActive(false);
                    lastOpen = JournalPanel;
                }
                else
                {
                    lastOpen = BlueprintPanel;
                    LoadBlueprints();
                    LoadScenes();
                }
            }
        }

        #region State
        public async void ChangeState()
        {
            await SocketManager.Socket.EmitAsync("change-state", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        #endregion

        #region Blueprints
        private async void LoadBlueprints()
        {
            await SocketManager.Socket.EmitAsync("get-blueprints", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    var directories = callback.GetValue(1).EnumerateArray().ToArray();
                    for (int i = 0; i < directories.Length; i++)
                    {
                        LoadBlueprintDirectory(directories[i], "");
                    }
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }
            });
        }
        private void LoadBlueprintDirectory(System.Text.Json.JsonElement json, string path)
        {
            if (json.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                LoadBlueprint(json.GetString(), path);
                return;
            }
            var id = json.GetProperty("id").GetString();
            var name = json.GetProperty("name").GetString();
            var subFolders = json.GetProperty("subFolders").EnumerateArray().ToArray();
            var contents = json.GetProperty("contents").EnumerateArray().ToArray();

            HandleFolderBlueprintAdded(string.IsNullOrEmpty(path) ? id : $"{path}/{id}", name);

            for (int i = 0; i < subFolders.Length; i++)
            {
                LoadBlueprintDirectory(subFolders[i], string.IsNullOrEmpty(path) ? id : $"{path}/{id}");
            }

            for (int i = 0; i < contents.Length; i++)
            {
                LoadBlueprint(contents[i].GetString(), string.IsNullOrEmpty(path) ? id : $"{path}/{id}");
            }
        }
        private async void LoadBlueprint(string id, string path)
        {
            await SocketManager.Socket.EmitAsync("get-blueprint", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    var data = JsonUtility.FromJson<TokenData>(callback.GetValue(1).ToString());
                    data.id = id;
                    var bp = HandleBlueprintAdded(default, id, path, null);

                    WebManager.Download(data.image, true, async (bytes) =>
                    {
                        await UniTask.SwitchToMainThread();
                        bp.LoadData(data, id, path, bytes, this);
                    });
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }

        public async void AddBlueprint(TokenData data, string path, byte[] image)
        {
            MessageManager.QueueMessage("Creating blueprint");

            data.permissions = new List<Permission>();
            for (int i = 0; i < SessionManager.Users.Count; i++)
            {
                data.permissions.Add(new Permission()
                {
                    user = SessionManager.Users[i],
                    permission = PermissionType.None
                });
            }

            await SocketManager.Socket.EmitAsync("create-blueprint", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    data.image = callback.GetValue(1).GetProperty("image").GetString();
                    data.id = callback.GetValue(1).GetProperty("_id").GetString();

                    HandleBlueprintAdded(data, data.id, path, image);
                    MessageManager.QueueMessage("Blueprint created successfully");
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, path, JsonUtility.ToJson(data), Convert.ToBase64String(image));
        }
        public async void DeleteBlueprint(BlueprintHolder blueprint, string path)
        {
            await SocketManager.Socket.EmitAsync("remove-blueprint", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Blueprint deleted successfully");
                    blueprints.Remove(blueprint);
                    Destroy(blueprint.gameObject);
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }

            }, blueprint.Id, path);
        }
        public void MoveBlueprint(BlueprintHolder bp, string newPath)
        {
            FolderBlueprint newF = blueprintFolders.FirstOrDefault(x => x.Path == newPath);

            bp.transform.SetParent(newF == null ? BlueprintList : newF.Content);
            bp.transform.SetAsLastSibling();
        }

        public void OpenBlueprintConfig()
        {
            blueprintConfiguration.gameObject.SetActive(true);
            blueprintConfiguration.LoadData(default, this, "");
        }
        public void OpenBlueprintConfig(TokenData data, string path)
        {
            blueprintConfiguration.gameObject.SetActive(true);
            blueprintConfiguration.LoadData(data, this, path);
        }
        public void OpenBlueprintConfig(TokenData data, BlueprintHolder blueprint, string path, byte[] image)
        {
            blueprintConfiguration.gameObject.SetActive(true);
            blueprintConfiguration.LoadData(data, blueprint, path, image);
        }


        public void OpenBlueprintFolder()
        {
            folderPanel.gameObject.SetActive(true);
            folderPanel.LoadData("", FolderType.Blueprint);
        }
        public void OpenBlueprintFolder(string path)
        {
            folderPanel.gameObject.SetActive(true);
            folderPanel.LoadData(path, FolderType.Blueprint);
        }

        public async void AddBlueprintFolder(string path, string name)
        {
            await SocketManager.Socket.EmitAsync("create-blueprint-folder", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    var dest = path == "" ? $"{callback.GetValue(1).GetString()}" : $"{path}/{callback.GetValue(1).GetString()}";
                    HandleFolderBlueprintAdded(dest, name);
                }
            }, path, name);
        }

        public async void RemoveBlueprintFolder(string path)
        {
            await SocketManager.Socket.EmitAsync("remove-blueprint-folder", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean()) HandleFolderBlueprintRemoved(path);
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, path);
        }

        private void HandleFolderBlueprintAdded(string path, string name)
        {
            var split = path.Split("/").ToList();
            split.RemoveAt(split.Count - 1);

            var parent = "";
            for (int i = 0; i < split.Count; i++)
            {
                if (i == 0) parent += split[i];
                else parent += $"/{split[i]}";
            }

            if (split.Count < 1)
            {
                var bpFolder = Instantiate(blueprintFolder, BlueprintList);
                bpFolder.LoadData(name, path, this);
                blueprintFolders.Add(bpFolder);
                bpFolder.transform.SetAsFirstSibling();
            }
            else
            {
                var bpFolder = Instantiate(blueprintFolder, blueprintFolders.FirstOrDefault(x => x.Path == parent).Content);
                bpFolder.LoadData(name, path, this);
                blueprintFolders.Add(bpFolder);
                bpFolder.transform.SetAsFirstSibling();
            }
        }

        private void HandleFolderBlueprintRemoved(string path)
        {
            var folder = blueprintFolders.FirstOrDefault(x => x.Path == path);
            if (folder != null)
            {
                var bps = folder.GetComponentsInChildren<BlueprintHolder>(true);
                var folders = folder.GetComponentsInChildren<FolderBlueprint>(true);
                for (int i = 0; i < bps.Length; i++)
                {
                    blueprints.Remove(bps[i]);
                    Destroy(bps[i].gameObject);
                }
                for (int i = 0; i < folders.Length; i++)
                {
                    blueprintFolders.Remove(folders[i]);
                    Destroy(folders[i].gameObject);
                }
                blueprintFolders.Remove(folder);
                Destroy(folder.gameObject);
            }
        }

        private BlueprintHolder HandleBlueprintAdded(TokenData data, string id, string path, byte[] image)
        {
            BlueprintHolder bp;

            if (string.IsNullOrEmpty(path))
            {
                bp = Instantiate(blueprintHolder, BlueprintList);
                if (image != null) bp.LoadData(data, id, path, image, this);
                bp.transform.SetAsLastSibling();
                blueprints.Add(bp);
            }
            else
            {
                bp = Instantiate(blueprintHolder, BlueprintList);
                if (image != null) bp.LoadData(data, id, path, image, this);
                var f = blueprintFolders.FirstOrDefault(x => x.Path == path);
                blueprints.Add(bp);

                if (f != null) bp.transform.SetParent(f.Content);
                bp.transform.SetAsLastSibling();
            }

            return bp;
        }
        #endregion

        #region Scenes
        private async void LoadScenes()
        {
            await SocketManager.Socket.EmitAsync("get-scenes", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    var directories = callback.GetValue(1).EnumerateArray().ToArray();
                    for (int i = 0; i < directories.Length; i++)
                    {
                        LoadSceneDirectory(directories[i], "");
                    }
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }
            });
        }
        private void LoadSceneDirectory(System.Text.Json.JsonElement json, string path)
        {
            if (json.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                LoadScene(json.GetString(), path);
                return;
            }
            var id = json.GetProperty("id").GetString();
            var name = json.GetProperty("name").GetString();
            var contents = json.GetProperty("contents").EnumerateArray().ToArray();
            var subFolders = json.GetProperty("subFolders").EnumerateArray().ToArray();

            HandleFolderSceneAdded(string.IsNullOrEmpty(path) ? id : $"{path}/{id}", name);

            for (int i = 0; i < subFolders.Length; i++)
            {
                LoadSceneDirectory(subFolders[i], string.IsNullOrEmpty(path) ? id : $"{path}/{id}");
            }

            for (int i = 0; i < contents.Length; i++)
            {
                LoadScene(contents[i].GetString(), string.IsNullOrEmpty(path) ? id : $"{path}/{id}");
            }
        }
        private async void LoadScene(string id, string path)
        {
            await SocketManager.Socket.EmitAsync("get-scene", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    var data = JsonUtility.FromJson<SceneSettings>(callback.GetValue(1).ToString());
                    data.id = callback.GetValue(2).GetString();
                    WebManager.Download(data.data.image, true, async (bytes) =>
                    {
                        await UniTask.SwitchToMainThread();
                        HandleSceneAdded(data, id, path, bytes);
                    });
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }

        public async void DeleteScene(SceneHolder scene, string path)
        {
            await SocketManager.Socket.EmitAsync("remove-scene", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Scene deleted successfully");
                    scenes.Remove(scene);
                    Destroy(scene.gameObject);

                    if (callback.GetValue(1).GetBoolean()) SessionManager.Session.UnloadScene();
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }

            }, scene.Id, path);
        }
        public async void RemoveSceneFolder(string path)
        {
            await SocketManager.Socket.EmitAsync("remove-scene-folder", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    HandleFolderSceneRemoved(path);
                    if (callback.GetValue(1).GetBoolean()) SessionManager.Session.UnloadScene();
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, path);
        }
        public void MoveScene(SceneHolder scene, string newPath)
        {
            FolderScene newF = sceneFolders.FirstOrDefault(x => x.Path == newPath);

            scene.transform.SetParent(newF == null ? SceneList : newF.Content);
            scene.transform.SetAsLastSibling();
        }

        public void OpenSceneCreation(string path)
        {
            createScenePanel.SetActive(true);
            createScenePanel.GetComponentInChildren<TMP_InputField>().text = "";
            scenePath = path;
        }
        public async void CreateScene()
        {
            if (bytes == null)
            {
                MessageManager.QueueMessage("Image not selected");
                return;
            }
            string name = createScenePanel.GetComponentInChildren<TMP_InputField>().text;
            Texture2D texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            var cellSize = texture.width * 0.0004f;
            var rows = Mathf.RoundToInt(texture.height * 0.01f / cellSize);

            SocketManager.SceneSettings = new SceneSettings()
            {
                path = scenePath,
                bytes = bytes,

                data = new SceneData() { name = name },
                grid = new GridData()
                {
                    dimensions = new Vector2Int(25, rows),
                    cellSize = cellSize,
                    position = new Vector2(-(texture.width * 0.005f), -(texture.height * 0.005f))
                },
                fogOfWar = new FogOfWarData(),
                walls = new List<WallData>(),
                tokens = new List<string>()
            };

            scenePath = null;
            bytes = null;
            await SocketManager.Socket.EmitAsync("set-scene", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, "");
            SceneManager.LoadScene("Scene");
        }

        public void OpenSceneFolder()
        {
            folderPanel.gameObject.SetActive(true);
            folderPanel.LoadData("", FolderType.Scene);
        }
        public void OpenSceneFolder(string path)
        {
            folderPanel.gameObject.SetActive(true);
            folderPanel.LoadData(path, FolderType.Scene);
        }

        public async void AddSceneFolder(string path, string name)
        {
            await SocketManager.Socket.EmitAsync("create-scene-folder", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    var dest = path == "" ? $"{callback.GetValue(1).GetString()}" : $"{path}/{callback.GetValue(1).GetString()}";
                    HandleFolderSceneAdded(dest, name);
                }
            }, path, name);
        }

        private void HandleFolderSceneAdded(string path, string name)
        {
            var split = path.Split("/").ToList();
            split.RemoveAt(split.Count - 1);

            var parent = "";
            for (int i = 0; i < split.Count; i++)
            {
                if (i == 0) parent += split[i];
                else parent += $"/{split[i]}";
            }

            if (split.Count < 1)
            {
                var sceneFolder = Instantiate(this.sceneFolder, SceneList);
                sceneFolder.LoadData(name, path, this);
                sceneFolders.Add(sceneFolder);
                sceneFolder.transform.SetAsFirstSibling();
            }
            else
            {
                var sceneFolder = Instantiate(this.sceneFolder, sceneFolders.FirstOrDefault(x => x.Path == parent).Content);
                sceneFolder.LoadData(name, path, this);
                sceneFolders.Add(sceneFolder);
                sceneFolder.transform.SetAsFirstSibling();
            }
        }

        private void HandleFolderSceneRemoved(string path)
        {
            var folder = sceneFolders.FirstOrDefault(x => x.Path == path);
            if (folder != null)
            {
                var scns = folder.GetComponentsInChildren<SceneHolder>(true);
                var folders = folder.GetComponentsInChildren<FolderScene>(true);
                for (int i = 0; i < scns.Length; i++)
                {
                    scenes.Remove(scns[i]);
                    Destroy(scns[i].gameObject);
                }
                for (int i = 0; i < folders.Length; i++)
                {
                    sceneFolders.Remove(folders[i]);
                    Destroy(folders[i].gameObject);
                }
                sceneFolders.Remove(folder);
                Destroy(folder.gameObject);
            }
        }

        private void HandleSceneAdded(SceneSettings data, string id, string path, byte[] image)
        {
            if (string.IsNullOrEmpty(path))
            {
                SceneHolder scene = Instantiate(sceneHolder, SceneList);
                scene.LoadData(data, id, path, image, this);
                scene.transform.SetAsLastSibling();
                scenes.Add(scene);
            }
            else
            {
                SceneHolder scene = Instantiate(sceneHolder, SceneList);
                scene.LoadData(data, id, path, image, this);
                var f = sceneFolders.FirstOrDefault(x => x.Path == path);
                scenes.Add(scene);

                if (f != null) scene.transform.SetParent(f.Content);
                scene.transform.SetAsLastSibling();
            }
        }
        #endregion

        #region Journal
        private async void LoadJournals()
        {
            await SocketManager.Socket.EmitAsync("get-journals", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    var directories = callback.GetValue(1).EnumerateArray().ToArray();
                    for (int i = 0; i < directories.Length; i++)
                    {
                        LoadJournalDirectory(directories[i], "");
                    }
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }
            });
        }
        private void LoadJournalDirectory(System.Text.Json.JsonElement json, string path)
        {
            if (json.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                LoadJournal(json.GetString(), path);
                return;
            }
            var id = json.GetProperty("id").GetString();
            var name = id == "shared" ? "Shared" : json.GetProperty("name").GetString();
            var contents = json.GetProperty("contents").EnumerateArray().ToArray();
            var subFolders = json.GetProperty("subFolders").EnumerateArray().ToArray();

            HandleFolderJournalAdded(string.IsNullOrEmpty(path) ? id : $"{path}/{id}", name);

            for (int i = 0; i < subFolders.Length; i++)
            {
                LoadJournalDirectory(subFolders[i], string.IsNullOrEmpty(path) ? id : $"{path}/{id}");
            }

            for (int i = 0; i < contents.Length; i++)
            {
                LoadJournal(contents[i].GetString(), string.IsNullOrEmpty(path) ? id : $"{path}/{id}");
            }
        }
        public async void LoadJournal(string id, string path)
        {
            await SocketManager.Socket.EmitAsync("get-journal", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    var data = JsonUtility.FromJson<JournalData>(callback.GetValue(1).ToString());
                    data.id = id;
                    HandleJournalAdded(data, id, path);
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, id);
        }
        public void UpdateJournalText(string id, string text)
        {
            var journal = journals.FirstOrDefault(x => x.Id == id);
            if (journal == null) return;

            journal.UpdateText(text);
        }
        public void UpdateJournalHeader(string id, string text)
        {
            var journal = journals.FirstOrDefault(x => x.Id == id);
            if (journal == null) return;

            journal.UpdateHeader(text);
        }
        public void UpdateJournalImage(string id, string text)
        {
            var journal = journals.FirstOrDefault(x => x.Id == id);
            if (journal == null) return;

            journal.UpdateImage(text);
        }
        public void RemoveJournal(string id)
        {
            var journal = journals.FirstOrDefault(x => x.Id == id);
            if (journal == null) return;

            journals.Remove(journal);
            Destroy(journal.gameObject);
        }

        public async void DeleteJournal(JournalHolder journal, string path)
        {
            await SocketManager.Socket.EmitAsync("remove-journal", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    MessageManager.QueueMessage("Journal deleted successfully");
                    journals.Remove(journal);
                    Destroy(journal.gameObject);
                }
                else
                {
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }

            }, journal.Id, path);
        }
        public async void RemoveJournalFolder(string path)
        {
            await SocketManager.Socket.EmitAsync("remove-journal-folder", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    HandleFolderJournalRemoved(path);
                    if (callback.GetValue(1).GetBoolean()) SessionManager.Session.UnloadScene();
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, path);
        }
        public void MoveJournal(JournalHolder journal, string newPath)
        {
            FolderJournal newF = journalFolders.FirstOrDefault(x => x.Path == newPath);

            journal.transform.SetParent(newF == null ? JournalList : newF.Content);
            journal.transform.SetAsLastSibling();
        }

        public void OpenJournalCreation(string path)
        {
            createJournalPanel.SetActive(true);
            createJournalPanel.GetComponentInChildren<TMP_InputField>().text = "";
            journalPath = path;
        }
        public async void CreateJournal()
        {
            string name = createJournalPanel.GetComponentInChildren<TMP_InputField>().text;
            JournalData data = new JournalData()
            {
                owner = "",
                header = name,
                text = "",
                image = "",
            };

            data.collaborators = new List<Collaborator>();
            for (int i = 0; i < SessionManager.Users.Count; i++)
            {
                if (SessionManager.Users[i] == SocketManager.UserId) continue;

                data.collaborators.Add(new Collaborator()
                {
                    user = SessionManager.Users[i],
                    isCollaborator = false
                });
            }
            if (!SessionManager.IsMaster)
            {
                data.collaborators.Add(new Collaborator()
                {
                    user = SessionManager.MasterId,
                    isCollaborator = false
                });
            }

            await SocketManager.Socket.EmitAsync("create-journal", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (callback.GetValue().GetBoolean())
                {
                    data.id = callback.GetValue(1).GetProperty("_id").GetString();
                    data.owner = SocketManager.UserId;

                    HandleJournalAdded(data, data.id, journalPath);
                    MessageManager.QueueMessage("Journal page created successfully");
                }
                else MessageManager.QueueMessage(callback.GetValue(1).GetString());

                journalPath = "";
            }, journalPath, JsonUtility.ToJson(data));

        }

        public void OpenJournalFolder()
        {
            folderPanel.gameObject.SetActive(true);
            folderPanel.LoadData("", FolderType.Journal);
        }
        public void OpenJournalFolder(string path)
        {
            folderPanel.gameObject.SetActive(true);
            folderPanel.LoadData(path, FolderType.Journal);
        }

        public async void AddJournalFolder(string path, string name)
        {
            await SocketManager.Socket.EmitAsync("create-journal-folder", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    var dest = path == "" ? $"{callback.GetValue(1).GetString()}" : $"{path}/{callback.GetValue(1).GetString()}";
                    HandleFolderJournalAdded(dest, name);
                }
            }, path, name);
        }

        private void HandleFolderJournalAdded(string path, string name)
        {
            var split = path.Split("/").ToList();
            split.RemoveAt(split.Count - 1);

            var parent = "";
            for (int i = 0; i < split.Count; i++)
            {
                if (i == 0) parent += split[i];
                else parent += $"/{split[i]}";
            }

            if (split.Count < 1)
            {
                var journalFolder = Instantiate(this.journalFolder, JournalList);
                journalFolder.LoadData(name, path, this);
                journalFolders.Add(journalFolder);
                journalFolder.transform.SetAsFirstSibling();
            }
            else
            {
                var journalFolder = Instantiate(this.journalFolder, journalFolders.FirstOrDefault(x => x.Path == parent).Content);
                journalFolder.LoadData(name, path, this);
                journalFolders.Add(journalFolder);
                journalFolder.transform.SetAsFirstSibling();
            }
        }

        private void HandleFolderJournalRemoved(string path)
        {
            var folder = journalFolders.FirstOrDefault(x => x.Path == path);
            if (folder != null)
            {
                var jrnals = folder.GetComponentsInChildren<JournalHolder>(true);
                var folders = folder.GetComponentsInChildren<FolderJournal>(true);
                for (int i = 0; i < jrnals.Length; i++)
                {
                    journals.Remove(jrnals[i]);
                    Destroy(jrnals[i].gameObject);
                }
                for (int i = 0; i < folders.Length; i++)
                {
                    journalFolders.Remove(folders[i]);
                    Destroy(folders[i].gameObject);
                }
                journalFolders.Remove(folder);
                Destroy(folder.gameObject);
            }
        }

        private void HandleJournalAdded(JournalData data, string id, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                JournalHolder journal = Instantiate(journalHolder, JournalList);
                journal.LoadData(data, id, path, this, journalManager);
                journal.transform.SetAsLastSibling();
                journals.Add(journal);
            }
            else
            {
                JournalHolder journal = Instantiate(journalHolder, JournalList);
                journal.LoadData(data, id, path, this, journalManager);
                var f = journalFolders.FirstOrDefault(x => x.Path == path);
                journals.Add(journal);

                if (f != null) journal.transform.SetParent(f.Content);
                journal.transform.SetAsLastSibling();
            }
        }
        #endregion

        #region Images
        public async void SelectImage() => await ImageTask();
        private async Task ImageTask()
        {
            var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };
            StandaloneFileBrowser.OpenFilePanelAsync("Select file", "", extensions, false, (string[] paths) =>
            {
                if (paths.Length == 0) return;
                bytes = File.ReadAllBytes(paths[0]);
            });
            await Task.Yield();
        }
        #endregion

        #region Panels
        public void ConfirmDeletion(Action<bool> callback)
        {
            confirmCallback = callback;
            confirmPanel.SetActive(true);
        }

        public void Delete()
        {
            confirmCallback(true);
            confirmCallback = null;
            confirmPanel.SetActive(false);
        }
        public void Cancel()
        {
            confirmCallback(false);
            confirmCallback = null;
            confirmPanel.SetActive(false);
        }

        private void RotateToggle()
        {
            float scale = toggle ? 1 : -1;
            LeanTween.scaleY(toggleButton, scale, 0.15f);
            toggle = !toggle;
        }

        public void ToggleTokens()
        {
            if (lastOpen == BlueprintPanel || !lastOpen.activeInHierarchy) RotateToggle();

            ScenePanel.SetActive(false);
            JournalPanel.SetActive(false);
            if (BlueprintPanel.activeInHierarchy)
            {
                LeanTween.size(GetComponent<RectTransform>(), new Vector2(150, 0), 0.15f);
                BlueprintPanel.SetActive(false);
            }
            else
            {
                BlueprintPanel.SetActive(true);
                LeanTween.size(GetComponent<RectTransform>(), new Vector2(300, 1030), 0.15f);
            }

            lastOpen = BlueprintPanel;
        }
        public void ToggleScenes()
        {
            if (lastOpen == ScenePanel || !lastOpen.activeInHierarchy) RotateToggle();

            BlueprintPanel.SetActive(false);
            JournalPanel.SetActive(false);
            if (ScenePanel.activeInHierarchy)
            {
                LeanTween.size(GetComponent<RectTransform>(), new Vector2(150, 0), 0.15f);
                ScenePanel.SetActive(false);
            }
            else
            {
                ScenePanel.SetActive(true);
                LeanTween.size(GetComponent<RectTransform>(), new Vector2(300, 1030), 0.15f);
            }

            lastOpen = ScenePanel;
        }
        public void ToggleJournals()
        {
            if (lastOpen == JournalPanel || !lastOpen.activeInHierarchy) RotateToggle();

            BlueprintPanel.SetActive(false);
            ScenePanel.SetActive(false);
            if (JournalPanel.activeInHierarchy)
            {
                LeanTween.size(GetComponent<RectTransform>(), new Vector2(150, 0), 0.15f);
                JournalPanel.SetActive(false);
            }
            else
            {
                JournalPanel.SetActive(true);
                LeanTween.size(GetComponent<RectTransform>(), new Vector2(300, 1030), 0.15f);
            }

            lastOpen = JournalPanel;
        }
        public void ToggleLast()
        {
            if (lastOpen.activeInHierarchy)
            {
                LeanTween.size(GetComponent<RectTransform>(), new Vector2(150, 0), 0.15f);
                lastOpen.SetActive(false);
            }
            else
            {
                lastOpen.SetActive(true);
                LeanTween.size(GetComponent<RectTransform>(), new Vector2(300, 1030), 0.15f);
            }

            RotateToggle();
        }
        #endregion

        #region Search
        public void HandleSearch(string type)
        {
            switch (type)
            {
                case "Blueprints":
                    for (int i = 0; i < blueprints.Count; i++)
                    {
                        blueprints[i].gameObject.SetActive(blueprints[i].Data.name.ToLower().Contains(blueprintSearch.text.ToLower()));
                    }
                    for (int i = 0; i < blueprintFolders.Count; i++)
                    {
                        var active = blueprintFolders[i].GetComponentsInChildren<BlueprintHolder>(false);
                        var all = blueprintFolders[i].GetComponentsInChildren<BlueprintHolder>(true);
                        if (all.Length >= 1)
                        {
                            blueprintFolders[i].gameObject.SetActive(active.Length > 0);
                        }
                        else blueprintFolders[i].gameObject.SetActive(true);
                    }
                    break;
                case "Scenes":
                    for (int i = 0; i < scenes.Count; i++)
                    {
                        scenes[i].gameObject.SetActive(scenes[i].Data.data.name.ToLower().Contains(sceneSearch.text.ToLower()));
                    }
                    for (int i = 0; i < sceneFolders.Count; i++)
                    {
                        var active = sceneFolders[i].GetComponentsInChildren<SceneHolder>(false);
                        var all = sceneFolders[i].GetComponentsInChildren<SceneHolder>(true);
                        if (all.Length >= 1)
                        {
                            sceneFolders[i].gameObject.SetActive(active.Length > 0);
                        }
                        else sceneFolders[i].gameObject.SetActive(true);
                    }
                    break;
                case "Journals":
                    for (int i = 0; i < journals.Count; i++)
                    {
                        journals[i].gameObject.SetActive(journals[i].Data.header.ToLower().Contains(journalSearch.text.ToLower()));
                    }
                    for (int i = 0; i < journalFolders.Count; i++)
                    {
                        var active = journalFolders[i].GetComponentsInChildren<JournalHolder>(false);
                        var all = journalFolders[i].GetComponentsInChildren<JournalHolder>(true);
                        if (all.Length >= 1)
                        {
                            journalFolders[i].gameObject.SetActive(active.Length > 0);
                        }
                        else journalFolders[i].gameObject.SetActive(true);
                    }
                    break;
            }
        }
        #endregion
    }
}
