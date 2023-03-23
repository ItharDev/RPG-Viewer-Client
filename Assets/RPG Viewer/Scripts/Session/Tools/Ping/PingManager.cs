using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class PingManager : MonoBehaviour
    {
        [SerializeField] private GameObject pingPrefab;
        [SerializeField] private PointerHandler pointerPrefab;
        [SerializeField] private Transform pointerParent;
        [SerializeField] private float tickSpeed;
        [SerializeField] private float pointerMoveThreshold;

        private Vector2 pingStartPos;
        private bool pointing;

        private float currentTime;
        private float TickSpeed { get { return 1 / tickSpeed; } }

        private Vector2 lastPointerPos;
        private Vector2 currentPointerPos;
        private PointerHandler myPointer;
        private Dictionary<string, PointerHandler> pointers = new Dictionary<string, PointerHandler>();


        private void Start()
        {
            SocketManager.Socket.On("ping", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                Vector2 pos = JsonUtility.FromJson<Vector2>(data.GetValue().GetString());
                HandlePing(pos, data.GetValue(1).GetBoolean());
            });

            SocketManager.Socket.On("start-pointer", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                Vector2 pos = JsonUtility.FromJson<Vector2>(data.GetValue().GetString());
                string id = data.GetValue(1).GetString();

                var pointer = Instantiate(pointerPrefab, pointerParent);
                pointer.transform.position = pos;
                pointers.Add(id, pointer);
            });
            SocketManager.Socket.On("update-pointer", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                Vector2 pos = JsonUtility.FromJson<Vector2>(data.GetValue().GetString());
                string id = data.GetValue(1).GetString();

                if (pointers.ContainsKey(id)) pointers[id].UpdatePointer(pos);
            });
            SocketManager.Socket.On("end-pointer", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                string id = data.GetValue().GetString();

                if (pointers.ContainsKey(id))
                {
                    var pointer = pointers[id];
                    pointers.Remove(id);
                    Destroy(pointer.gameObject);
                }
            });
        }
        private void Update()
        {
            currentTime += Time.deltaTime;

            if (Input.GetMouseButtonDown(1) && GetComponent<StateManager>().ToolState == ToolState.Ping)
            {
                if (GetComponent<StateManager>().PingType == PingType.Ping)
                {
                    pingStartPos = Input.mousePosition;
                    StartCoroutine(StartPing());
                }
                else
                {
                    lastPointerPos = GetMousePos();
                    pointing = true;
                    InitialisePointer(lastPointerPos);
                }
            }

            if (pointing && Input.GetMouseButton(1))
            {
                currentPointerPos = GetMousePos();
                if (currentTime >= TickSpeed)
                {
                    currentTime = 0;
                    if (Vector2.Distance(lastPointerPos, currentPointerPos) >= pointerMoveThreshold)
                    {
                        lastPointerPos = currentPointerPos;
                        if (myPointer != null) MovePointer(currentPointerPos);
                    }
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                if (GetComponent<StateManager>().PingType == PingType.Ping)
                {
                    StopAllCoroutines();
                    if (Vector2.Distance(pingStartPos, Input.mousePosition) < 5.0f) Ping(false);
                }
                else
                {
                    pointing = false;
                    lastPointerPos = Vector2.zero;
                    currentPointerPos = Vector2.zero;
                    StopPointer();
                }
            }
        }

        public async void HandlePing(Vector2 location, bool strong)
        {
            if (strong) FindObjectOfType<Camera2D>().MoveToPosition(location);
            var ping = Instantiate(pingPrefab, location, Quaternion.identity);
            ping.GetComponent<Canvas>().sortingLayerName = "Above Fog";
            await UniTask.Delay(10000);
            if (ping != null) Destroy(ping);
        }

        private void Ping(bool strong)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            var pingPos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pingStartPos = Vector2.zero;

            SocketManager.Socket.Emit("ping", JsonUtility.ToJson(pingPos), strong);
        }
        private IEnumerator StartPing()
        {
            yield return new WaitForSeconds(1.0f);
            if (Input.GetMouseButton(0) && Vector2.Distance(pingStartPos, Input.mousePosition) < 5.0f) Ping(true);
        }

        private void InitialisePointer(Vector2 pos)
        {
            myPointer = Instantiate(pointerPrefab, pointerParent);
            myPointer.transform.position = pos;

            SocketManager.Socket.Emit("start-pointer", JsonUtility.ToJson(pos), SocketManager.UserId);
            Debug.Log("Start pointer");
        }
        private void MovePointer(Vector2 pos)
        {
            myPointer.UpdatePointer(pos);

            SocketManager.Socket.Emit("update-pointer", JsonUtility.ToJson(pos), SocketManager.UserId);
            Debug.Log("Update pointer");
        }
        private void StopPointer()
        {
            if (myPointer == null) return;
            Destroy(myPointer.gameObject);

            SocketManager.Socket.Emit("end-pointer", SocketManager.UserId);
            Debug.Log("Stop pointer");
        }

        private Vector2 GetMousePos()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            return mousePos;
        }
    }

    public enum PingType
    {
        Ping,
        Pointer
    }
}