using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;

namespace RPG
{
    public class PingManager : MonoBehaviour
    {
        [SerializeField] private Camera2D camera2D;
        [SerializeField] private GameObject pingPrefab;
        [SerializeField] private PointerHandler pointerPrefab;
        [SerializeField] private Transform pointerParent;
        [SerializeField] private float tickSpeed;
        [SerializeField] private float pointerMoveThreshold;

        private Vector2 pointerStartPos;
        private bool pointing;
        private bool holdingPing;

        private float currentTime;
        private float TickSpeed { get { return 1 / tickSpeed; } }

        private bool toolEnabled;
        private Vector2 lastPointerPos;
        private PointerHandler myPointer;
        private PingType pingType;
        private Dictionary<string, PointerHandler> pointers = new Dictionary<string, PointerHandler>();

        private void OnEnable()
        {
            // Add event listeners
            Events.OnPointerStarted.AddListener(StartPointer);
            Events.OnPointerUpdated.AddListener(UpdatePointer);
            Events.OnPointerStopped.AddListener(StopPointer);
            Events.OnPing.AddListener(Ping);
            Events.OnToolChanged.AddListener(TogglePointer);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnPointerStarted.RemoveListener(StartPointer);
            Events.OnPointerUpdated.RemoveListener(UpdatePointer);
            Events.OnPointerStopped.RemoveListener(StopPointer);
            Events.OnPing.RemoveListener(Ping);
            Events.OnToolChanged.RemoveListener(TogglePointer);
        }
        private void Update()
        {
            currentTime += Time.deltaTime;

            if (toolEnabled && Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.LeftShift))
            {
                if (pingType == PingType.Pointer) StartPointer();
                else StartCoroutine(StartPing());
            }

            if (pointing)
            {
                if (Input.GetMouseButtonUp(0)) StopPointer();
                if (currentTime >= TickSpeed) UpdatePointer();
            }

            if (holdingPing)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    holdingPing = false;
                    StopAllCoroutines();
                    Ping(false);
                }
            }
        }

        private void StartPointer(string id, Vector2 position)
        {
            if (id == GameData.User.id) return;

            PointerHandler pointer = Instantiate(pointerPrefab, pointerParent);
            pointer.transform.position = position;
            pointers.Add(id, pointer);
        }
        private void UpdatePointer(string id, Vector2 position)
        {
            if (pointers.ContainsKey(id)) pointers[id].UpdatePointer(position);
        }
        private void StopPointer(string id)
        {
            if (pointers.ContainsKey(id))
            {
                PointerHandler pointer = pointers[id];
                pointers.Remove(id);
                Destroy(pointer.gameObject);
            }
        }
        private async void Ping(Vector2 position, bool strong)
        {
            if (strong) camera2D.MoveToPosition(position);

            var ping = Instantiate(pingPrefab, position, Quaternion.identity, pointerParent);

            float cellSize = Session.Instance.Grid.CellSize;
            ping.transform.localScale = new Vector3(cellSize * 0.03f, cellSize * 0.03f, 1.0f);

            await UniTask.Delay(6000);
            if (ping != null) Destroy(ping);
        }

        private void StartPointer()
        {
            pointerStartPos = GetMousePos();
            pointing = true;
            camera2D.UsePan = false;

            myPointer = Instantiate(pointerPrefab, pointerParent);
            myPointer.transform.position = pointerStartPos;

            SocketManager.Emit("start-pointer", JsonUtility.ToJson(pointerStartPos));
        }
        private void UpdatePointer()
        {
            currentTime = 0.0f;
            Vector2 newPos = GetMousePos();
            myPointer.UpdatePointer(newPos);
            if (Vector2.Distance(lastPointerPos, newPos) >= pointerMoveThreshold)
            {
                lastPointerPos = newPos;
                SocketManager.Emit("update-pointer", JsonUtility.ToJson(newPos));
            }
        }
        private void StopPointer()
        {
            pointing = false;
            camera2D.UsePan = true;
            Destroy(myPointer.gameObject);
            SocketManager.Emit("stop-pointer", GameData.User.id);
        }
        private IEnumerator StartPing()
        {
            holdingPing = true;
            Vector2 startPos = Input.mousePosition;

            yield return new WaitForSeconds(1.0f);
            holdingPing = false;
            Debug.Log(Vector2.Distance(startPos, GetMousePos()));
            if (Input.GetMouseButton(0) && Vector2.Distance(startPos, Input.mousePosition) < 5.0f) Ping(true);
        }
        private void Ping(bool strong)
        {
            Vector2 mousePos = GetMousePos();
            SocketManager.Emit("ping", JsonUtility.ToJson(mousePos), strong);
        }
        private void TogglePointer(Tool tool)
        {
            toolEnabled = tool.ToString().ToLower().Contains("ping");
            if (tool == Tool.Ping_Ping) pingType = PingType.Ping;
            else if (tool == Tool.Ping_Pointer) pingType = PingType.Pointer;
        }

        private Vector2 GetMousePos()
        {
            return Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    public enum PingType
    {
        Ping,
        Pointer
    }
}