using System.Collections.Generic;
using Networking;
using UnityEngine;

namespace RPG
{
    public class WallTools : MonoBehaviour
    {
        [SerializeField] private LineController controllerPrefab;
        [SerializeField] private Transform controllerParent;
        [SerializeField] private CanvasGroup canvasGroup;

        public static WallTools Instance { get; private set; }
        public bool MouseOver;

        private WallManger wallManager;
        private bool initialising;
        private WallType wallType;
        private List<LineController> controllers = new List<LineController>();

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
        private void Awake()
        {
            Instance = this;

            if (wallManager == null) wallManager = GetComponent<WallManger>();
        }
        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0) && !MouseOver && !initialising) InitialiseWall();
            if (Input.GetMouseButtonUp(0) && initialising) FinishCreation();
        }

        private void ToggleUI(Setting setting)
        {
            bool enabled = setting.ToString().ToLower().Contains("walls");
            canvasGroup.alpha = enabled ? 1.0f : 0.0f;
            canvasGroup.blocksRaycasts = enabled;

            switch (setting)
            {
                case Setting.Walls_Regular:
                    wallType = WallType.Wall;
                    break;
                case Setting.Walls_Door:
                    wallType = WallType.Door;
                    break;
                case Setting.Walls_Hidden_Door:
                    wallType = WallType.Hidden_Door;
                    break;
                case Setting.Walls_Invisible:
                    wallType = WallType.Invisible;
                    break;
            }
        }
        public void LoadWalls(List<WallData> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                LineController controller = Instantiate(controllerPrefab, controllerParent);
                controller.Initialise(list[i]);
            }
        }
        public void SplitWall(LineController line, PointController point)
        {
            var index = line.Points.IndexOf(point);
            if (index == line.Points.Count - 1 || index == 0) return;

            LineController controller = Instantiate(controllerPrefab, controllerParent);
            controller.Initialise(line.Type, false);
            var firstRemove = line.Points.Count - 1;

            for (int i = index; i < line.Points.Count; i++)
            {
                controller.AddPoint(line.Points[i].transform.localPosition);
            }
            for (int i = firstRemove; i > index; i--)
            {
                line.DeletePoint(line.Points[i]);
            }

            controllers.Add(controller);

            WallData oldData = line.GetData();
            WallData newData = controller.GetData();

            SocketManager.EmitAsync("modify-wall", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(oldData));

            SocketManager.EmitAsync("create-wall", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(newData));
        }
        private void InitialiseWall()
        {
            initialising = true;
            LineController controller = Instantiate(controllerPrefab, Vector3.zero, Quaternion.identity, controllerParent);
            controller.Initialise(wallType);
            controllers.Add(controller);
        }
        private void FinishCreation()
        {
            initialising = false;
            LineController target = controllers[controllers.Count - 1];
            WallData data = target.GetData();
            SocketManager.EmitAsync("create-wall", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                RemoveWall(target);
            }, JsonUtility.ToJson(data));
        }

        public void RemoveWall(LineController controller)
        {
            controllers.Remove(controller);
            Destroy(controller.gameObject);
        }
    }
}