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
        private TokenFolder selectedFolder;
        private TokenHolder selectedToken;
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
            token.LoadData(id, path, this);

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
        public Color GetColor()
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
            if (parentFolder == null || folderToCheck == null) return false;

            List<TokenFolder> subFolders = parentFolder.GetComponentsInChildren<TokenFolder>(true).ToList();
            subFolders.Remove(parentFolder);
            return subFolders.Contains(folderToCheck);
        }

        public void SelectFolder(TokenFolder folder)
        {
            // Deselect token
            selectedToken = null;

            // Store selected folder
            selectedFolder = folder;
            Events.OnBlueprintFolderSelected?.Invoke(folder);
            Events.OnBlueprintSelected?.Invoke(null);
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
            Events.OnBlueprintFolderDeselected?.Invoke();
            Events.OnBlueprintDeselected?.Invoke();
        }
        public void DeselectFolder()
        {
            // Send cancel event
            selectedFolder = null;
            Events.OnBlueprintFolderDeselected?.Invoke();
            Events.OnBlueprintDeselected?.Invoke();
        }
        public void MoveSelected(TokenFolder folder)
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
                Debug.Log($"Moving token from {selectedToken.Path} to {folder.Path}");

                // Set new transform
                selectedToken.transform.SetParent(folder.Content);
                selectedToken.transform.SetAsLastSibling();

                // Calculate new path
                selectedToken.UpdatePath(folder.Path);
                selectedToken = null;
            }

            // Send move finish move event
            Events.OnBlueprintFolderMoved?.Invoke();
            Events.OnBlueprintMoved?.Invoke();
        }

        public void SelectToken(TokenHolder token)
        {
            // Deselect folder
            selectedFolder = null;

            // Store selected token
            selectedToken = token;
            Events.OnBlueprintSelected?.Invoke(token);
            Events.OnBlueprintFolderSelected?.Invoke(null);
        }
        public void MoveTokenRoot()
        {
            if (selectedToken == null) return;

            Debug.Log($"Moving token from {selectedToken.Path} to root");

            // Set new transform
            selectedToken.transform.SetParent(rootTransform);
            selectedToken.transform.SetAsFirstSibling();

            // Calculate new path
            selectedToken.UpdatePath("");

            // Send cancel event
            selectedToken = null;
            Events.OnBlueprintDeselected?.Invoke();
            Events.OnBlueprintFolderDeselected?.Invoke();
        }
        public void DeselectScene()
        {
            // Send cancel event
            selectedToken = null;
            Events.OnBlueprintDeselected?.Invoke();
            Events.OnBlueprintFolderDeselected?.Invoke();
        }
    }
}