using System;
using System.Collections.Generic;
using System.Linq;
using Networking;
using UnityEngine;

namespace RPG
{
    public class Token : MonoBehaviour
    {
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private LayerMask mountLayers;

        public TokenData Data;
        public PresetData Lighting;

        public TokenMovement Movement { get; private set; }
        public TokenVision Vision { get; private set; }
        public TokenUI UI { get; private set; }
        public TokenConditions Conditions { get; private set; }
        public TokenEffect Effect { get; private set; }

        public bool IsOwner
        {
            get { return Permission.type == PermissionType.Controller; }
        }
        public string Id { get { return Data.id; } }
        public bool IsPlayer
        {
            get
            {
                Permission owner = Data.permissions.FirstOrDefault(value => value.type == PermissionType.Controller);
                return !string.IsNullOrEmpty(owner.user);
            }
        }

        public Permission Permission;
        public Visible Visibility;
        public bool Selected;
        public bool Enabled;

        private void OnEnable()
        {
            // Get child references
            if (Movement == null) Movement = GetComponent<TokenMovement>();
            if (Vision == null) Vision = GetComponent<TokenVision>();
            if (UI == null) UI = GetComponent<TokenUI>();
            if (Conditions == null) Conditions = GetComponent<TokenConditions>();
            if (Effect == null) Effect = GetComponent<TokenEffect>();

            // Add event listeners
            Events.OnTokensSelected.AddListener(HandleSelection);
            Events.OnViewChanged.AddListener(ChangeView);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnTokensSelected.RemoveListener(HandleSelection);
            Events.OnViewChanged.RemoveListener(ChangeView);
        }
        private void Update()
        {
            // Return if token is not selected or we are edting any fields
            if (!Selected || UI.Editing || !IsOwner) return;

            HandleDeletion();
        }

        private void HandleSelection(List<Token> tokens)
        {
            if (tokens.Count == 0)
            {
                Selected = false;
                Enabled = Permission.type != PermissionType.None;
            }
            else
            {
                Selected = tokens.Contains(this);
                Enabled = Selected;
            }

            if (Selected) FindFirstObjectByType<Camera2D>().FollowTarget(transform);
            Vision.ToggleVision(Enabled && Visibility.visible && (Data.enabled || ConnectionManager.Info.isMaster));
        }

        private void ChangeView(GameView view)
        {
            Vision.ToggleLight(Data.lightEnabled && view != GameView.Lights);
        }

        private void HandleDeletion()
        {
            // Check if backspace or delete key is pressed
            if (Input.GetKeyUp(KeyCode.Backspace) || Input.GetKeyUp(KeyCode.Delete))
            {
                DeleteToken();
            }
        }

        public List<Token> GetNearbyTokens()
        {
            // Check for colliders within radius
            List<Token> mounted = new List<Token>();
            float radius = (Data.dimensions.x >= Data.dimensions.y ? UI.Rect.sizeDelta.x : UI.Rect.sizeDelta.y) * 0.009f;
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius, mountLayers);

            // Loop through each collider
            for (var i = 0; i < colliders.Length; i++)
            {
                Token token = colliders[i].GetComponent<Token>();

                // Don't count in ourselves
                if (token == this || token == null) continue;
                if (token.Data.type == TokenType.Character && token.Data.enabled) mounted.Add(token);
            }

            return mounted;
        }
        public void DisableCollider()
        {
            // Disable collisions
            boxCollider.enabled = false;
        }

        public void LoadData(TokenData data, Sprite sprite)
        {
            // Update data and set permissions
            Data = data;
            SetPermission();
            SetVisibility();
            Enabled = Permission.type != PermissionType.None;

            // Update conditions
            Conditions.SetConditions(data.conditions);

            // Reload UI
            if (sprite != null) UI.SetImage(sprite);
            UI.Reload();

            // Load lighting and vision
            Vision.Reload();

            // Load effects
            Effect.Reload();

            float cellSize = Session.Instance.Grid.CellSize;
            Vector2 dimensions = Data.dimensions;
            boxCollider.size = new Vector2(100 * cellSize * (dimensions.x / Session.Instance.Grid.Unit.scale), 100 * cellSize * (dimensions.y / Session.Instance.Grid.Unit.scale));
        }

        private void SetPermission()
        {
            // Set as owner if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                Permission = new Permission(GameData.User.id, PermissionType.Controller);
                return;
            }

            // Check if permissions are defined for our uid
            Permission myPermission = Data.permissions.FirstOrDefault(value => value.user == GameData.User.id);
            if (string.IsNullOrEmpty(myPermission.user))
            {
                Permission = new Permission(GameData.User.id, PermissionType.None);
                return;
            }

            // Update our permission
            Permission = myPermission;
        }
        private void SetVisibility()
        {
            // Set as visible if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                Visibility = new Visible(GameData.User.id, true);
                return;
            }

            // Check if visibility is defined for our uid
            Visible myVisibility = Data.visible.FirstOrDefault(value => value.user == GameData.User.id);
            if (string.IsNullOrEmpty(myVisibility.user))
            {
                Visibility = new Visible(GameData.User.id, true);
                return;
            }

            // Update our visibility
            Visibility = myVisibility;
        }
        public void EnableToken(bool enabled)
        {
            Data.enabled = enabled;

            // Enable / disable token
            Vision.Reload();
            Effect.Reload();
            UI.Toggle(enabled);
        }
        public void ToggleLight(bool enabled)
        {
            Data.lightEnabled = enabled;
            Vision.ToggleLight(enabled);
            UI.ToggleLight(enabled);
        }
        public void SetElevation(int elevation)
        {
            Data.elevation = elevation;
            UI.SetElevation(elevation);
        }
        public void SetLocked(bool locked)
        {
            Data.locked = locked;
            UI.EnableRaycasting(!(locked && !ConnectionManager.Info.isMaster));
            UI.LockToken(locked);
        }
        public void SetHealth(int health)
        {
            Data.health = health;
            UI.SetHealth(health);
        }

        #region Buttons
        public void DeleteToken()
        {
            if (!IsPlayer)
            {
                FinishDeletion(true);
                return;
            }

            // Ask for confirmation
            Confirmation confirmation = new Confirmation("Confirm deletion?", "Confirm", "Cancel", FinishDeletion);
            MessageManager.AskConfirmation(confirmation);
        }

        private void FinishDeletion(bool result)
        {
            // Return if result was negative
            if (!result) return;

            SocketManager.EmitAsync("remove-token", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Data.id);
        }
        #endregion
    }

    [Serializable]
    public struct TokenData
    {
        public string id;
        public string name;
        public TokenType type;
        public List<Permission> permissions;
        public List<Visible> visible;
        public Vector2 dimensions;
        public float visionRadius;
        public float nightRadius;
        public string image;
        public string art;
        public string light;
        public Vector2 position;
        public bool enabled;
        public int health;
        public int elevation;
        public int conditions;
        public bool locked;
        public float rotation;
        public float lightRotation;
        public bool lightEnabled;
        public bool teleportProtection;
        public bool synced;
        public string parentInstance;
        public string effect;
    }

    [Serializable]
    public enum TokenType
    {
        Character,
        Mount,
        Item
    }

    [Serializable]
    public struct Permission
    {
        public string user;
        public PermissionType type;

        public Permission(string _user, PermissionType _type)
        {
            user = _user;
            type = _type;
        }
    }

    [Serializable]
    public struct Visible
    {
        public string user;
        public bool visible;

        public Visible(string _user, bool _visible)
        {
            user = _user;
            visible = _visible;
        }
    }

    [Serializable]
    public enum PermissionType
    {
        None,
        Observer,
        Controller
    }

    [Serializable]
    public struct MovementData
    {
        public string id;
        public List<Vector2> points;
        public bool teleport;

        public MovementData(string _id, List<Vector2> _points, bool _teleport)
        {
            id = _id;
            points = _points;
            teleport = _teleport;
        }
    }
}