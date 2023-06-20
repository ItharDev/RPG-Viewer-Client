using System.Collections.Generic;
using UnityEngine;

namespace RPG
{
    public class WallTools : MonoBehaviour
    {
        [SerializeField] private LineController controllerPrefab;
        [SerializeField] private Transform controllerParent;

        public static WallTools Instance { get; private set; }
        public bool MouseOver;

        private WallManger wallManager;
        private bool initialising;
        private WallType wallType;
        private List<LineController> controllers = new List<LineController>();

        private void Awake()
        {
            Instance = this;

            if (wallManager == null) wallManager = GetComponent<WallManger>();
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(1) && !MouseOver && !initialising) InitialiseWall();
            if (Input.GetMouseButtonUp(1) && initialising) StopCreation();
        }

        private void InitialiseWall()
        {
            initialising = true;
            LineController controller = Instantiate(controllerPrefab, Vector3.zero, Quaternion.identity, controllerParent);
            controller.Initialise(wallType);
            controllers.Add(controller);
        }
        private void StopCreation()
        {
            initialising = false;
        }

        public void RemoveWall(LineController controller)
        {
            controllers.Remove(controller);
            Destroy(controller.gameObject);
        }
    }
}