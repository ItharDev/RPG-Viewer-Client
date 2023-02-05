using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FunkyCode;
using Networking;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class Wall : MonoBehaviour
    {
        [SerializeField] private new EdgeCollider2D collider2D;
        [SerializeField] private LightCollider2D lightCollider;
        [SerializeField] private Image doorImage;

        [SerializeField] private Sprite openDoor;
        [SerializeField] private Sprite closeDoor;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color hiddenColor;

        [SerializeField] private GameObject configPanel;

        public WallData Data;

        private void Update()
        {
            bool showDoor = false;
            if (!SessionManager.IsMaster)
            {
                for (int i = 0; i < SessionManager.session.Tokens.Count; i++)
                {
                    if (SessionManager.session.Tokens[i].Permission.permission == PermissionType.Owner && Vector2.Distance(GetComponentInChildren<Canvas>(true).transform.position, SessionManager.session.Tokens[i].transform.position) <= SessionManager.session.Settings.grid.cellSize) showDoor = true;
                }
            }
            GetComponentInChildren<Canvas>(true).sortingOrder = (showDoor || SessionManager.IsMaster) ? 1 : 0;
        }

        public void GenerateWall(WallData wall)
        {
            doorImage.transform.parent.gameObject.SetActive(false);

            Data = wall;
            collider2D.SetPoints(Data.points);
            collider2D.enabled = !Data.open;
            lightCollider.enabled = collider2D.enabled;
            lightCollider.maskType = LightCollider2D.MaskType.None;
            GetComponentInChildren<Canvas>(true).sortingOrder = SessionManager.IsMaster ? 1 : 0;

            switch (Data.model)
            {
                case WallType.Wall:
                    gameObject.layer = 8;
                    doorImage.transform.parent.gameObject.SetActive(false);
                    doorImage.color = normalColor;
                    break;
                case WallType.Door:
                    gameObject.layer = 8;
                    GetComponentInChildren<Canvas>().transform.position = (wall.points[0] + wall.points[wall.points.Count - 1]) / 2f;
                    GetComponentInChildren<Canvas>().transform.gameObject.SetActive(true);

                    doorImage.transform.parent.gameObject.SetActive(true);
                    doorImage.sprite = Data.open ? openDoor : closeDoor;
                    doorImage.color = normalColor;
                    break;
                case WallType.Invisible:
                    gameObject.layer = 7;
                    doorImage.color = normalColor;
                    lightCollider.enabled = false;
                    break;
                case WallType.Hidden_Door:
                    gameObject.layer = 8;
                    if (!SessionManager.IsMaster) break;

                    GetComponentInChildren<Canvas>().transform.position = (wall.points[0] + wall.points[wall.points.Count - 1]) / 2f;
                    GetComponentInChildren<Canvas>().transform.gameObject.SetActive(true);

                    doorImage.transform.parent.gameObject.SetActive(true);
                    doorImage.sprite = Data.open ? openDoor : closeDoor;
                    doorImage.color = hiddenColor;
                    break;
            }

            lightCollider.Initialize();
        }
        public void SetState(bool state)
        {
            Data.open = state;
            collider2D.enabled = !Data.open;
            lightCollider.enabled = collider2D.enabled;
            doorImage.sprite = Data.open ? openDoor : closeDoor;
        }
        public void ModifyDoor(WallData wall)
        {
            doorImage.transform.parent.gameObject.SetActive(false);

            Data = wall;
            collider2D.enabled = !Data.open;
            lightCollider.enabled = collider2D.enabled;
            lightCollider.maskType = LightCollider2D.MaskType.None;
            GetComponentInChildren<Canvas>(true).sortingOrder = SessionManager.IsMaster ? 1 : 0;

            switch (Data.model)
            {
                case WallType.Wall:
                    gameObject.layer = 8;
                    doorImage.transform.parent.gameObject.SetActive(false);
                    doorImage.color = normalColor;
                    break;
                case WallType.Door:
                    gameObject.layer = 8;
                    GetComponentInChildren<Canvas>().transform.position = (wall.points[0] + wall.points[wall.points.Count - 1]) / 2f;
                    GetComponentInChildren<Canvas>().transform.gameObject.SetActive(true);

                    doorImage.transform.parent.gameObject.SetActive(true);
                    doorImage.sprite = Data.open ? openDoor : closeDoor;
                    doorImage.color = normalColor;
                    break;
                case WallType.Invisible:
                    gameObject.layer = 7;
                    doorImage.color = normalColor;
                    lightCollider.enabled = false;
                    break;
                case WallType.Hidden_Door:
                    gameObject.layer = 8;
                    if (!SessionManager.IsMaster) break;

                    GetComponentInChildren<Canvas>().transform.position = (wall.points[0] + wall.points[wall.points.Count - 1]) / 2f;
                    GetComponentInChildren<Canvas>().transform.gameObject.SetActive(true);

                    doorImage.transform.parent.gameObject.SetActive(true);
                    doorImage.sprite = Data.open ? openDoor : closeDoor;
                    doorImage.color = hiddenColor;
                    break;
            }

            lightCollider.Initialize();
        }

        public async void ToggleDoor(BaseEventData eventData)
        {
            PointerEventData pointerData = eventData as PointerEventData;
            if (pointerData.button == PointerEventData.InputButton.Left)
            {
                if (Data.locked && !SessionManager.IsMaster)
                {
                    MessageManager.QueueMessage("This door is lockced");
                    return;
                }

                await SocketManager.Socket.EmitAsync("toggle-door", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, Data.wallId, !Data.open);
            }
            else if (pointerData.button == PointerEventData.InputButton.Right)
            {
                if (SessionManager.IsMaster) OpenConfig();
            }
        }
        public async void ChangeData(WallData data)
        {
            data.points = Data.points;
            data.wallId = Data.wallId;
            data.open = Data.open;

            await SocketManager.Socket.EmitAsync("modify-door", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(data));
        }

        private void OpenConfig()
        {
            configPanel.SetActive(true);
            configPanel.transform.SetParent(GameObject.Find("Main Canvas").transform);
            configPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            configPanel.transform.SetAsLastSibling();

            configPanel.GetComponentInChildren<Toggle>(true).isOn = Data.locked;
            configPanel.GetComponentInChildren<TMP_Dropdown>(true).value = Data.model == WallType.Door ? 0 : 1;
        }
        public void SaveConfig()
        {
            WallData data = new WallData()
            {
                locked = configPanel.GetComponentInChildren<Toggle>(true).isOn,
                model = configPanel.GetComponentInChildren<TMP_Dropdown>(true).value == 0 ? WallType.Door : WallType.Hidden_Door
            };

            ChangeData(data);
        }
    }
}