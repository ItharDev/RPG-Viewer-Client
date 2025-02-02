using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Networking;
using Nobi.UiRoundedCorners;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RPG
{
    public class InitiativePanel : MonoBehaviour
    {
        [SerializeField] private InitiativeHolder holderPrefab;
        [SerializeField] private RectTransform content;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image background;
        [SerializeField] private GameObject buttons;

        public Transform Content { get { return content.transform; } }

        private Dictionary<string, InitiativeHolder> holders = new Dictionary<string, InitiativeHolder>();
        private RectTransform rect;
        private float lastSize;
        private int lastCount;
        private bool isOpen;

        private void OnEnable()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();

            // Add event listners
            Events.OnStateChanged.AddListener(ReloadInitiatives);
            Events.OnSceneLoaded.AddListener(LoadInitiatives);
            Events.OnInitiativeCreated.AddListener(CreateHolder);
            Events.OnInitiativeModified.AddListener(ModifyInitiative);
            Events.OnInitiativeRemoved.AddListener(RemoveInitiative);
            Events.OnInitiativeSorted.AddListener(SortContent);
            Events.OnInitiativesReset.AddListener(ReloadInitiatives);
        }
        private void OnDisable()
        {
            // Remove event listners
            Events.OnStateChanged.RemoveListener(ReloadInitiatives);
            Events.OnSceneLoaded.RemoveListener(LoadInitiatives);
            Events.OnInitiativeCreated.RemoveListener(CreateHolder);
            Events.OnInitiativeModified.RemoveListener(ModifyInitiative);
            Events.OnInitiativeRemoved.RemoveListener(RemoveInitiative);
            Events.OnInitiativeSorted.RemoveListener(SortContent);
            Events.OnInitiativesReset.RemoveListener(ReloadInitiatives);
        }
        private void Update()
        {
            if (content.sizeDelta.y != lastSize)
            {
                lastSize = content.sizeDelta.y;
                Resize();
            }
            if (lastCount != content.transform.childCount)
            {
                lastCount = content.transform.childCount;
                SortContent();
            }
        }

        public void Toggle(BaseEventData eventData)
        {
            // Return if we are dragging
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.dragging) return;

            isOpen = !isOpen;
            buttons.SetActive(isOpen);
            Resize();
        }
        public void AddInitiative(bool visible)
        {
            InitiativeData data = new InitiativeData("", "", 0, visible);
            SocketManager.EmitAsync("create-initiative", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, JsonUtility.ToJson(data));
        }
        public void ResetInitiatives()
        {
            SocketManager.EmitAsync("reset-initiatives", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) GetUsers();

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        private void GetUsers()
        {
            SocketManager.EmitAsync("get-users", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    var users = callback.GetValue(1).EnumerateArray().ToList();

                    for (int i = 0; i < users.Count; i++)
                    {
                        string id = users[i].GetString();
                        AddInitiative(id);
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        private void AddInitiative(string userId)
        {
            SocketManager.EmitAsync("get-user", async (callback) =>
            {
                await UniTask.SwitchToMainThread();

                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    string name = callback.GetValue(1).GetString();

                    InitiativeData data = new InitiativeData("", name, 0, true);
                    SocketManager.EmitAsync("create-initiative", (callback) =>
                    {
                        // Check if the event was successful
                        if (callback.GetValue().GetBoolean()) return;

                        // Send error message
                        MessageManager.QueueMessage(callback.GetValue(1).GetString());
                    }, JsonUtility.ToJson(data));

                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            }, userId);
        }
        public void Sort()
        {
            SocketManager.EmitAsync("sort-initiative", (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean()) return;

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        private void Resize()
        {
            rect.sizeDelta = new Vector2(200.0f, isOpen ? 35.0f + content.sizeDelta.y : 25.0f);

            // Refresh corners
            var corners = background.GetComponent<ImageWithRoundedCorners>();
            corners.Validate();
            corners.Refresh();
        }
        private void SortContent()
        {
            List<InitiativeHolder> sortedList = new List<InitiativeHolder>();
            foreach (var item in holders)
            {
                sortedList.Add(item.Value);
                sortedList.Sort(SortByRoll);
                sortedList.Reverse();
                item.Value.transform.SetSiblingIndex(sortedList.IndexOf(item.Value));
            }
        }

        private void ReloadInitiatives(SessionState oldState, SessionState newState)
        {
            // Check if we are the master client
            if (ConnectionManager.Info.isMaster)
            {
                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadInitiatives();
            }
            else
            {
                // Unload initiatives if syncing was disabled
                if (!newState.synced)
                {
                    UnloadInitiatives();
                    return;
                }

                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadInitiatives();
            }
        }
        private void ReloadInitiatives()
        {
            // Loop through each token
            foreach (var item in holders)
            {
                // Continue if token is null
                if (item.Value == null) continue;
                Destroy(item.Value.gameObject);
            }

            // Clear lists
            holders.Clear();
        }
        private void UnloadInitiatives()
        {
            // Clear lists
            holders.Clear();
            foreach (Transform child in Content) Destroy(child.gameObject);
            canvasGroup.alpha = 0.0f;
            canvasGroup.blocksRaycasts = false;
        }
        private void LoadInitiatives(SceneData data)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.blocksRaycasts = true;

            SocketManager.EmitAsync("get-initiatives", async (callback) =>
            {
                // Check if the event was successful
                if (callback.GetValue().GetBoolean())
                {
                    await UniTask.SwitchToMainThread();
                    var initiatives = callback.GetValue(1).EnumerateObject().ToArray();
                    for (int i = 0; i < initiatives.Length; i++)
                    {
                        InitiativeData data = JsonUtility.FromJson<InitiativeData>(initiatives[i].Value.ToString());
                        CreateHolder(data);
                    }
                    return;
                }

                // Send error message
                MessageManager.QueueMessage(callback.GetValue(1).GetString());
            });
        }
        private void ModifyInitiative(string id, InitiativeData data)
        {
            holders[id].LoadData(data);
            SortContent();
        }
        private void RemoveInitiative(string id)
        {
            InitiativeHolder holder = holders[id];
            holders.Remove(id);
            Destroy(holder.gameObject);
            SortContent();
        }
        private void CreateHolder(InitiativeData data)
        {
            InitiativeHolder holder = Instantiate(holderPrefab, Content);
            holder.LoadData(data);
            holders.Add(data.id, holder);
            SortContent();
        }

        private int SortByRoll(InitiativeHolder holderA, InitiativeHolder holderB)
        {
            return holderA.Data.roll.CompareTo(holderB.Data.roll);
        }
    }
}