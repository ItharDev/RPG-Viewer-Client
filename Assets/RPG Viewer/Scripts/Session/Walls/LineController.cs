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

        private void Update()
        {
            if (dragging)
            {
                if (Input.GetMouseButtonUp(1)) dragging = false;
                HandleDrag();
            }
        }
        private void LateUpdate()
        {
            // Update line width
            if (line != null) line.SetWidth((15.0f / Camera.main.orthographicSize) * (Screen.currentResolution.width / 1600));
        }

        private void HandleDrag()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) StopDrawing();

            // Return if no point is being dragged
            if (draggedPoint == null) return;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0.0f;
            draggedPoint.transform.position = mousePos;
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

                line.points3 = list3D;
                collider2D.SetPoints(list2D);
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
            initialPoint.OnDragEvent.AddListener(DragPoint);

            draggedPoint = Instantiate(pointPrefab, mousePos, Quaternion.identity, transform);
            draggedPoint.Initialise(color, this);
            points.Add(draggedPoint);
            initialPoint.OnDragEvent.AddListener(DragPoint);

            dragging = true;
            UpdateLine();
        }

        private void DragPoint()
        {
            UpdateLine();
        }
    }
}