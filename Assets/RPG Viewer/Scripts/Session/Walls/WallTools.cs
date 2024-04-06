using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
        private bool interactable;
        private WallType wallType;
        private List<LineController> controllers = new List<LineController>();

        private void OnEnable()
        {
            // Add event listeners
            Events.OnSettingChanged.AddListener(ToggleUI);
            Events.OnStateChanged.AddListener(ReloadWalls);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnSettingChanged.RemoveListener(ToggleUI);
            Events.OnStateChanged.RemoveListener(ReloadWalls);
        }
        private void Awake()
        {
            Instance = this;

            if (wallManager == null) wallManager = GetComponent<WallManger>();
        }
        private void Update()
        {
            if (!interactable) return;
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0) && !MouseOver && !initialising) InitialiseWall();
            if (Input.GetMouseButtonUp(0) && initialising) FinishCreation();
        }

        private void ReloadWalls(SessionState oldState, SessionState newState)
        {
            // Check if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadWalls();
            }
            else
            {
                // Unload tokens if syncing was disabled
                if (!newState.synced)
                {
                    UnloadWalls();
                    return;
                }

                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadWalls();
            }
        }
        private void UnloadWalls()
        {
            // Loop through each wall
            foreach (var item in controllers)
            {
                // Continue if token is null
                if (item == null) continue;

                item.DestroyLine();
                Destroy(item.gameObject);
            }

            // Clear lists
            controllers.Clear();
        }
        private void ToggleUI(Setting setting)
        {
            bool enabled = setting.ToString().ToLower().Contains("walls");
            canvasGroup.alpha = enabled ? 1.0f : 0.0f;
            canvasGroup.blocksRaycasts = enabled;
            interactable = enabled;

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
                case Setting.Walls_Fog:
                    wallType = WallType.Fog;
                    break;
                case Setting.Walls_Environment:
                    wallType = WallType.Environment;
                    break;
            }
        }
        public void LoadWalls(List<WallData> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                LineController controller = Instantiate(controllerPrefab, controllerParent);
                controller.Initialise(list[i]);
                controllers.Add(controller);
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
                if (callback.GetValue().GetBoolean())
                {
                    controller.Data.id = callback.GetValue(1).GetString();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(newData));
        }
        public void CreateWall(WallData data)
        {
            data.id = "";
            LineController controller = Instantiate(controllerPrefab, controllerParent);
            controller.Initialise(data);
            SocketManager.EmitAsync("create-wall", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    controller.Data.id = callback.GetValue(1).GetString();
                    controllers.Add(controller);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                RemoveWall(controller);
            }, JsonUtility.ToJson(data));
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
                if (callback.GetValue().GetBoolean())
                {
                    target.Data.id = callback.GetValue(1).GetString();
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
                RemoveWall(target);
            }, JsonUtility.ToJson(data));
        }

        public void RemoveWall(LineController controller)
        {
            SocketManager.EmitAsync("remove-wall", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    controllers.Remove(controller);
                    Destroy(controller.gameObject);
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, controller.GetData().id);
        }
    }
}