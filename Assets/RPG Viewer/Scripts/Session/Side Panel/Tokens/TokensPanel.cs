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
    public class TokensPanel : MonoBehaviour
    {
        [SerializeField] private TokenFolder folderPrefab;
        [SerializeField] private TokenHolder tokenPrefab;
        [SerializeField] private Transform rootTransform;
        [SerializeField] private TokenConfiguration configPrefab;

        private Dictionary<string, TokenFolder> folders = new Dictionary<string, TokenFolder>();
        private Dictionary<string, TokenHolder> tokens = new Dictionary<string, TokenHolder>();

        private bool loaded;
        private TokenFolder selectedFolder;
        private TokenHolder selectedToken;
        private float lastCount;
        private float lastColor;

        private void OnEnable()
        {
            if (!loaded)
            {
                loaded = true;
                LoadTokens();
            }
        }
        private void Update()
        {
            if (lastCount != rootTransform.childCount)
            {
                lastCount = rootTransform.childCount;
                SortContent();
            }
        }

        private void SortContent()
        {
            List<TokenFolder> listOfFolders = folders.Values.ToList();
            List<TokenHolder> listOfTokens = tokens.Values.ToList();

            listOfFolders.Sort(SortByName);
            listOfTokens.Sort(SortByName);

            for (int i = 0; i < folders.Count; i++)
            {
                listOfFolders[i].transform.SetSiblingIndex(i);
            }
            for (int i = 0; i < listOfTokens.Count; i++)
            {
                listOfTokens[i].transform.SetSiblingIndex(i + folders.Count);
            }
        }
        private int SortByName(TokenFolder folderA, TokenFolder folderB)
        {
            return folderA.Data.name.CompareTo(folderB.Data.name);
        }
        private int SortByName(TokenHolder holderA, TokenHolder holderB)
        {
            return holderA.Data.name.CompareTo(holderB.Data.name);
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
                    var folders = callback.GetValue(1).GetProperty("folders").EnumerateObject().ToArray();
                    var contents = callback.GetValue(1).GetProperty("contents").EnumerateArray().ToArray();

                    // Load folders
                    for (int i = 0; i < folders.Length; i++)
                    {
                        LoadDirectory(folders[i].Value, folders[i].Name, "");
                    }

                    // Load tokens
                    for (int i = 0; i < contents.Length; i++)
                    {
                        LoadToken(contents[i].GetString(), "");
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
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
            TokenFolder targetFolder = GetDirectoryByPath(path);
            TokenFolder folder = Instantiate(folderPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            folder.LoadData(data, this);

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
                LoadToken(contents[i].GetString(), pathToThisFolder);
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

        public async void CreateToken(string path)
        {
            await ImageTask((bytes) =>
            {
                TokenConfiguration config = Instantiate(configPrefab);
                config.transform.SetParent(UICanvas.Instance.transform);
                config.transform.localPosition = Vector3.zero;
                config.transform.SetAsLastSibling();
                config.LoadData(new TokenData(), new PresetData(), bytes, "Create new Blueprint", (tokenData, image, lightData) =>
                {
                    SocketManager.EmitAsync("create-blueprint", (callback) =>
                    {
                        // Check if the event was successful
                        if (callback.GetValue().GetBoolean())
                        {
                            string id = callback.GetValue().GetString();
                            string image = callback.GetValue(1).GetString();

                            tokenData.id = id;
                            LoadToken(id, path);
                            return;
                        }

                        // Send error message
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    }, path, JsonUtility.ToJson(tokenData), JsonUtility.ToJson(lightData), Convert.ToBase64String(image));
                });
            });
        }
        private async Task ImageTask(Action<byte[]> callback)
        {
            // Only allow image files
            ExtensionFilter[] extensions = new ExtensionFilter[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg") };

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
        public TokenFolder CreateFolder(string id, string path)
        {
            // Create path to this directory
            string pathToThisFolder = string.IsNullOrEmpty(path) ? id : $"{path}/{id}";
            Folder data = new Folder(id, path, "New Folder", GetColor());

            // Instantiate folder
            TokenFolder targetFolder = GetDirectoryByPath(path);
            TokenFolder folder = Instantiate(folderPrefab, targetFolder == null ? rootTransform : targetFolder.Content);
            folder.LoadData(data, this);

            // Add folder to dictionary
            this.folders.Add(id, folder);
            return folder;
        }
        public void RemoveFolder(TokenFolder folder)
        {
            List<TokenFolder> subFolders = GetFolders(folder);
            List<TokenHolder> tokens = GetTokens(folder);

            for (int i = 0; i < subFolders.Count; i++)
            {
                RemoveFolder(subFolders[i]);
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                RemoveToken(tokens[i]);
            }

            folders.Remove(folder.Id);
            Destroy(folder.gameObject);
        }
        public void CreateFolder()
        {
            SocketManager.EmitAsync("create-blueprint-folder", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();

                    // Create the folder
                    string id = callback.GetValue(1).GetString();
                    TokenFolder createdFolder = CreateFolder(id, "");

                    // Activate rename field
                    createdFolder.Rename();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, "", "New Folder");
        }
        public void RemoveToken(TokenHolder token)
        {
            tokens.Remove(token.Id);
            Destroy(token.gameObject);
        }
        public TokenFolder GetDirectoryByPath(string path)
        {
            // Find folder with specified path
            return folders.FirstOrDefault(item => item.Value.Path == path).Value;
        }
        public bool IsSubFolderOf(TokenFolder folderToCheck, TokenFolder parentFolder)
        {
            if (parentFolder == null || folderToCheck == null) return false;

            List<TokenFolder> subFolders = parentFolder.GetComponentsInChildren<TokenFolder>(true).ToList();
            subFolders.Remove(parentFolder);
            return subFolders.Contains(folderToCheck);
        }
        public List<TokenFolder> GetFolders(TokenFolder folder, bool deepSearch = false)
        {
            List<TokenFolder> listOfFolders = new List<TokenFolder>();
            TokenFolder[] folders = folder.Content.GetComponentsInChildren<TokenFolder>(false);
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
        public List<TokenHolder> GetTokens(TokenFolder folder, bool deepSearch = false)
        {
            List<TokenHolder> listOfTokens = new List<TokenHolder>();
            TokenHolder[] holders = folder.Content.GetComponentsInChildren<TokenHolder>(false);
            for (int i = 0; i < holders.Length; i++)
            {
                if (deepSearch)
                {
                    listOfTokens.Add(holders[i]);
                    continue;
                }

                if (holders[i].transform.parent == folder.Content) listOfTokens.Add(holders[i]);
            }

            return listOfTokens;
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

            SocketManager.EmitAsync("move-blueprint-folder", async (callback) =>
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

                    Events.OnBlueprintFolderMoved?.Invoke();
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
            Events.OnBlueprintFolderDeselected?.Invoke();
            Events.OnBlueprintDeselected?.Invoke();
        }
        public void MoveSelected(TokenFolder folder)
        {
            if (selectedFolder != null)
            {
                SocketManager.EmitAsync("move-blueprint-folder", async (callback) =>
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

                        Events.OnBlueprintFolderMoved?.Invoke();
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
                SocketManager.EmitAsync("move-blueprint", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();

                    if (callback.GetValue().GetBoolean())
                    {
                        // Set new transform
                        selectedToken.transform.SetParent(folder.Content);
                        selectedToken.transform.SetAsLastSibling();

                        // Calculate new path
                        selectedToken.UpdatePath(folder.Path);

                        Events.OnBlueprintMoved?.Invoke();
                        selectedToken = null;
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    selectedToken = null;
                }, selectedToken.Id, selectedToken.Path, folder.Path);
            }
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

            SocketManager.EmitAsync("move-blueprint", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                if (callback.GetValue().GetBoolean())
                {
                    // Set new transform
                    selectedToken.transform.SetParent(rootTransform);
                    selectedToken.transform.SetAsLastSibling();

                    // Calculate new path
                    selectedToken.UpdatePath("");

                    Events.OnBlueprintMoved?.Invoke();
                    selectedToken = null;
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                selectedToken = null;
            }, selectedToken.Id, selectedToken.Path, "");
        }
        public void DeselectToken()
        {
            // Send cancel event
            selectedToken = null;
            Events.OnBlueprintDeselected?.Invoke();
            Events.OnBlueprintFolderDeselected?.Invoke();
        }
    }
}