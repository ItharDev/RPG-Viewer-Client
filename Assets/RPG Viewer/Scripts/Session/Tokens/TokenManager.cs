using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class TokenManager : MonoBehaviour
    {
        [SerializeField] private Token tokenPrefab;
        [SerializeField] private Transform tokenParent;

        public Dictionary<string, Token> Tokens = new Dictionary<string, Token>();
        private List<Token> myTokens = new List<Token>();

        private TokenData copyData;
        private int selectedToken = -1;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnTokenCreated.AddListener(CreateToken);
            Events.OnTokenMoved.AddListener(MoveToken);
            Events.OnTokenModified.AddListener(ModifyToken);
            Events.OnTokenRemoved.AddListener(RemoveToken);
            Events.OnTokenEnabled.AddListener(EnableToken);
            Events.OnTokenLocked.AddListener(LockToken);
            Events.OnConditionsModified.AddListener(UpdateConditions);
            Events.OnHealthModified.AddListener(UpdateHealth);
            Events.OnElevationModified.AddListener(UpdateElevation);
            Events.OnTokenRotated.AddListener(RotateToken);
            Events.OnStateChanged.AddListener(ReloadTokens);
            Events.OnSceneLoaded.AddListener(LoadTokens);
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
            Events.OnConditionsModified.RemoveListener(UpdateConditions);
            Events.OnHealthModified.RemoveListener(UpdateHealth);
            Events.OnElevationModified.RemoveListener(UpdateElevation);
            Events.OnTokenRotated.RemoveListener(RotateToken);
            Events.OnStateChanged.RemoveListener(ReloadTokens);
            Events.OnSceneLoaded.RemoveListener(LoadTokens);
        }
        private void Update()
        {
            // Paste token
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
            {
                // if (FindObjectOfType<StateManager>(true).ToolState == ToolState.Pan && !string.IsNullOrEmpty(copyData.id))
                // {
                //     PasteToken(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                // }
            }

            // Iterate through my tokens
            if (myTokens.Count != 0 && Input.GetKeyDown(KeyCode.Tab))
            {
                // Reset selection at the end of list
                if (selectedToken >= myTokens.Count) selectedToken = 0;

                // Check if any token is selected
                if (selectedToken >= 0)
                {
                    // Set current token as not selected
                    myTokens[selectedToken].DeselectToken();
                }

                // Move to next token in the list
                selectedToken++;

                // Reset selection at the end of list
                if (selectedToken >= myTokens.Count) selectedToken = 0;

                // Set new token as selected
                myTokens[selectedToken].SelectToken();

                // Apply new target for the camera to follow
                FindObjectOfType<Camera2D>().FollowTarget(myTokens[selectedToken].transform);
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
                if (!ConnectionManager.Info.isMaster && !data.enabled) token.gameObject.SetActive(false);

                // Load data and add it to dictionary
                token.LoadData(data, sprite);
                Tokens.Add(data.id, token);

                // Check token's permissions
                if (token.Permission.type == PermissionType.Owner) myTokens.Add(token);

                // Select this token if it's the first token we instantiate and this client is player
                if (myTokens.Count == 1) SelectToken(ConnectionManager.Info.isMaster ? null : myTokens[0]);
            });
        }
        private void MoveToken(string id, MovementData data)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

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
                });

                return;
            }

            // Load data
            token.LoadData(data, null);
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

            // Check if we are the master client
            if (ConnectionManager.Info.isMaster) token.EnableToken(enabled);
            else token.gameObject.SetActive(enabled);
        }
        private void UpdateElevation(string id, string elevation)
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
        private void UpdateHealth(string id, int health)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.SetHealth(health);
        }
        private void RotateToken(string id, float angle)
        {
            // Find the correct token
            Token token = Tokens[id];

            // Check if token was found
            if (token == null) return;

            token.UI.SetRotation(angle);
        }

        private void LoadTokens(SceneSettings settings)
        {
            List<string> list = settings.tokens;
            SocketManager.EmitAsync("get-tokens", (callback) =>
            {
                // TODO: Load all tokens at once
            }, settings.id);
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
                if (oldState.synced && !newState.synced)
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

        public void SelectToken(Token token)
        {
            // Loop through each token
            foreach (var item in Tokens)
            {
                // Set token as selected
                item.Value.Selected = true;

                // Deselect token if it's not correct
                if (item.Value != token && token != null) item.Value.Selected = false;

                // Reload sorting and lighting
                item.Value.UI.UpdateSorting();
                // TODO: item.Value.LoadLights();
            }

            // Return if no token was selected
            if (token == null) return;

            // Update index of selected token
            int index = myTokens.IndexOf(token);
            selectedToken = index;
        }

        public void CopyToken(TokenData data)
        {
            // Update data in clipboard
            copyData = data;
        }
        private void PasteToken(Vector2 pos)
        {
            // Update pasted token's position
            copyData.position = pos;

            SocketManager.EmitAsync("create-token", (callback) =>
            {
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(copyData));
        }
    }
}