using System.Collections.Generic;
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
        [SerializeField] private Image background;
        [SerializeField] private GameObject buttons;

        public Transform Content { get { return content.transform; } }

        private List<InitiativeHolder> holders = new List<InitiativeHolder>();
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
        }
        private void OnDisable()
        {
            // Remove event listners
            Events.OnStateChanged.RemoveListener(ReloadInitiatives);
            Events.OnSceneLoaded.RemoveListener(LoadInitiatives);
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
        private void Resize()
        {
            rect.sizeDelta = new Vector2(200.0f, isOpen ? 30.0f + content.sizeDelta.y : 30.0f);

            // Refresh corners
            var corners = background.GetComponent<ImageWithRoundedCorners>();
            corners.Validate();
            corners.Refresh();
        }
        private void SortContent()
        {
            holders.Sort(SortByRoll);
            holders.Reverse();

            for (int i = 0; i < holders.Count; i++)
            {
                holders[i].transform.SetSiblingIndex(i);
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
                if (oldState.synced && !newState.synced)
                {
                    UnloadInitiatives();
                    return;
                }

                // Return if scene was not changed
                if (oldState.scene == newState.scene) return;
                UnloadInitiatives();
            }
        }
        private void UnloadInitiatives()
        {
            // Loop through each token
            foreach (var item in holders)
            {
                // Continue if token is null
                if (item == null) continue;
                Destroy(item.gameObject);
            }

            // Clear lists
            holders.Clear();
        }
        private void LoadInitiatives(SceneData data)
        {
            List<InitiativeData> initiatives = data.initiatives;
            for (int i = 0; i < initiatives.Count; i++)
            {
                CreateHolder(initiatives[i]);
            }

            holders.Sort(SortByRoll);
        }
        private void CreateHolder(InitiativeData data)
        {
            InitiativeHolder holder = Instantiate(holderPrefab, Content);
            holder.LoadData(data);
            holders.Add(holder);
        }

        private int SortByRoll(InitiativeHolder holderA, InitiativeHolder holderB)
        {
            return holderA.Data.roll.CompareTo(holderB.Data.roll);
        }
    }
}