using System.Collections.Generic;
using System.Linq;
using Networking;
using UnityEngine;

namespace RPG
{
    public class PortalManager : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Portal portalPrefab;
        [SerializeField] private Transform portalParent;

        public static PortalManager Instance { get; private set; }
        public Dictionary<string, Portal> Portals = new Dictionary<string, Portal>();
        public PortalMode Mode;
        public bool Linking => linkSource != null;

        private bool interactable;
        private Portal linkSource;

        private float defaultRadius => Session.Instance.Grid.Unit.scale;

        private void Awake()
        {
            Instance = this;
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnPortalCreated.AddListener(CreatePortal);
            Events.OnPortalModified.AddListener(ModifyPortal);
            Events.OnPortalMoved.AddListener(MovePortal);
            Events.OnPortalEnabled.AddListener(EnablePortal);
            Events.OnPortalRemoved.AddListener(RemovePortal);
            Events.OnPortalLinked.AddListener(LinkPortal);
            Events.OnStateChanged.AddListener(ReloadPortals);
            Events.OnSceneLoaded.AddListener(LoadPortals);
            Events.OnSettingChanged.AddListener(ToggleUI);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnPortalCreated.RemoveListener(CreatePortal);
            Events.OnPortalModified.RemoveListener(ModifyPortal);
            Events.OnPortalMoved.RemoveListener(MovePortal);
            Events.OnPortalEnabled.RemoveListener(EnablePortal);
            Events.OnPortalRemoved.RemoveListener(RemovePortal);
            Events.OnPortalLinked.RemoveListener(LinkPortal);
            Events.OnStateChanged.RemoveListener(ReloadPortals);
            Events.OnSceneLoaded.RemoveListener(LoadPortals);
            Events.OnSettingChanged.RemoveListener(ToggleUI);
        }
        private void Update()
        {
            if (!interactable || Mode == PortalMode.Delete) return;
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonUp(0)) CreatePortal();
            if (Input.GetKeyDown(KeyCode.Escape) && linkSource != null)
            {
                linkSource.EndLinking();
                linkSource = null;
            }
        }

        private void ToggleUI(Setting setting)
        {
            bool enabled = setting.ToString().ToLower().Contains("portals");
            canvasGroup.blocksRaycasts = enabled;
            interactable = enabled;

            switch (setting)
            {
                case Setting.Portals_Create:
                    Mode = PortalMode.Create;
                    break;
                case Setting.Portals_Delete:
                    Mode = PortalMode.Delete;
                    break;
                case Setting.Portals_Link:
                    Mode = PortalMode.Link;
                    break;
            }
        }

        public bool SelectLink(Portal portal)
        {
            if (!Portals.ContainsValue(portal)) return false;
            if (linkSource == null)
            {
                linkSource = portal;
                return true;
            }

            SocketManager.EmitAsync("link-portal", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, linkSource.Id, portal.Id);

            linkSource.EndLinking();
            linkSource = null;
            return false;
        }

        private void CreatePortal()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            SocketManager.EmitAsync("create-portal", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(mousePos), defaultRadius);
        }

        private void CreatePortal(string id, PortalData data)
        {
            // Instantiate portal and load its data
            Portal portal = Instantiate(portalPrefab, portalParent);
            portal.LoadData(id, data);

            // Store portal to dictionary
            Portals.Add(id, portal);
        }

        private void ModifyPortal(string id, PortalData data)
        {
            // Check if the portal exists
            if (Portals.ContainsKey(id))
            {
                // Move the portal to the new position
                Portals[id].UpdateData(data);
            }
        }

        private void MovePortal(string id, Vector2 position)
        {
            // Check if the portal exists
            if (Portals.ContainsKey(id))
            {
                // Move the portal to the new position
                Portals[id].UpdatePosition(position);
            }
        }

        private void EnablePortal(string id, bool active)
        {
            // Check if the portal exists
            if (Portals.ContainsKey(id))
            {
                // Enable or disable the portal
                Portals[id].SetActive(active);
            }
        }

        private void RemovePortal(string id)
        {
            // Check if the portal exists
            if (!Portals.ContainsKey(id)) return;

            foreach (var portal in Portals)
            {
                if (portal.Value.Data.link == id) portal.Value.RemoveLink();
            }

            // Destroy the portal
            Destroy(Portals[id].gameObject);
            Portals.Remove(id);
        }

        private void LinkPortal(string source, string destination)
        {
            // Check if the portal exists
            if (Portals.ContainsKey(source) && Portals.ContainsKey(destination))
            {
                // Link the portal to another portal
                Portals[source].Link(Portals[destination]);
            }
        }

        private void ReloadPortals(SessionState oldState, SessionState newState)
        {
            // Check if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadPortals();
            }
            else
            {
                // Unload portals if syncing was disabled
                if (!newState.synced)
                {
                    UnloadPortals();
                    return;
                }
            }
        }

        private void UnloadPortals()
        {
            // Clear lists
            Portals.Clear();
            foreach (Transform child in portalParent) Destroy(child.gameObject);
        }
        private void LoadPortals(SceneData settings)
        {
            // Instantiate portals
            List<PortalData> list = settings.portals;
            for (int i = 0; i < list.Count; i++)
            {
                CreatePortal(list[i]);
            }

            // Link portals
            for (int i = 0; i < Portals.Count; i++)
            {
                Portals.ElementAt(i).Value.CreateLink();
            }
        }

        private void CreatePortal(PortalData data)
        {
            // Instantiate portal and load its data
            Portal portal = Instantiate(portalPrefab, portalParent);
            portal.LoadData(data.id, data);

            // Store portal to dictionary
            Portals.Add(data.id, portal);
        }

        public void HoverLink(Portal portal)
        {
            if (linkSource == null || linkSource == portal) return;
            linkSource.ApplyLink(portal == null ? Vector2.zero : portal.transform.position);
        }
    }

    public enum PortalMode
    {
        Create,
        Link,
        Delete
    }
}