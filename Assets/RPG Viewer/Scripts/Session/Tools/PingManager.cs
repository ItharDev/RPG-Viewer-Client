using System.Collections;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class PingManager : MonoBehaviour
    {
        [SerializeField] private GameObject pingPrefab;

        private Vector2 startPos;

        private void Start()
        {
            SocketManager.Socket.On("ping", async (data) =>
             {
                 await UniTask.SwitchToMainThread();
                 Vector2 pos = JsonUtility.FromJson<Vector2>(data.GetValue().GetString());
                 HandlePing(pos, data.GetValue(1).GetBoolean());
             });
        }
        private void Update()
        {
            if (Input.GetMouseButtonDown(0) && GetComponent<StateManager>().ToolState == ToolState.Ping)
            {
                startPos = Input.mousePosition;
                StartCoroutine(StartPing());
            }

            if (Input.GetMouseButtonUp(0))
            {
                StopAllCoroutines();
                if (Vector2.Distance(startPos, Input.mousePosition) < 5.0f) Ping(false);
            }
        }

        public async void HandlePing(Vector2 location, bool strong)
        {
            if (strong) FindObjectOfType<Camera2D>().MoveToPosition(location);
            var ping = Instantiate(pingPrefab, location, Quaternion.identity);
            ping.GetComponent<Canvas>().sortingLayerName = "Doors";
            await UniTask.Delay(10000);
            if (ping != null) Destroy(ping);
        }

        private void Ping(bool strong)
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            var pingPos = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            startPos = Vector2.zero;

            SocketManager.Socket.Emit("ping", JsonUtility.ToJson(pingPos), strong);
        }
        private IEnumerator StartPing()
        {
            yield return new WaitForSeconds(1.0f);
            if (Input.GetMouseButton(0) && Vector2.Distance(startPos, Input.mousePosition) < 5.0f) Ping(true);
        }
    }
}