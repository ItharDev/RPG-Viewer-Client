using System.Collections.Generic;
using Networking;
using UnityEngine;
using Vectrosity;

namespace RPG
{
    public class LineController : MonoBehaviour
    {
        [SerializeField] private PointController pointPrefab;
        [SerializeField] private Color wallColor;
        [SerializeField] private Color doorColor;
        [SerializeField] private Color invisibleColor;
        [SerializeField] private Color environmentColor;
        [SerializeField] private Color hiddenDoorColor;
        [SerializeField] private Color fogColor;
        [SerializeField] private Texture lineTexture;
        [SerializeField] private new EdgeCollider2D collider2D;
        [SerializeField] private WallConfiguration configurationPrefab;

        public WallData Data;
        public WallType Type;
        public List<PointController> Points = new List<PointController>();

        private WallConfiguration activeConfig;
        private bool locked;
        private bool open;
        private Color color;
        private VectorLine line;

        private PointController draggedPoint;
        private bool dragging;
        private bool moving;
        private bool continuing;
        private bool hovered;
        private int selectedIndex;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnPointDragged.AddListener(DragPoint);
            Events.OnPointContinued.AddListener(ContinuePoint);
            Events.OnLineHovered.AddListener(HandleSorting);
            Events.OnPointDeleted.AddListener(DeletePoint);
            Events.OnSettingChanged.AddListener(ToggleUI);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnPointDragged.RemoveListener(DragPoint);
            Events.OnPointContinued.RemoveListener(ContinuePoint);
            Events.OnLineHovered.RemoveListener(HandleSorting);
            Events.OnPointDeleted.RemoveListener(DeletePoint);
            Events.OnSettingChanged.RemoveListener(ToggleUI);

        }
        private void Update()
        {
            if (hovered && Input.GetMouseButtonUp(1))
            {
                if (selectedIndex != -1 && line.active) ConfigureWall();
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (dragging) dragging = false;
                if (continuing)
                {
                    continuing = false;
                    WallData newData = GetData();

                    SocketManager.EmitAsync("modify-wall", (callback) =>
                    {
                        // Check if the event was successful
                        if (callback.GetValue().GetBoolean())
                        {
                            Data = newData;
                            return;
                        }

                        // Send error message
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                        Type = Data.type;
                        locked = Data.locked;
                        LoadType(Type);
                    }, JsonUtility.ToJson(newData));
                }
                if (moving)
                {
                    moving = false;
                    WallData newData = GetData();

                    SocketManager.EmitAsync("modify-wall", (callback) =>
                    {
                        // Check if the event was successful
                        if (callback.GetValue().GetBoolean())
                        {
                            Data = newData;
                            return;
                        }

                        // Send error message
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                        Type = Data.type;
                        locked = Data.locked;
                        LoadType(Type);
                    }, JsonUtility.ToJson(newData));
                }
            }
            if (dragging)
            {
                // Update line texture
                UpdateLine();
            }

            if (line == null) return;

            line.Selected(Input.mousePosition, 10, out selectedIndex);
            if (selectedIndex != -1 && !hovered) Events.OnLineHovered?.Invoke(this);
            if (selectedIndex == -1 && hovered) Events.OnLineHovered?.Invoke(null);
        }

        private void HandleSorting(LineController controller)
        {
            if (controller == null)
            {
                hovered = false;
                transform.localPosition = new Vector3(0.0f, 0.0f, -1.0f);
                return;
            }

            hovered = controller == this;
            transform.localPosition = new Vector3(0.0f, 0.0f, hovered ? -1.1f : -1.0f);
        }
        public void DeletePoint(PointController point)
        {
            // Return if the point deleted is not ours
            if (!Points.Contains(point)) return;

            // Remove point from the list
            Points.Remove(point);
            Destroy(point.gameObject);

            // Update line texture
            UpdateLine();

            // Delete this wall if only single point is left
            if (Points.Count <= 1)
            {
                DestroyLine();
                WallTools.Instance.RemoveWall(this);
            }
            else
            {
                WallData newData = GetData();
                SocketManager.EmitAsync("modify-wall", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean())
                    {
                        Data = newData;
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    Type = Data.type;
                    locked = Data.locked;
                    LoadType(Type);
                }, JsonUtility.ToJson(newData));
            }
        }
        private void ToggleUI(Setting setting)
        {
            bool enabled = setting.ToString().ToLower().Contains("walls");
            if (line == null) return;
            line.active = enabled;
        }
        private void UpdateLine()
        {
            if (Points.Count >= 2)
            {
                // initialise lists
                List<Vector3> list3D = new List<Vector3>();
                List<Vector2> list2D = new List<Vector2>();

                foreach (var point in Points)
                {
                    // Return if the point is null
                    if (point == null) continue;

                    // Add point's position to lists
                    list3D.Add(new Vector3(point.transform.position.x, point.transform.position.y, -2.0f));
                    list2D.Add(point.transform.position);
                }

                // Create line if it's not yet initialised
                if (line == null)
                {
                    line = new VectorLine("Wall controller", list3D, 10.0f / Camera.main.orthographicSize);
                    line.lineType = LineType.Continuous;
                    line.color = color;
                }
                else
                {
                    // Update points 
                    line.SetWidth(10.0f / Camera.main.orthographicSize);
                    line.points3 = list3D;
                    collider2D.SetPoints(list2D);
                }

                // Redraw line texture, using the new points
                line.Draw3D();
            }
        }

        public void Initialise(WallType _type, bool initialiseWalls = true)
        {
            Type = _type;
            locked = false;

            LoadType(Type);

            if (!initialiseWalls) return;

            // Initialise the first and second points to mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = -1.0f;
            PointController initialPoint = Instantiate(pointPrefab, transform);
            initialPoint.transform.localPosition = mousePos;
            Points.Add(initialPoint);
            initialPoint.Initialise(color, this, true, false);

            draggedPoint = Instantiate(pointPrefab, transform);
            draggedPoint.transform.localPosition = mousePos;
            Points.Add(draggedPoint);
            draggedPoint.Initialise(color, this, false, false);

            dragging = true;

            bool enabled = SettingsHandler.Instance.Setting.ToString().ToLower().Contains("walls");
            if (line == null) return;
            line.active = enabled;
        }
        public void Initialise(WallData _data)
        {
            Data = _data;
            Type = _data.type;
            locked = _data.locked;

            LoadType(Type);
            LoadPoints();

            bool enabled = SettingsHandler.Instance.Setting.ToString().ToLower().Contains("walls");
            if (line == null) return;
            line.active = enabled;
        }
        public void AddPoint(Vector3 position)
        {
            PointController point = Instantiate(pointPrefab, transform);
            point.transform.localPosition = position;
            Points.Add(point);
            point.Initialise(color, this, true);
            UpdateLine();

        }
        public WallData GetData()
        {
            List<Vector2> points = new List<Vector2>();
            for (int i = 0; i < Points.Count; i++)
            {
                points.Add(Points[i].transform.localPosition);
            }
            return new WallData(Data.id, points, Type, open, locked);
        }
        private void LoadType(WallType type)
        {
            // Define wall color
            switch (type)
            {
                case WallType.Wall:
                    color = wallColor;
                    break;
                case WallType.Door:
                    color = doorColor;
                    break;
                case WallType.Hidden_Door:
                    color = hiddenDoorColor;
                    break;
                case WallType.Invisible:
                    color = invisibleColor;
                    break;
                case WallType.Fog:
                    color = fogColor;
                    break;
                case WallType.Environment:
                    color = environmentColor;
                    break;
            }

            UpdateColor();
        }
        private void UpdateColor()
        {
            if (line != null) line.color = color;
            for (int i = 0; i < Points.Count; i++)
            {
                Points[i].UpdateColor(color);
            }

            UpdateLine();
        }
        private void LoadPoints()
        {
            for (int i = 0; i < Data.points.Count; i++)
            {
                PointController point = Instantiate(pointPrefab, transform);
                point.transform.localPosition = new Vector3(Data.points[i].x, Data.points[i].y, -1.0f);
                Points.Add(point);
                point.Initialise(color, this, true);
            }

            UpdateLine();
        }
        public void ConfigureWall()
        {
            // Return if there is an active config 
            if (activeConfig != null) return;

            activeConfig = Instantiate(configurationPrefab);
            activeConfig.transform.SetParent(UICanvas.Instance.transform);
            activeConfig.transform.localPosition = Vector3.zero;
            activeConfig.transform.SetAsLastSibling();
            activeConfig.OpenPanel(Type, locked, (_type, _locked) =>
            {
                Type = _type;
                locked = _locked;
                LoadType(Type);
                WallData newData = GetData();

                SocketManager.EmitAsync("modify-wall", (callback) =>
                {
                    // Check if the event was successful
                    if (callback.GetValue().GetBoolean())
                    {
                        Data = newData;
                        return;
                    }

                    // Send error message
                    MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    Type = Data.type;
                    locked = Data.locked;
                    LoadType(Type);
                }, JsonUtility.ToJson(newData));
            });
        }

        private void DragPoint(PointController point)
        {
            // Update our currently dragged point
            if (Points.Contains(point))
            {
                dragging = true;
                draggedPoint = point;
                moving = true;
                return;
            }

            // No point is being dragged
            draggedPoint = null;
            dragging = false;
        }
        private void ContinuePoint(PointController point)
        {
            // Return if the dragged point is not ours
            if (!Points.Contains(point)) return;

            // Return if the point is in the middle of the wall
            if (Points.IndexOf(point) > 0 && Points.IndexOf(point) < Points.Count - 1) return;

            // Initialise the first and second points to mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = -1.0f;
            draggedPoint = Instantiate(pointPrefab, transform);
            draggedPoint.transform.localPosition = mousePos;
            if (Points.IndexOf(point) == 0)
            {
                Points.Insert(0, draggedPoint);
            }
            else if (Points.IndexOf(point) == Points.Count - 1)
            {
                Points.Add(draggedPoint);
            }

            draggedPoint.Initialise(color, this, false);
            dragging = true;
            continuing = true;
        }

        public void DestroyLine()
        {
            if (line != null) Destroy(line.rectTransform.gameObject);
        }
    }
}