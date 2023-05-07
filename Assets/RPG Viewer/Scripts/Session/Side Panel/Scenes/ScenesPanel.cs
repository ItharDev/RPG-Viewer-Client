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
            scene.LoadData(id, path);

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
            folder.LoadData(data);

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
        private Color GetColor()
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
    }
}