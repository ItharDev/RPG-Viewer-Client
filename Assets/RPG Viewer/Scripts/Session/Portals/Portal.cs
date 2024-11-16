using System;
using System.Collections.Generic;
using Networking;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vectrosity;

namespace RPG
{
    public class Portal : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private Image icon;
        [SerializeField] private Sprite regularIcon;
        [SerializeField] private Sprite modifyIcon;
        [SerializeField] private Image radiusOutline;
        [SerializeField] private GameObject radiusIndicator;
        [SerializeField] private Color disabledColor;
        [SerializeField] private Color activeColor;
        [SerializeField] private Color normalColor;
        [SerializeField] private LayerMask blockingLayers;
        [SerializeField] private PortalConfiguration configPrefab;

        private PortalConfiguration activeConfig;

        public string Id;
        public PortalData Data;

        public bool IsProximity => Data.continuous;
        public bool IsStationary => !Data.continuous;
        public bool IsActive => Data.active;

        private Canvas canvas;
        private CircleCollider2D radiusCollider;
        private Portal linkedPortal;
        private bool dragging;
        private bool linking;
        private Vector2 linkPosition;

        private VectorLine linkLine;
        private Color lineColor => Data.active ? activeColor : disabledColor;

        private void Awake()
        {
            // Get reference of our canvas
            if (canvas == null) canvas = GetComponentInChildren<Canvas>();
            if (radiusCollider == null) radiusCollider = GetComponent<CircleCollider2D>();
        }
        private void OnEnable()
        {
            // Add event listeners
            Events.OnSettingChanged.AddListener(ToggleUI);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSettingChanged.RemoveListener(ToggleUI);
        }
        private void OnDestroy()
        {
            if (linkLine != null) Destroy(linkLine.rectTransform.gameObject);
        }
        private void Update()
        {
            if (dragging)
            {
                // Update our position when being dragged
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePos.z = 0.0f;
                transform.localPosition = mousePos;
            }

            if (linkLine != null)
            {
                if (!linking && linkedPortal == null)
                {
                    linkLine = null;
                }
            }
            linkLine?.SetWidth(40.0f / Camera.main.orthographicSize * (Screen.currentResolution.width / 1600));
        }
        private void LateUpdate()
        {
            DrawLink();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsActive || linkedPortal == null) return;
            if (other.CompareTag("Token"))
            {
                Vector2 direction = other.transform.position - transform.position;
                if (Physics2D.Raycast(transform.position, direction, direction.magnitude, blockingLayers).collider != null) return;
                if (!other.TryGetComponent<TokenMovement>(out var token)) return;
                token.EnterPortal(this);
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Token"))
            {
                if (!other.TryGetComponent<TokenMovement>(out var token)) return;
                token.ExitPortal(this);
            }
        }

        private void ToggleUI(Setting setting)
        {
            bool enabled = SettingsHandler.Instance.Setting.ToString().ToLower().Contains("portal");
            if (linkLine != null) linkLine.active = enabled;
            if (!enabled) icon.color = normalColor;
            else icon.color = Data.active ? activeColor : disabledColor;

            icon.sprite = enabled ? modifyIcon : regularIcon;
            canvas.enabled = (Data.active && Data.visible) || enabled;
            canvas.sortingLayerName = enabled ? "Above Fog" : "Default";
            radiusIndicator.SetActive(enabled);
        }

        public void LoadData(string id, PortalData data)
        {
            Id = id;
            Data = data;

            bool enabled = SettingsHandler.Instance.Setting.ToString().ToLower().Contains("portal");
            if (!enabled) icon.color = normalColor;
            else icon.color = Data.active ? activeColor : disabledColor;

            icon.sprite = enabled ? modifyIcon : regularIcon;
            radiusOutline.color = Data.active ? activeColor : disabledColor;

            float cellSize = Session.Instance.Grid.CellSize;

            // Update our position and scale
            transform.position = Data.position;
            canvas.transform.localScale = new Vector3(cellSize * 0.03f, cellSize * 0.03f, 1.0f);
            radiusCollider.radius = data.radius / Session.Instance.Grid.Unit.scale * Session.Instance.Grid.CellSize;
            radiusOutline.transform.parent.localScale = new Vector3(2.0f * radiusCollider.radius / canvas.transform.localScale.x, 2.0f * radiusCollider.radius / canvas.transform.localScale.y, 1.0f);
            canvas.enabled = (Data.active && Data.visible) || enabled;
            canvas.sortingLayerName = enabled ? "Above Fog" : "Default";
            radiusIndicator.SetActive(enabled);
        }

        public void CreateLink()
        {
            if (string.IsNullOrEmpty(Data.link)) return;

            // Get the linked portal
            if (PortalManager.Instance.Portals.TryGetValue(Data.link, out Portal portal)) linkedPortal = portal;

            DrawLink();
            bool enabled = SettingsHandler.Instance.Setting == Setting.Portals_Link;
            if (linkLine != null) linkLine.active = enabled;
        }

        private void DrawLink()
        {
            if (linkedPortal == null && !linking) return;

            Vector2 endPoint = linkPosition == Vector2.zero ? linking ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : linkedPortal.transform.position : linkPosition;
            Vector2 startPoint = transform.position;

            Vector3 adjustedStartPoint = startPoint + 0.35f * Session.Instance.Grid.CellSize * (endPoint - startPoint).normalized;
            Vector3 adjustedEndPoint = endPoint + 0.35f * Session.Instance.Grid.CellSize * (startPoint - endPoint).normalized;

            adjustedStartPoint.z = adjustedEndPoint.z = -2.0f;

            List<Vector3> points = new List<Vector3>
            {
                adjustedStartPoint,
                adjustedEndPoint
            };

            if (linkLine == null)
            {
                linkLine = new VectorLine("Link arrow", points, 1.0f);
                linkLine.lineType = LineType.Continuous;
                linkLine.rectTransform.gameObject.layer = 5;
                linkLine.endCap = "Arrow";
                linkLine.color = lineColor;
            }
            else
            {
                linkLine.points3 = points;
                linkLine.color = lineColor;
            }

            linkLine.Draw3D();
        }

        public void Link(Portal portal)
        {
            Data.link = portal.Id;
            linkedPortal = portal;

            DrawLink();
            bool enabled = SettingsHandler.Instance.Setting == Setting.Portals_Link;
            if (linkLine != null) linkLine.active = enabled;
        }

        public void SetActive(bool active)
        {
            bool enabled = SettingsHandler.Instance.Setting.ToString().ToLower().Contains("portal");
            Data.active = active;
            if (!enabled) icon.color = normalColor;
            else icon.color = active ? activeColor : disabledColor;

            icon.sprite = enabled ? modifyIcon : regularIcon;
            radiusOutline.color = Data.active ? activeColor : disabledColor;
            canvas.enabled = (active && Data.visible) || enabled;
        }

        public void UpdatePosition(Vector2 position)
        {
            Data.position = position;
            transform.position = position;
        }

        public void BeginDrag(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;

            if (pointerData.button != PointerEventData.InputButton.Left) return;

            dragging = true;
        }
        public void EndDrag(BaseEventData eventData)
        {
            dragging = false;
            Vector2 position = transform.localPosition;
            SocketManager.EmitAsync("move-portal", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    return;
                }

                transform.localPosition = position;
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Id, JsonUtility.ToJson(position));
        }
        public void OnClick(BaseEventData eventData)
        {
            // Get pointer data
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.dragging) return;

            if (pointerData.button == PointerEventData.InputButton.Left)
            {
                if (PortalManager.Instance.Mode == PortalMode.Create) TogglePortal();
                else if (PortalManager.Instance.Mode == PortalMode.Link) linking = PortalManager.Instance.SelectLink(this);
                else DeletePortal();
            }
            else if (pointerData.button == PointerEventData.InputButton.Right) OpenConfiguration();
        }

        private void OpenConfiguration()
        {
            if (activeConfig != null) return;

            activeConfig = Instantiate(configPrefab);
            activeConfig.transform.SetParent(UICanvas.Instance.transform);
            activeConfig.transform.localPosition = Vector3.zero;
            activeConfig.transform.SetAsLastSibling();
            activeConfig.OpenPanel(Data, (data) =>
            {
                data.position = transform.position;
                data.link = Data.link;
                data.id = Id;
                SocketManager.EmitAsync("modify-portal", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean()) return;

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Id, JsonUtility.ToJson(data));
            });
        }

        public void OnHoverEnter(BaseEventData eventData)
        {
            if (!PortalManager.Instance.Linking) return;
            PortalManager.Instance.HoverLink(this);
        }
        public void OnHoverExit(BaseEventData eventData)
        {
            if (!PortalManager.Instance.Linking) return;
            PortalManager.Instance.HoverLink(null);
        }

        public void EndLinking()
        {
            linking = false;
            VectorLine.Destroy(ref linkLine);
        }

        private void TogglePortal()
        {
            SocketManager.EmitAsync("activate-portal", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Id, !Data.active);
        }

        private void DeletePortal()
        {
            SocketManager.EmitAsync("remove-portal", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, Id);
        }

        public void ApplyLink(Vector2 position)
        {
            linkPosition = position;
        }

        public void UpdateData(PortalData data)
        {
            Data = data;
            bool enabled = SettingsHandler.Instance.Setting.ToString().ToLower().Contains("portal");
            if (!enabled) icon.color = normalColor;
            else icon.color = Data.active ? activeColor : disabledColor;

            icon.sprite = enabled ? modifyIcon : regularIcon;
            canvas.enabled = (Data.active && Data.visible) || enabled;
            radiusOutline.color = icon.color;

            // Update our position and scale
            transform.position = Data.position;
            radiusCollider.radius = data.radius / Session.Instance.Grid.Unit.scale * Session.Instance.Grid.CellSize;
            radiusOutline.transform.parent.localScale = new Vector3(2.0f * radiusCollider.radius / canvas.transform.localScale.x, 2.0f * radiusCollider.radius / canvas.transform.localScale.y, 1.0f);

            CreateLink();
        }
    }

    [Serializable]
    public struct PortalData
    {
        public string id;
        public Vector2 position;
        public float radius;
        public bool active;
        public bool visible;
        public string link;
        public bool continuous;
    }
}