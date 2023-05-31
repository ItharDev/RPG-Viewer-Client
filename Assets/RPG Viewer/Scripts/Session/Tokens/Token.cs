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

        public TokenMovement Movement { get; private set; }
        public TokenVision Vision { get; private set; }
        public TokenUI UI { get; private set; }
        public TokenConditions Conditions { get; private set; }

        public bool IsOwner
        {
            get { return Permission.type == PermissionType.Owner; }
        }

        public Permission Permission;
        public bool Selected;

        private void OnEnable()
        {
            // Get child references
            if (Movement == null) Movement = GetComponent<TokenMovement>();
            if (Vision == null) Vision = GetComponent<TokenVision>();
            if (UI == null) UI = GetComponent<TokenUI>();
            if (Conditions == null) Conditions = GetComponent<TokenConditions>();
        }

        private void Update()
        {
            // Return if token is not selected or we are edting any fields
            if (!Selected || UI.Editing) return;

            HandleDeletion();
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
            float radius = 0.009f * Data.dimensions.x >= Data.dimensions.y ? UI.Rect.sizeDelta.x : UI.Rect.sizeDelta.y;
            Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(radius, radius), 360, mountLayers);

            // Loop through each collider
            for (var i = 0; i < colliders.Length; i++)
            {
                Token token = colliders[i].GetComponent<Token>();

                // Don't count in ourselves
                if (token == this || token == null) continue;
                if (token.Data.type == TokenType.Character) mounted.Add(token);
            }

            return mounted;
        }
        public void DisableCollider()
        {
            // Disable collisions
            boxCollider.enabled = false;
        }
        public void SelectToken()
        {
            Selected = true;
        }
        public void DeselectToken()
        {
            Selected = false;
        }

        public void LoadData(TokenData data, Sprite sprite)
        {
            // Update data and set permissions
            Data = data;
            SetPermission();

            // Reload UI
            if (sprite != null) UI.SetImage(sprite);
            UI.Reload();

            // Update conditions
            Conditions.SetConditions(data.conditions);

            // Load lighting and vision
            Vision.Reload();
        }

        private void SetPermission()
        {
            // Set as owner if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                Permission = new Permission(GameData.User.id, PermissionType.Owner);
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
        public void EnableToken(bool enabled)
        {
            Data.enabled = enabled;

            // Enable / disable vision and image
            Vision.Reload();
            UI.SetAlpha(enabled ? 1.0f : 0.5f);
        }
        public void SetElevation(string elevation)
        {
            Data.elevation = elevation;
            // TODO: Update elevation input
        }
        public void SetLocked(bool locked)
        {
            Data.locked = locked;
            // TODO: Update locked icon
        }
        public void SetHealth(int health)
        {
            Data.health = health;
            // TODO: Update health
        }

        #region Buttons
        public void DeleteToken()
        {
            // Check if any player is the owner
            Permission owner = Data.permissions.FirstOrDefault(value => value.type == PermissionType.Owner);
            if (!string.IsNullOrEmpty(owner.user))
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
        public string id; //
        public string name; //
        public TokenType type; //
        public List<Permission> permissions; //
        public List<Permission> visible; //
        public Vector2Int dimensions; //
        public float visionRadius; //
        public float nightRadius; //
        public string image; //
        public Vector2 position;
        public bool enabled;
        public int health;
        public string elevation;
        public int conditions;
        public bool locked;
        public float rotation;
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
        Owner
    }

    [Serializable]
    public struct MovementData
    {
        public string id;
        public List<Vector2> points;

        public MovementData(string _id, List<Vector2> _points)
        {
            id = _id;
            points = _points;
        }
    }
}