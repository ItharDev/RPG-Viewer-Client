using System.Collections.Generic;
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
        [SerializeField] private Color hiddenDoorColor;
        [SerializeField] private Texture lineTexture;
        [SerializeField] private new EdgeCollider2D collider2D;

        private WallType type;
        private Color color;
        private VectorLine line;

        private List<PointController> points = new List<PointController>();
        private PointController draggedPoint;
        private bool dragging;

        private void OnEnable()
        {
            // Add event listeners
            Events.OnPointDragged.AddListener(DragPoint);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnPointDragged.RemoveListener(DragPoint);
        }
        private void Update()
        {
            if (dragging)
            {
                // Stop dragging when mouse button is released
                if (Input.GetMouseButtonUp(1)) dragging = false;

                // Update line texture
                UpdateLine();

                // TODO: update first point's position (OnDrag event wont be fired for that point)
            }
        }

        private void StopDrawing()
        {
            dragging = false;
            points.Remove(draggedPoint);
            Destroy(draggedPoint.gameObject);
            if (points.Count <= 1) DeleteWall();
        }
        private void DeleteWall()
        {
            WallTools.Instance.RemoveWall(this);
        }
        private void UpdateLine()
        {
            if (points.Count >= 2)
            {
                // initialise lists
                List<Vector3> list3D = new List<Vector3>();
                List<Vector2> list2D = new List<Vector2>();

                foreach (var point in points)
                {
                    // Add point's position to lists
                    list3D.Add(point.transform.position);
                    list2D.Add(point.transform.position);
                }

                // Create line if it's not yet initialised
                if (line == null)
                {
                    line = new VectorLine("Wall controller", list3D, 3.0f);
                    line.lineType = LineType.Continuous;
                    line.color = color;
                }
                else
                {
                    // Update points 
                    line.points3 = list3D;
                    collider2D.SetPoints(list2D);
                }

                // Redraw line texture, using the new points
                line.Draw3D();
            }
        }

        public void Initialise(WallType _type)
        {
            type = _type;

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
            }

            // Initialise the first and second points to mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0.0f;
            PointController initialPoint = Instantiate(pointPrefab, mousePos, Quaternion.identity, transform);
            initialPoint.Initialise(color, this);
            points.Add(initialPoint);

            draggedPoint = Instantiate(pointPrefab, mousePos, Quaternion.identity, transform);
            draggedPoint.Initialise(color, this);
            points.Add(draggedPoint);

            dragging = true;
        }

        private void DragPoint(PointController point)
        {
            // Update our currently dragged point
            if (points.Contains(point))
            {
                dragging = true;
                draggedPoint = point;
                return;
            }

            // No point is being dragged
            draggedPoint = null;
            dragging = false;
        }
    }
}