using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class TokensPanel : MonoBehaviour
    {
        [SerializeField] private TokenFolder folderPrefab;
        [SerializeField] private TokenHolder tokenPrefab;
        [SerializeField] private Transform rootTransform;

        private Dictionary<string, TokenFolder> folders = new Dictionary<string, TokenFolder>();
        private Dictionary<string, TokenHolder> tokens = new Dictionary<string, TokenHolder>();

        private bool loaded;
        private TokenFolder movedFolder;

        private void OnEnable()
        {
            if (!loaded)
            {
                loaded = true;
                LoadTokens();
            }
        }

        private void LoadTokens()
        {
            SocketManager.EmitAsync("get-blueprints", async (callback) =>
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
            if (json.ValueKind == System.Text.Json.JsonValueKind.String) LoadToken(json.GetString(), path);
            else LoadDirectory(json, path);
        }
        private void LoadToken(string id, string path)
        {
            // Find target folder
            TokenFolder targetFolder = GetDirectoryByPath(path);

            // Instantiate token
            TokenHolder token = Instantiate(tokenPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            token.transform.SetAsLastSibling();
            token.LoadData(id, path);

            // Add token to dictionary
            tokens.Add(id, token);
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
            TokenFolder targetFolder = GetDirectoryByPath(path);
            TokenFolder folder = Instantiate(folderPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
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
                LoadToken(contents[i].GetString(), pathToThisFolder);
            }
        }
        private Color GetColor()
        {
            // Generate random color with 13 variants
            float randomHue = Random.Range(0, 12) * (1.0f / 12.0f);
            return Color.HSVToRGB(randomHue, 0.6f, 1.0f);
        }

        public TokenFolder GetDirectoryByPath(string path)
        {
            // Find folder with specified path
            return folders.FirstOrDefault(item => item.Value.Path == path).Value;
        }
        public TokenFolder GetDirectoryById(string id)
        {
            // Find folder with specified id
            return folders[id];
        }
        public bool IsSubFolderOf(TokenFolder folderToCheck, TokenFolder parentFolder)
        {
            List<TokenFolder> subFolders = parentFolder.GetComponentsInChildren<TokenFolder>(true).ToList();
            subFolders.Remove(parentFolder);
            return subFolders.Contains(folderToCheck);
        }

        public void InitFolderMove(TokenFolder folder)
        {
            // Store selected folder
            movedFolder = folder;
            Events.OnTokenFolderMoveInit?.Invoke(folder);
        }
        public void MoveRoot()
        {
            if (movedFolder == null) return;

            Debug.Log($"Moving folder from {movedFolder.Path} to root");

            // Set new transform
            movedFolder.transform.SetParent(rootTransform);
            movedFolder.transform.SetAsFirstSibling();

            // Calculate new path
            movedFolder.CalculatePath("");

            // Send cancel event
            movedFolder = null;
            Events.OnTokenFolderMoveCancel?.Invoke();
        }
        public void CancelFolderMove()
        {
            // Send cancel event
            movedFolder = null;
            Events.OnTokenFolderMoveCancel?.Invoke();
        }
        public void FinishFolderMove(TokenFolder folder)
        {
            // Return if there's no folder selected
            if (movedFolder == null) return;

            Debug.Log($"Moving folder from {movedFolder.Path} to {folder.Path}");

            // Set new transform
            movedFolder.transform.SetParent(folder.Content);
            movedFolder.transform.SetAsFirstSibling();

            // Calculate new path
            movedFolder.CalculatePath(folder.Path);

            // Send move finish move event
            movedFolder = null;
            Events.OnTokenFolderMoveFinish?.Invoke();
        }
    }
}