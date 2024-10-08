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
        [SerializeField] private Color disabledColor;
        [SerializeField] private Color activeColor;

        public string Id;

        private Canvas canvas;
        private PortalData data;
        private Portal linkedPortal;
        private bool dragging;
        private bool linking;
        private Vector2 linkPosition;

        private VectorLine linkLine;
        private Color lineColor => data.active ? activeColor : disabledColor;

        private void Awake()
        {
            // Get reference of our canvas
            if (canvas == null) canvas = GetComponentInChildren<Canvas>();
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

        private void ToggleUI(Setting setting)
        {
            bool enabled = SettingsHandler.Instance.Setting == Setting.Portals_Link;
            if (linkLine != null) linkLine.active = enabled;
        }

        public void LoadData(string _id, PortalData _data)
        {
            Id = _id;
            data = _data;

            icon.color = data.active ? activeColor : disabledColor;

            float cellSize = Session.Instance.Grid.CellSize;

            // Update our position and scale
            transform.position = data.position;
            canvas.transform.localScale = new Vector3(cellSize * 0.03f, cellSize * 0.03f, 1.0f);
        }

        public void CreateLink()
        {
            if (string.IsNullOrEmpty(data.link)) return;

            // Get the linked portal
            if (PortalManager.Instance.Portals.TryGetValue(data.link, out Portal portal)) linkedPortal = portal;

            DrawLink();
            bool enabled = SettingsHandler.Instance.Setting == Setting.Portals_Link;
            if (linkLine != null) linkLine.active = enabled;
        }

        private void DrawLink()
        {
            if (linkedPortal == null && !linking) return;

            Vector2 endPoint = linkPosition == Vector2.zero ? linking ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : linkedPortal.transform.position : linkPosition;
            Vector2 startPoint = transform.position;

            Vector2 adjustedStartPoint = Camera.main.WorldToScreenPoint(startPoint + 0.35f * Session.Instance.Grid.CellSize * (endPoint - startPoint).normalized);
            Vector2 adjustedEndPoint = Camera.main.WorldToScreenPoint(endPoint + 0.35f * Session.Instance.Grid.CellSize * (startPoint - endPoint).normalized);

            List<Vector2> points = new List<Vector2>
            {
                adjustedStartPoint,
                adjustedEndPoint
            };
            if (linkLine == null)
            {
                linkLine = new VectorLine("Link arrow", points, 1.0f, LineType.Continuous, Joins.Weld);
                linkLine.rectTransform.gameObject.layer = 5;
                linkLine.endCap = "Arrow";
                linkLine.color = lineColor;
            }
            else
            {
                linkLine.points2 = points;
                linkLine.color = lineColor;
            }

            linkLine.Draw();
        }

        public void Link(Portal portal)
        {
            data.link = portal.Id;
            linkedPortal = portal;

            DrawLink();
            bool enabled = SettingsHandler.Instance.Setting == Setting.Portals_Link;
            if (linkLine != null) linkLine.active = enabled;
        }

        public void SetEnabled(bool active)
        {
            data.active = active;
            icon.color = active ? activeColor : disabledColor;
        }

        public void UpdatePosition(Vector2 position)
        {
            data.position = position;
            transform.position = position;
        }

        public void UpdateRadius(float radius)
        {
            data.radius = radius;
        }

        public void SetContinuous(bool continuo)
        {
            data.continuous = continuo;
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
            }, Id, !data.active);
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
    }

    [Serializable]
    public struct PortalData
    {
        public string id;
        public Vector2 position;
        public float radius;
        public bool active;
        public string link;
        public bool continuous;
    }
}