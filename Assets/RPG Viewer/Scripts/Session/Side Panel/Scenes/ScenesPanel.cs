using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
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

        private void OnEnable()
        {
            if (!loaded)
            {
                loaded = true;
                LoadScenes();
            }
        }

        private void LoadScenes()
        {
            SocketManager.EmitAsync("get-scenes", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Enumerate tokens array
                    var list = callback.GetValue(1).EnumerateArray().ToArray();

                    // Load elements
                    for (int i = 0; i < list.Length; i++)
                    {
                        LoadElement(list[i], "");
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        private void LoadElement(System.Text.Json.JsonElement json, string path)
        {
            // Check if this element is a token or a directory
            if (json.ValueKind == System.Text.Json.JsonValueKind.String) LoadScene(json.GetString(), path);
            else LoadDirectory(json, path);
        }
        private void LoadScene(string id, string path)
        {
            // Find target folder
            SceneFolder targetFolder = GetDirectoryByPath(path);

            // Instantiate scene
            SceneHolder scene = Instantiate(scenePrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            scene.transform.SetAsLastSibling();
            scene.LoadData(id, path, this);

            // Add scene to dictionary
            scenes.Add(id, scene);
        }
        private void LoadDirectory(System.Text.Json.JsonElement json, string path)
        {
            // Load folder's data
            string id = json.GetProperty("id").GetString();
            string name = json.GetProperty("name").GetString();
            var subFolders = json.GetProperty("subFolders").EnumerateArray().ToArray();
            var contents = json.GetProperty("contents").EnumerateArray().ToArray();

            // Create path to this directory
            string pathToThisFolder = string.IsNullOrEmpty(path) ? id : $"{path}/{id}";
            Folder data = new Folder(id, path, name, GetColor());

            // Instantiate folder
            SceneFolder targetFolder = GetDirectoryByPath(path);
            SceneFolder folder = Instantiate(folderPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            folder.LoadData(data, this);

            // Add folder to dictionary
            folders.Add(id, folder);

            // Load sub folders
            for (int i = 0; i < subFolders.Length; i++)
            {
                LoadDirectory(subFolders[i], pathToThisFolder);
            }

            // Load tokens
            for (int i = 0; i < contents.Length; i++)
            {
                LoadScene(contents[i].GetString(), pathToThisFolder);
            }
        }
        public Color GetColor()
        {
            // Generate random color with 13 variants
            float randomHue = Random.Range(0, 12) * (1.0f / 12.0f);
            return Color.HSVToRGB(randomHue, 0.6f, 1.0f);
        }

        public SceneFolder GetDirectoryByPath(string path)
        {
            // Find folder with specified path
            return folders.FirstOrDefault(item => item.Value.Path == path).Value;
        }
        public SceneFolder GetDirectoryById(string id)
        {
            // Find folder with specified id
            return folders[id];
        }
        public bool IsSubFolderOf(SceneFolder folderToCheck, SceneFolder parentFolder)
        {
            if (parentFolder == null || folderToCheck == null) return false;

            List<SceneFolder> subFolders = parentFolder.GetComponentsInChildren<SceneFolder>(true).ToList();
            subFolders.Remove(parentFolder);
            return subFolders.Contains(folderToCheck);
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

            Debug.Log($"Moving folder from {selectedFolder.Path} to root");

            // Set new transform
            selectedFolder.transform.SetParent(rootTransform);
            selectedFolder.transform.SetAsFirstSibling();

            // Calculate new path
            selectedFolder.CalculatePath("");

            // Send cancel event
            selectedFolder = null;
            Events.OnSceneFolderDeselected?.Invoke();
            Events.OnSceneDeselected?.Invoke();
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
                Debug.Log($"Moving folder from {selectedFolder.Path} to {folder.Path}");

                // Set new transform
                selectedFolder.transform.SetParent(folder.Content);
                selectedFolder.transform.SetAsFirstSibling();

                // Calculate new path
                selectedFolder.CalculatePath(folder.Path);
                selectedFolder = null;
            }
            else
            {
                Debug.Log($"Moving scene from {selectedScene.Path} to {folder.Path}");

                // Set new transform
                selectedScene.transform.SetParent(folder.Content);
                selectedScene.transform.SetAsFirstSibling();

                // Calculate new path
                selectedScene.UpdatePath(folder.Path);
                selectedScene = null;
            }

            // Send move finish move event
            Events.OnSceneFolderMoved?.Invoke();
            Events.OnSceneMoved?.Invoke();
        }

        public void SelectToken(SceneHolder scene)
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

            Debug.Log($"Moving scene from {selectedScene.Path} to root");

            // Set new transform
            selectedScene.transform.SetParent(rootTransform);
            selectedScene.transform.SetAsFirstSibling();

            // Calculate new path
            selectedScene.UpdatePath("");

            // Send cancel event
            selectedScene = null;
            Events.OnSceneDeselected?.Invoke();
            Events.OnSceneFolderDeselected?.Invoke();
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