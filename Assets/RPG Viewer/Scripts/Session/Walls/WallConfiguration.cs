using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class WallConfiguration : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown typeDropdown;
        [SerializeField] private Toggle lockedToggle;

        private WallData data;
        private RectTransform rect;
        private Action<WallType, bool> callback;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
        }

        public void OpenPanel(WallType type, bool locked, Action<WallType, bool> onCompleted)
        {
            typeDropdown.value = (int)type;
            lockedToggle.isOn = locked;
            callback = onCompleted;
        }
        public void ClosePanel(bool saveData)
        {
            LeanTween.size(rect, new Vector2(250.0f, 0.0f), 0.2f).setOnComplete(() =>
            {
                if (saveData) SaveData();
                Destroy(gameObject);
            });
        }

        private void SaveData()
        {
            callback?.Invoke((WallType)typeDropdown.value, lockedToggle.isOn);
        }
    }
}