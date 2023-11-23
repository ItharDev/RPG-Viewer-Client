using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Networking;
using SFB;
using UnityEngine;

namespace RPG
{
    public class ScenesPanel : MonoBehaviour
    {
        [SerializeField] private SceneFolder folderPrefab;
        [SerializeField] private SceneHolder scenePrefab;
        [SerializeField] private Transform rootTransform;

        private Dictionary<string, SceneFolder> folders = new Dictionary<string, SceneFolder>();
        private Dictionary<string, SceneHolder> scenes = new Dictionary<string, SceneHolder>();

        private bool loaded;
        private SceneFolder selectedFolder;
        private SceneHolder selectedScene;
        private float lastCount;
        private float lastColor;

        private void OnEnable()
        {
            if (!loaded)
            {
                loaded = true;
                LoadScenes();
            }

            // Add event listeners
            Events.OnSidePanelChanged.AddListener(DeselectScene);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSidePanelChanged.RemoveListener(DeselectScene);
        }
        private void Update()
        {
            if (lastCount != rootTransform.childCount)
            {
                lastCount = rootTransform.childCount;
                if (lastCount > 1) SortContent();
            }
        }

        public void SortContent()
        {
            List<SceneFolder> listOfFolders = folders.Values.ToList();
            List<SceneHolder> listOfScenes = scenes.Values.ToList();

            listOfFolders.Sort(SortByName);
            listOfScenes.Sort(SortByName);

            for (int i = 0; i < folders.Count; i++)
            {
                listOfFolders[i].transform.SetSiblingIndex(i);
            }
            for (int i = 0; i < listOfScenes.Count; i++)
            {
                listOfScenes[i].transform.SetSiblingIndex(i + folders.Count);
            }
        }
        private int SortByName(SceneFolder folderA, SceneFolder folderB)
        {
            return folderA.Data.name.CompareTo(folderB.Data.name);
        }
        private int SortByName(SceneHolder holderA, SceneHolder holderB)
        {
            return holderA.Data.info.name.CompareTo(holderB.Data.info.name);
        }
        private void LoadScenes()
        {
            SocketManager.EmitAsync("get-scenes", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Load folders
                    System.Text.Json.JsonElement folders;
                    if (callback.GetValue(1).TryGetProperty("folders", out folders))
                    {
                        var list = folders.EnumerateObject().ToArray();
                        for (int i = 0; i < list.Length; i++)
                        {
                            LoadDirectory(list[i].Value, list[i].Name, "");
                        }
                    }

                    // Load scenes
                    System.Text.Json.JsonElement contents;
                    if (callback.GetValue(1).TryGetProperty("contents", out contents))
                    {
                        var list = contents.EnumerateArray().ToArray();
                        for (int i = 0; i < list.Length; i++)
                        {
                            LoadScene(list[i].GetString(), "");
                        }
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        private void LoadScene(string id, string path)
        {
            // Find target folder
            SceneFolder targetFolder = GetDirectoryByPath(path);

            // Instantiate scene
            SceneHolder scene = Instantiate(scenePrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            scene.transform.SetAsLastSibling();
            scene.LoadData(id, path, this, SortContent);

            // Add scene to dictionary
            scenes.Add(id, scene);
            SortContent();
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
            SceneFolder targetFolder = GetDirectoryByPath(path);
            SceneFolder folder = Instantiate(folderPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            folder.LoadData(data, this, SortContent);

            // Add folder to dictionary
            this.folders.Add(id, folder);

            // Load folders
            for (int i = 0; i < folders.Length; i++)
            {
                LoadDirectory(folders[i].Value, folders[i].Name, pathToThisFolder);
            }

            // Load tokens
            for (int i = 0; i < contents.Length; i++)
            {
                LoadScene(contents[i].GetString(), pathToThisFolder);
            }

            SortContent();
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
            return UnityEngine.Random.Range(0, 12) * (1.0f / 12.0f);
        }

        public async void CreateScene(string path)
        {
            await ImageTask(async (bytes) =>
            {
                if (bytes == null) return;

                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                float cellSize = texture.width * 0.0004f;
                Vector2 gridPosition = new Vector2(-(texture.width * 0.005f), -(texture.height * 0.005f));
                int rows = Mathf.RoundToInt(texture.height * 0.01f / cellSize);

                SceneInfo info = new SceneInfo("New Scene", "", 0.0f);
                GridData grid = new GridData(true, new Vector2Int(25, rows), cellSize, gridPosition, Color.black, new GridUnit("feet", 5));
                LightingSettings darkness = new LightingSettings(true, false, Color.black);
                SceneData data = new SceneData(info, grid, darkness);
                data.path = path;

                SocketManager.EmitAsync("create-scene", async (callback) =>
                {
                    if (callback.GetValue().GetBoolean())
                    {
                        await UniTask.SwitchToMainThread();
                        string id = callback.GetValue(1).GetString();

                        // Find target folder
                        SceneFolder targetFolder = GetDirectoryByPath(data.path);

                        // Instantiate scene
                        SceneHolder scene = Instantiate(scenePrefab, targetFolder == null ? rootTransform : targetFolder.Content);
                        scene.transform.SetAsLastSibling();
                        scene.LoadData(id, data.path, this, SortContent);

                        // Add scene to dictionary
                        scenes.Add(id, scene);
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, path, JsonUtility.ToJson(data), Convert.ToBase64String(bytes));
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
        public SceneFolder CreateFolder(string id, string path)
        {
            // Create path to this directory
            string pathToThisFolder = string.IsNullOrEmpty(path) ? id : $"{path}/{id}";
            Folder data = new Folder(id, path, "New folder", GetColor());

            // Instantiate folder
            SceneFolder targetFolder = GetDirectoryByPath(path);
            SceneFolder folder = Instantiate(folderPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            folder.LoadData(data, this, SortContent);

            // Add folder to dictionary
            this.folders.Add(id, folder);
            return folder;
        }
        public void RemoveFolder(SceneFolder folder)
        {
            List<SceneFolder> subFolders = GetFolders(folder);
            List<SceneHolder> scenes = GetScenes(folder);

            for (int i = 0; i < subFolders.Count; i++)
            {
                RemoveFolder(subFolders[i]);
            }

            for (int i = 0; i < scenes.Count; i++)
            {
                RemoveScene(scenes[i]);
            }

            folders.Remove(folder.Id);
            Destroy(folder.gameObject);
        }
        public void CreateFolder()
        {
            SocketManager.EmitAsync("create-scene-folder", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Create the folder
                    string id = callback.GetValue(1).GetString();
                    SceneFolder createdFolder = CreateFolder(id, "");

                    // Activate rename field
                    createdFolder.Rename();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, "", "New folder");
        }
        public void RemoveScene(SceneHolder scene)
        {
            scenes.Remove(scene.Id);
            Destroy(scene.gameObject);
        }
        public SceneFolder GetDirectoryByPath(string path)
        {
            // Find folder with specified path
            return folders.FirstOrDefault(item => item.Value.Path == path).Value;
        }
        public bool IsSubFolderOf(SceneFolder folderToCheck, SceneFolder parentFolder)
        {
            if (parentFolder == null || folderToCheck == null) return false;

            List<SceneFolder> subFolders = parentFolder.GetComponentsInChildren<SceneFolder>(true).ToList();
            subFolders.Remove(parentFolder);
            return subFolders.Contains(folderToCheck);
        }
        public List<SceneFolder> GetFolders(SceneFolder folder, bool deepSearch = false)
        {
            List<SceneFolder> listOfFolders = new List<SceneFolder>();
            SceneFolder[] folders = folder.Content.GetComponentsInChildren<SceneFolder>(false);
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
        public List<SceneHolder> GetScenes(SceneFolder folder, bool deepSearch = false)
        {
            List<SceneHolder> listOfScenes = new List<SceneHolder>();
            SceneHolder[] holders = folder.Content.GetComponentsInChildren<SceneHolder>(false);
            for (int i = 0; i < holders.Length; i++)
            {
                if (deepSearch)
                {
                    listOfScenes.Add(holders[i]);
                    continue;
                }

                if (holders[i].transform.parent == folder.Content) listOfScenes.Add(holders[i]);
            }

            return listOfScenes;
        }
        public void SelectFolder(SceneFolder folder)
        {
            // Deselect scene
            selectedScene = null;

            // Store selected folder
            selectedFolder = folder;
            Events.OnSceneFolderSelected?.Invoke(folder);
            Events.OnSceneSelected?.Invoke(null);
        }
        public void MoveFolderRoot()
        {
            if (selectedFolder == null) return;

            SocketManager.EmitAsync("move-scene-folder", async (callback) =>
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

                    Events.OnSceneFolderMoved?.Invoke();
                    selectedFolder = null;
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
            Events.OnSceneFolderDeselected?.Invoke();
            Events.OnSceneDeselected?.Invoke();
        }
        public void MoveSelected(SceneFolder folder)
        {
            if (selectedFolder != null)
            {
                SocketManager.EmitAsync("move-scene-folder", async (callback) =>
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

                        Events.OnSceneFolderMoved?.Invoke();
                        selectedFolder = null;
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    selectedFolder = null;
                }, selectedFolder.Path, folder.Path);
            }
            else
            {
                SocketManager.EmitAsync("move-scene", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();

                    if (callback.GetValue().GetBoolean())
                    {
                        // Set new transform
                        selectedScene.transform.SetParent(folder.Content);
                        selectedScene.transform.SetAsLastSibling();

                        // Calculate new path
                        selectedScene.UpdatePath(folder.Path);

                        Events.OnSceneMoved?.Invoke();
                        selectedScene = null;
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    selectedScene = null;
                }, selectedScene.Id, selectedScene.Path, folder.Path);
            }
        }

        public void SelectScene(SceneHolder scene)
        {
            // Deselect folder
            selectedFolder = null;

            // Store selected scene
            selectedScene = scene;
            Events.OnSceneSelected?.Invoke(scene);
            Events.OnSceneFolderSelected?.Invoke(null);
        }
        public void MoveSceneRoot()
        {
            if (selectedScene == null) return;

            SocketManager.EmitAsync("move-scene", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    // Set new transform
                    selectedScene.transform.SetParent(rootTransform);
                    selectedScene.transform.SetAsLastSibling();

                    // Calculate new path
                    selectedScene.UpdatePath("");

                    Events.OnSceneMoved?.Invoke();
                    selectedScene = null;
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                selectedScene = null;
            }, selectedScene.Id, selectedScene.Path, "");
        }
        public void DeselectScene()
        {
            // Send cancel event
            selectedScene = null;
            Events.OnSceneDeselected?.Invoke();
            Events.OnSceneFolderDeselected?.Invoke();
        }
    }
}