using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Networking;
using Cysharp.Threading.Tasks;
using System.Linq;

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
                for (int i = 0; i < holders.Count; i++) holders[i].LoadData(this, JsonUtility.FromJson<InitiativeData>(initiatives[i].ToString()));

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
            for (int i = 0; i < holders.Count; i++) holders[i].LoadData(this, list[i]);

            SessionManager.session.Loaders++;
        }
        public void UnloadHolders()
        {
            for (int i = 0; i < holders.Count; i++) Destroy(holders[i].gameObject);
            holders.Clear();

            gameObject.SetActive(false);
        }

        [System.Obsolete]
        public async void AddHolder(bool visible)
        {
            await UniTask.WaitForEndOfFrame();
            InitiativeData data = new InitiativeData()
            {
                index = holders.Count,
                name = "",
                roll = "",
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
    }
}