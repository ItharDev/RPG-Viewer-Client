using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class InitiativeController : MonoBehaviour
    {
        [SerializeField] private InitiativeHolder holderPrefab;
        [SerializeField] private Transform holderTransform;

        private List<InitiativeHolder> holders = new List<InitiativeHolder>();
        private InitiativeHolder dragHolder;

        private void Start()
        {
            SocketManager.Socket.On("modify-initiatives", async (data) =>
            {
                await UniTask.SwitchToMainThread();
                var initiatives = data.GetValue().EnumerateArray().ToArray();
                for (int i = 0; i < holders.Count; i++) Destroy(holders[i].gameObject);
                holders.Clear();

                for (int i = 0; i < initiatives.Length; i++) CreateHolder(JsonUtility.FromJson<InitiativeData>(initiatives[i].ToString()));
                for (int i = 0; i < holders.Count; i++) holders[i].SetIndex(holders[i].Data.index);
            });
        }

        [System.Obsolete]
        public async void LoadHolders(List<InitiativeData> list)
        {
            gameObject.SetActive(true);

            for (int i = 0; i < holders.Count; i++) Destroy(holders[i].gameObject);
            holders.Clear();

            await UniTask.WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            holderTransform.GetComponent<GridLayoutGroup>().enabled = false;
            holderTransform.GetComponent<GridLayoutGroup>().enabled = true;

            for (int i = 0; i < list.Count; i++) CreateHolder(list[i]);
            for (int i = 0; i < holders.Count; i++) holders[i].SetIndex(holders[i].Data.index);

            SessionManager.Session.Loaders++;
        }
        public void UnloadHolders()
        {
            for (int i = 0; i < holders.Count; i++) Destroy(holders[i].gameObject);
            holders.Clear();

            gameObject.SetActive(false);
        }

        [System.Obsolete]
        public void SortHolders()
        {
            holders.Sort(SortByRoll);
            holders.Reverse();

            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].SetIndex(i);
            }
            UpdateHolder();
        }

        public async void ResetInitiatives()
        {
            for (int i = 0; i < holders.Count; i++) Destroy(holders[i].gameObject);
            holders.Clear();

            for (int i = 0; i < SessionManager.Users.Count; i++)
            {
                var user = SessionManager.Users[i];
                await SocketManager.Socket.EmitAsync("get-user", async (callback) =>
                {
                    await UniTask.SwitchToMainThread();
                    if (callback.GetValue().GetBoolean())
                    {
                        var data = new InitiativeData()
                        {
                            index = holders.Count,
                            name = callback.GetValue(1).GetProperty("name").GetString(),
                            roll = "0",
                            visible = true
                        };
                        CreateHolder(data);
                        if (holders.Count == SessionManager.Users.Count)
                        {
                            await SocketManager.Socket.EmitAsync("modify-initiatives", async (callback) =>
                            {
                                await UniTask.SwitchToMainThread();
                                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
                            }, GetHolderData());
                        }
                    }
                    else MessageManager.QueueMessage(callback.GetValue(1).GetString());
                }, user);
            }
        }

        [System.Obsolete]
        public async void AddHolder(bool visible)
        {
            await UniTask.WaitForEndOfFrame();
            InitiativeData data = new InitiativeData()
            {
                index = holders.Count,
                name = "",
                roll = "0",
                visible = visible
            };

            CreateHolder(data);
            await SocketManager.Socket.EmitAsync("modify-initiatives", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, GetHolderData());
        }

        [System.Obsolete]
        public async void MoveHolder()
        {
            await UniTask.WaitForEndOfFrame();
            for (int i = 0; i < holders.Count; i++) holders[i].UpdateIndex();

            await SocketManager.Socket.EmitAsync("modify-initiatives", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, GetHolderData());
        }
        [System.Obsolete]
        public async void RemoveHolder(InitiativeHolder holder)
        {
            holders.Remove(holder);
            Destroy(holder.gameObject);

            await UniTask.WaitForEndOfFrame();
            Canvas.ForceUpdateCanvases();
            holderTransform.GetComponent<GridLayoutGroup>().enabled = false;
            holderTransform.GetComponent<GridLayoutGroup>().enabled = true;

            for (int i = 0; i < holders.Count; i++) holders[i].UpdateIndex();

            await SocketManager.Socket.EmitAsync("modify-initiatives", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, GetHolderData());
        }
        [System.Obsolete]
        public async void UpdateHolder()
        {
            await UniTask.WaitForEndOfFrame();
            await SocketManager.Socket.EmitAsync("modify-initiatives", async (callback) =>
            {
                await UniTask.SwitchToMainThread();
                if (!callback.GetValue().GetBoolean()) MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, GetHolderData());
        }

        public void ToggleHolders()
        {
            holderTransform.gameObject.SetActive(!holderTransform.gameObject.activeInHierarchy);
        }

        private void CreateHolder(InitiativeData data)
        {
            var holder = Instantiate(holderPrefab, holderTransform);
            holders.Add(holder);
            holder.LoadData(this, data);
            if (!SessionManager.IsMaster) holder.gameObject.SetActive(data.visible);

            Canvas.ForceUpdateCanvases();
            holderTransform.GetComponent<GridLayoutGroup>().enabled = false;
            holderTransform.GetComponent<GridLayoutGroup>().enabled = true;
            holder.SetIndex(data.index);
        }
        private List<string> GetHolderData()
        {
            var list = new List<string>();
            for (int i = 0; i < holders.Count; i++)
            {
                list.Add(JsonUtility.ToJson(holders[i].Data));
                if (list.Count == holders.Count) return list;
            }
            return null;
        }

        private int SortByRoll(InitiativeHolder holderA, InitiativeHolder holderB)
        {
            return int.Parse(holderA.Data.roll).CompareTo(int.Parse(holderB.Data.roll));
        }
    }
}