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
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private MeasurementType type;

        [Header("Line")]
        [SerializeField] private Texture2D lineTexture;
        [SerializeField] private Texture2D arrowTexture;
        [SerializeField] private Color lineColor;

        public static MeasurementManager Instance { get; private set; }
        public bool MouseOver;

        private VectorLine line;
        private Vector2 startCell;
        private List<Vector2> waypoints = new List<Vector2>();

        private bool canMeasure;
        private bool measuring;
        private MeasurementType currentType;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else Destroy(gameObject);
        }
        private void OnEnable()
        {
            VectorLine.SetEndCap("Arrow", EndCap.Back, -1.0f, lineTexture, arrowTexture);

            // Add event listeners
            Events.OnToolChanged.AddListener(ActivateTool);
        }
        private void OnDisable()
        {
            VectorLine.RemoveEndCap("Arrow");

            // Remove event listeners
            Events.OnToolChanged.RemoveListener(ActivateTool);
        }

        private void ActivateTool(Tool tool)
        {
            canMeasure = tool.ToString().ToLower().Contains("measure");
            if (tool == Tool.Measure_Precise) currentType = MeasurementType.Precise;
            else if (tool == Tool.Measure_Grid) currentType = MeasurementType.Grid;
        }

        private void Update()
        {
            if (line != null)
            {
                line.SetWidth((40.0f / Camera.main.orthographicSize) * (Screen.currentResolution.width / 1600));
                infoPanel.transform.position = new Vector2(line.points2[line.points2.Count - 1].x, line.points2[line.points2.Count - 1].y + 20);
            }

            if (Input.GetMouseButtonDown(0) && canMeasure && !MouseOver) StartMeasurement(Camera.main.ScreenToWorldPoint(Input.mousePosition), currentType);

            if (measuring)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    waypoints.Add(type == MeasurementType.Grid ? Session.Instance.Grid.SnapToGrid(mousePos, new Vector2(5, 5)) : mousePos);

                }
                Measure();
                if (Input.GetMouseButtonUp(0))
                {
                    camera2D.UsePan = true;
                    measuring = false;
                    waypoints.Clear();
                    infoPanel.SetActive(false);
                    Destroy(line.rectTransform.gameObject);
                    line = null;
                }
            }
        }
        private void LateUpdate()
        {
            if (measuring)
            {
                var list2D = new Vector2[waypoints.Count + 1];
                for (int i = 0; i < waypoints.Count; i++)
                {
                    list2D[i] = Camera.main.WorldToScreenPoint(waypoints[i]);
                }

                var endPoint = Session.Instance.Grid.DistanceBetweenPoints(waypoints[waypoints.Count - 1], Camera.main.ScreenToWorldPoint(Input.mousePosition), type);
                list2D[list2D.Length - 1] = Camera.main.WorldToScreenPoint(endPoint.end);

                if (line == null)
                {
                    line = new VectorLine("Measure arrow", list2D.ToList(), 1.0f, LineType.Continuous, Joins.Weld);
                    line.rectTransform.gameObject.layer = 5;
                    line.endCap = "Arrow";
                    line.SetEndCapColor(lineColor);
                }
                else
                {
                    line.points2 = list2D.ToList();
                    line.color = lineColor;
                }
                line.Draw();
            }
        }

        public MeasurementType StartMeasurement(Vector2 startPosition, MeasurementType type)
        {
            camera2D.UsePan = false;
            waypoints.Add(type == MeasurementType.Grid ? Session.Instance.Grid.SnapToGrid(startPosition, new Vector2(5, 5)) : startPosition);
            measuring = true;
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

            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                var result = Session.Instance.Grid.DistanceBetweenPoints(waypoints[i], waypoints[i + 1], type);
                var rounded = Mathf.RoundToInt(result.distance);
                totalDst += rounded;
                totalDiagonals += result.diagonals;
            }

            var endPoint = Session.Instance.Grid.DistanceBetweenPoints(waypoints[waypoints.Count - 1], Camera.main.ScreenToWorldPoint(Input.mousePosition), type);
            var endDst = Mathf.RoundToInt(endPoint.distance);
            totalDst += endDst;
            totalDiagonals += endPoint.diagonals;

            for (int i = 0; i < totalDiagonals; i++)
            {
                if (i % 2 == 0) totalDst += 5;
                else totalDst += 10;
            }

            infoText.text = $"{totalDst} ft";
        }
    }

    [System.Serializable]
    public struct MeasurementResult
    {
        public Vector2 start;
        public Vector2 end;
        public float distance;
        public int diagonals;

        public MeasurementResult(Vector2 _start, Vector2 _end, float _distance, int _diagonals)
        {
            start = _start;
            end = _end;
            distance = _distance;
            diagonals = _diagonals;
        }
    }

    public enum MeasurementType
    {
        Precise,
        Grid
    }
}