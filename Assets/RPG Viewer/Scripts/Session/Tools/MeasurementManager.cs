using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Vectrosity;

namespace RPG
{
    public class MeasurementManager : MonoBehaviour
    {
        [SerializeField] private Camera2D camera2D;

        [Header("Measure Panel")]
        [SerializeField] private GameObject infoPanel;
        [SerializeField] private MeasurementType type;

        [Header("Line")]
        [SerializeField] private Texture2D lineTexture;
        [SerializeField] private Texture2D arrowTexture;
        [SerializeField] private Color lineColor;

        private VectorLine line;
        private Vector2 startCell;
        private List<Vector2> wayPoints = new List<Vector2>();
        private bool useAction = false;

        private SessionGrid grid;
        private StateManager stateManager;

        private void OnEnable()
        {
            VectorLine.SetEndCap("Arrow", EndCap.Back, -1.0f, lineTexture, arrowTexture);
        }
        private void OnDisable()
        {
            VectorLine.RemoveEndCap("Arrow");
        }

        private void Update()
        {
            if (line != null)
            {
                line.SetWidth((40.0f / Camera.main.orthographicSize) * (Screen.currentResolution.width / 1600));
                infoPanel.transform.position = new Vector2(line.points2[line.points2.Count - 1].x, line.points2[line.points2.Count - 1].y + 20);
            }

            if (Input.GetMouseButtonDown(0) && stateManager.ToolState == ToolState.Measure && stateManager.allowMeaure) StartMeasurement(Camera.main.ScreenToWorldPoint(Input.mousePosition), stateManager.MeasureType);

            if (grid == null) grid = GetComponentInChildren<SessionGrid>(true);
            if (stateManager == null) stateManager = GetComponentInChildren<StateManager>(true);
            if (useAction)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    wayPoints.Add(type == MeasurementType.Grid ? FindObjectOfType<SessionGrid>().SnapToGrid(mousePos, new Vector2(5, 5)) : mousePos);

                }
                Measure();
                if (Input.GetMouseButtonUp(0))
                {
                    camera2D.UsePan = true;
                    useAction = false;
                    wayPoints.Clear();
                    infoPanel.SetActive(false);
                    Destroy(line.rectTransform.gameObject);
                    line = null;
                }
            }
        }
        private void LateUpdate()
        {
            if (useAction)
            {
                var list2D = new Vector2[wayPoints.Count + 1];
                for (int i = 0; i < wayPoints.Count; i++)
                {
                    list2D[i] = Camera.main.WorldToScreenPoint(wayPoints[i]);
                }

                var endPoint = grid.DistanceBetweenPoints(wayPoints[wayPoints.Count - 1], Camera.main.ScreenToWorldPoint(Input.mousePosition), type);
                list2D[list2D.Length - 1] = Camera.main.WorldToScreenPoint(endPoint.endPos);

                if (line == null)
                {
                    line = new VectorLine("Measure arrow", list2D.ToList(), 1.0f, LineType.Continuous);
                    line.rectTransform.gameObject.layer = 5;
                    line.endCap = "Arrow";
                    line.SetEndCapColor(lineColor);
                    line.SetColor(lineColor);
                }
                else
                {
                    line.points2 = list2D.ToList();
                }
                line.Draw();
            }
        }

        public MeasurementType StartMeasurement(Vector2 startPosition, MeasurementType type)
        {
            camera2D.UsePan = false;
            wayPoints.Add(type == MeasurementType.Grid ? FindObjectOfType<SessionGrid>().SnapToGrid(startPosition, new Vector2(5, 5)) : startPosition);
            useAction = true;
            this.type = type;

            infoPanel.SetActive(true);
            return type;
        }
        public void ChangeType(MeasurementType newType)
        {
            type = newType;
        }

        private void Measure()
        {
            float totalDst = 0;
            int totalDiagonals = 0;

            for (int i = 0; i < wayPoints.Count - 1; i++)
            {
                var result = grid.DistanceBetweenPoints(wayPoints[i], wayPoints[i + 1], type);
                var rounded = Mathf.RoundToInt(result.distance);
                totalDst += rounded;
                totalDiagonals += result.diagonals;
            }

            var endPoint = grid.DistanceBetweenPoints(wayPoints[wayPoints.Count - 1], Camera.main.ScreenToWorldPoint(Input.mousePosition), type);
            var endDst = Mathf.RoundToInt(endPoint.distance);
            totalDst += endDst;
            totalDiagonals += endPoint.diagonals;

            for (int i = 0; i < totalDiagonals; i++)
            {
                if (i % 2 == 0) totalDst += 5;
                else totalDst += 10;
            }

            infoPanel.GetComponentInChildren<TMP_Text>().text = $"{totalDst} ft";
        }
    }

    public enum MeasurementType
    {
        Precise,
        Grid
    }
}