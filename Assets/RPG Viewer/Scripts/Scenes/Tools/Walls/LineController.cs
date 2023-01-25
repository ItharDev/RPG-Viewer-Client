using System.Collections.Generic;
using UnityEngine;
using Vectrosity;
using MongoDB.Bson;

namespace RPG
{
    public class LineController : MonoBehaviour
    {
        [SerializeField] private PointController dotPrefab;
        [SerializeField] private Texture lineTexture;

        [SerializeField] private Color selectedColor;
        [SerializeField] private Color normalColor;

        private Color color;
        private bool hidden;

        private VectorLine line;
        private new EdgeCollider2D collider2D;

        public List<PointController> points = new List<PointController>();
        public PointController SelectedPoint;

        private PointController dragPoint;
        private PointController hoverPoint;

        private Vector2 startPos = Vector2.zero;
        private WallType type;

        public WallTools Tools;

        private void Update()
        {
            if (line == null) return;

            if (line.Selected(Input.mousePosition))
            {
                Tools.SendTop(this);
            }

            if (SelectedPoint != null || dragPoint != null)
            {
                foreach (var point in points)
                {
                    point.transform.position = new Vector3(point.transform.position.x, point.transform.position.y, -1);
                }
            }
            else
            {
                foreach (var point in points)
                {
                    point.transform.position = new Vector3(point.transform.position.x, point.transform.position.y, 0);
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape)) SelectedPoint = null;
        }

        private void LateUpdate()
        {
            if (line != null)
            {
                line.SetWidth((15.0f / Camera.main.orthographicSize) * (Screen.currentResolution.width / 1600));
            }

            if (Input.GetKey(KeyCode.LeftControl) && hoverPoint != null && Input.GetMouseButtonDown(0))
            {
                if (points.IndexOf(hoverPoint) == points.Count - 1)
                {
                    dragPoint = AddPoint(GetMousePosition());
                }
                else if (points.IndexOf(hoverPoint) == 0)
                {
                    dragPoint = AddPoint(GetMousePosition(), true);
                }
            }
            if (Input.GetMouseButtonDown(0))
            {
                if (SelectedPoint != null)
                {
                    if (!RectTransformUtility.RectangleContainsScreenPoint(SelectedPoint.GetComponent<RectTransform>(), GetMousePosition()))
                    {
                        SelectedPoint.Outline.color = normalColor;
                        SelectedPoint = null;
                    }
                }
            }
            if (Input.GetMouseButton(0))
            {
                if (startPos != Vector2.zero)
                {
                    if (Vector2.Distance(startPos, (Vector2)GetMousePosition()) > 0.1f)
                    {
                        AddPoint(startPos);
                        dragPoint = AddPoint(GetMousePosition());
                        startPos = Vector2.zero;
                    }
                }

                if (dragPoint != null) MovePoint(dragPoint);
            }
            if (Input.GetMouseButtonUp(0))
            {
                if (points.Count >= 2)
                {
                    if (dragPoint != null) EndDrag(dragPoint);
                    dragPoint = null;
                }
                if (points.Count <= 1)
                {
                    Destroy(gameObject);
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace) && SelectedPoint != null)
            {
                RemovePoint(SelectedPoint);
                SelectedPoint = null;
            }
        }

        public void Init(WallType type, WallTools tools, Color color)
        {
            this.type = type;
            Tools = tools;
            this.color = color;
            hidden = false;

            collider2D = GetComponent<EdgeCollider2D>();
            startPos = GetMousePosition();
        }
        public void Load(WallType type, bool hidden, WallTools tools, Color color)
        {
            this.type = type;
            Tools = tools;
            this.color = color;
            collider2D = GetComponent<EdgeCollider2D>();
            this.hidden = hidden;
        }
        public void Hide(bool active)
        {
            foreach (var point in points)
            {
                point.gameObject.SetActive(active);
            }
            line.active = active;
        }
        public void SendTop(bool selected)
        {
            foreach (var point in points)
            {
                point.GetComponentInChildren<Canvas>().sortingOrder = selected ? 2 : 1;
            }
        }

        public PointController AddPoint(Vector2 pos, bool atStart = false)
        {
            PointController point = Instantiate(dotPrefab, pos, Quaternion.identity, transform);

            point.Image.color = color;
            point.Line = this;

            point.OnBeginDragEvent += BeginDrag;
            point.OnDragEvent += MovePoint;
            point.OnEndDragEvent += EndDrag;
            point.OnClickEvent += SelectPoint;
            point.OnEnterEvent += (point) =>
            {
                Tools.HoverPoint = point;
                hoverPoint = point;
            };
            point.OnExitEvent += (point) =>
            {
                Debug.Log("F");
                Tools.HoverPoint = null;
                hoverPoint = null;
            };

            if (atStart) points.Insert(0, point);
            else points.Add(point);
            UpdatePoints();

            return point;
        }
        public void RemovePoint(PointController point)
        {
            points.Remove(point);
            Destroy(point.gameObject);
            UpdatePoints();
            if (points.Count <= 1)
            {
                Destroy(line.rectTransform.gameObject);
                Destroy(gameObject);
            }
        }
        public WallData SaveData()
        {
            List<Vector2> pts = new List<Vector2>();
            foreach (var p in line.points3) pts.Add(p);

            return new WallData()
            {
                wallId = ObjectId.GenerateNewId().ToString(),
                points = pts,
                model = type,
                open = this.hidden
            };
        }

        private void BeginDrag(PointController point)
        {
            SelectedPoint = point;
            MovePoint(point);
        }
        private void MovePoint(PointController point)
        {
            if (Tools.HoverPoint == point) Tools.HoverPoint = null;
            point.transform.position = GetMousePosition();
            UpdatePoints();
        }
        private void EndDrag(PointController point)
        {
            if (Tools.HoverPoint != null) point.transform.position = Tools.HoverPoint.transform.position;
            SelectedPoint = null;
            UpdatePoints();
        }
        private void SelectPoint(PointController point)
        {
            if (SelectedPoint != point && SelectedPoint != null)
            {
                SelectedPoint.Outline.color = normalColor;
            }
            SelectedPoint = point;
            SelectedPoint.Outline.color = selectedColor;
        }

        private void UpdatePoints()
        {
            if (points.Count >= 2)
            {
                List<Vector3> list3D = new List<Vector3>();
                List<Vector2> list2D = new List<Vector2>();

                foreach (var p in points)
                {
                    list3D.Add(p.transform.position);
                    list2D.Add(p.transform.position);
                }
                if (line == null)
                {
                    line = VectorLine.SetLine3D(color, list3D.ToArray());
                    line.SetWidth(7.0f);
                    line.lineType = LineType.Continuous;
                    line.Draw3DAuto();
                    line.material.mainTexture = lineTexture;
                }
                else
                {
                    line.points3 = list3D;
                }
                collider2D.SetPoints(list2D);
                line.Draw3D();
            }
        }
        private Vector3 GetMousePosition()
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return new Vector3(mousePos.x, mousePos.y, -1);
        }
    }
}