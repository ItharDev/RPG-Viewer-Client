using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class TokenManager : MonoBehaviour
    {
        [SerializeField] private Token tokenPrefab;
        [SerializeField] private Transform tokenParent;

        public Dictionary<string, Token> Tokens = new Dictionary<string, Token>();
        public bool TokenHovered;

        private List<Token> myTokens = new List<Token>();
        private List<Token> tokensToSelect = new List<Token>();

        private List<int> selectedTokens = new List<int>() { 0 };

        private void OnEnable()
        {
            // Add event listeners
            Events.OnTokenCreated.AddListener(CreateToken);
            Events.OnTokenMoved.AddListener(MoveToken);
            Events.OnTokenModified.AddListener(ModifyToken);
            Events.OnTokenRemoved.AddListener(RemoveToken);
            Events.OnTokenEnabled.AddListener(EnableToken);
            Events.OnTokenLocked.AddListener(LockToken);
            Events.OnTokenLightToggled.AddListener(ToggleLight);
            Events.OnConditionsModified.AddListener(UpdateConditions);
            Events.OnHealthModified.AddListener(UpdateHealth);
            Events.OnElevationModified.AddListener(UpdateElevation);
            Events.OnTokenRotated.AddListener(RotateToken);
            Events.OnTokenLightRotated.AddListener(RotateLight);
            Events.OnStateChanged.AddListener(ReloadTokens);
            Events.OnSceneLoaded.AddListener(LoadTokens);
            Events.OnTokenSelected.AddListener(SelectToken);
            Events.OnTokenTeleported.AddListener(TeleportToken);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnTokenCreated.RemoveListener(CreateToken);
            Events.OnTokenMoved.RemoveListener(MoveToken);
            Events.OnTokenModified.RemoveListener(ModifyToken);
            Events.OnTokenRemoved.RemoveListener(RemoveToken);
            Events.OnTokenEnabled.RemoveListener(EnableToken);
            Events.OnTokenLocked.RemoveListener(LockToken);
            Events.OnTokenLightToggled.RemoveListener(ToggleLight);
            Events.OnConditionsModified.RemoveListener(UpdateConditions);
            Events.OnHealthModified.RemoveListener(UpdateHealth);
            Events.OnElevationModified.RemoveListener(UpdateElevation);
            Events.OnTokenRotated.RemoveListener(RotateToken);
            Events.OnTokenLightRotated.RemoveListener(RotateLight);
            Events.OnStateChanged.RemoveListener(ReloadTokens);
            Events.OnSceneLoaded.RemoveListener(LoadTokens);
            Events.OnTokenSelected.RemoveListener(SelectToken);
            Events.OnTokenTeleported.RemoveListener(TeleportToken);
        }
        private void Update()
        {
            // Iterate through my tokens
            if (myTokens.Count != 0 && Input.GetKeyDown(KeyCode.Tab))
            {
                if (selectedTokens.Count == 0) selectedTokens = new List<int>() { 0 };
                else if (selectedTokens[0] >= myTokens.Count) selectedTokens = new List<int>() { 0 };

                Events.OnTokenSelected?.Invoke(myTokens[selectedTokens[0]], false);
                selectedTokens[0]++;
            }
        }

        private void CreateToken(TokenData data)
        {
            // Download token's texture
            WebManager.Download(data.image, true, async (bytes) =>
            {
                await UniTask.SwitchToMainThread();

                // Create texture
                Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                // Instantiate token and attach it to correct parent
                Token token = Instantiate(tokenPrefab, data.position, Quaternion.identity, tokenParent);

                // Disable token if needed
                if (!ConnectionManager.Info.isMaster && !data.enabled) token.UI.SetAlpha(0.0f);

                // Load data and add it to dictionary
                token.LoadData(data, sprite);
                Tokens.Add(data.id, token);
                
                // Select this token if it's the first token we instantiate and this client is player
                if (myTokens.Count == 0) SelectToken(ConnectionManager.Info.isMaster ? null : myTokens[0]);

                // Check token's permissions
                if (token.Permission.type == PermissionType.Controller && token.Visibility.visible && (token.Data.enabled || ConnectionManager.Info.isMaster)) myTokens.Add(token);
            });
        }
        private void MoveToken(string id, MovementData data)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.Data.teleportProtection = false;
            token.Movement.AddWaypoints(data);
        }
        private void ModifyToken(string id, TokenData data)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            // Check if token's image was modified
            if (token.Data.image != data.image)
            {
                WebManager.Download(data.image, true, async (bytes) =>
                {
                    await UniTask.SwitchToMainThread();

                    // Generate new texture
                    Texture2D texture = await AsyncImageLoader.CreateFromImageAsync(bytes);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

                    // Load data with the new image
                    token.LoadData(data, sprite);
                    if (myTokens.Contains(token) && (token.Permission.type != PermissionType.Controller || !token.Visibility.visible))
                    {
                        myTokens.Remove(token);
                        SelectToken(null);
                    }
                    else if (!myTokens.Contains(token) && token.Permission.type == PermissionType.Controller && token.Visibility.visible) myTokens.Add(token);
                });

                return;
            }

            // Load data
            token.LoadData(data, null);
            if (myTokens.Contains(token) && (token.Permission.type != PermissionType.Controller || !token.Visibility.visible))
            {
                myTokens.Remove(token);
                SelectToken(null);
            }
            else if (!myTokens.Contains(token) && token.Permission.type == PermissionType.Controller && token.Visibility.visible) myTokens.Add(token);
        }
        private void RemoveToken(string id)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            // Remove token from lists and destroy it
            if (myTokens.Contains(token)) myTokens.Remove(token);
            Tokens.Remove(id);
            Destroy(token.gameObject);
        }
        private void EnableToken(string id, bool enabled)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.EnableToken(enabled);

            if (ConnectionManager.Info.isMaster) return;

            if (myTokens.Contains(token) && !enabled)
            {
                myTokens.Remove(token);
                SelectToken(null);
            }
            else if (!myTokens.Contains(token) && token.Permission.type == PermissionType.Controller && token.Visibility.visible && enabled) myTokens.Add(token);
            token.gameObject.SetActive(enabled);
        }
        private void UpdateElevation(string id, int elevation)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.SetElevation(elevation);
        }
        private void UpdateConditions(string id, int conditions)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.Conditions.SetConditions(conditions);
        }
        private void LockToken(string id, bool locked)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.SetLocked(locked);
        }
        private void ToggleLight(string id, bool enabled)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.ToggleLight(enabled);
        }
        private void UpdateHealth(string id, int health)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.SetHealth(health);
        }
        private void RotateToken(string id, float angle, string user)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null || user == GameData.User.id) return;

            token.UI.SetRotation(angle);
        }
        private void RotateLight(string id, float angle, string user)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null || user == GameData.User.id) return;

            token.Vision.SetRotation(angle);
        }

        private void LoadTokens(SceneData settings)
        {
            SocketManager.EmitAsync("get-tokens", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    // Enumerate tokens array
                    var list = callback.GetValue(1).EnumerateArray().ToArray();

                    for (int i = 0; i < list.Length; i++)
                    {
                        LoadToken(list[i]);
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        private void LoadToken(System.Text.Json.JsonElement json)
        {
            TokenData data = JsonUtility.FromJson<TokenData>(json.ToString());
            data.id = json.GetProperty("_id").GetString();
            CreateToken(data);
        }
        private void ReloadTokens(SessionState oldState, SessionState newState)
        {
            // Check if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadTokens();
            }
            else
            {
                // Unload tokens if syncing was disabled
                if (!newState.synced)
                {
                    UnloadTokens();
                    return;
                }

                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadTokens();
            }
        }

        private void UnloadTokens()
        {
            // Loop through each token
            foreach (var item in Tokens)
            {
                // Continue if token is null
                if (item.Value == null) continue;
                Destroy(item.Value.gameObject);
            }

            // Clear lists
            Tokens.Clear();
            myTokens.Clear();
        }

        public void SelectToken(Token token, bool multiSelection = false)
        {
            // Update index of selected token
            if (token == null) selectedTokens = new List<int>();
            else if (multiSelection)
            {
                if (selectedTokens.Contains(myTokens.IndexOf(token))) selectedTokens.Remove(myTokens.IndexOf(token));
                else selectedTokens.Add(myTokens.IndexOf(token));
            }
            else selectedTokens = new List<int>() { myTokens.IndexOf(token) };

            tokensToSelect = new List<Token>();
            for (int i = 0; i < selectedTokens.Count; i++)
            {
                tokensToSelect.Add(myTokens[selectedTokens[i]]);
            }
            Events.OnTokensSelected?.Invoke(tokensToSelect);
        }

        public void HoverToken(bool hovered)
        {
            TokenHovered = hovered;
        }
        public void HandleMultiMovement(Token original, List<Vector2> dragPoints)
        {
            if (!tokensToSelect.Contains(original))
            {
                original.Movement.EndMovement(dragPoints);
                return;
            }
            for (int i = 0; i < tokensToSelect.Count; i++)
            {
                tokensToSelect[i].Movement.EndMovement(dragPoints);
            }
        }

        public void HandlePortal(Token token, Portal portal)
        {
            if (token == null) return;
            SocketManager.EmitAsync("enter-portal", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, token.Id, portal.Id);
        }
        public void TeleportToken(string id, Vector2 destination)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.Movement.Teleport(destination);
        }
    }
}