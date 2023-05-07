using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class SceneFolder : MonoBehaviour
    {
        [SerializeField] private RectTransform content;
        [SerializeField] private TMP_Text header;
        [SerializeField] private Image border;
        [SerializeField] private Image icon;
        [SerializeField] private Sprite openIcon;
        [SerializeField] private Sprite closedIcon;

        public string Path
        {
            get { return string.IsNullOrEmpty(data.path) ? Id : $"{data.path}/{Id}"; }
        }
        public string Id { get { return data.id; } }
        public Transform Content { get { return content.transform; } }

        private Folder data;
        private RectTransform rect;
        private bool isOpen;

        private void OnEnable()
        {
            // Get reference of our rect transform
            if (rect == null) rect = GetComponent<RectTransform>();
        }
        private void Update()
        {
            Resize();
        }

        private void Resize()
        {
            // Resize rect
            rect.sizeDelta = new Vector2(0.0f, isOpen ? 40.0f + content.sizeDelta.y : 40.0f);
        }

        public void OpenFolder()
        {
            // Toggle open state
            isOpen = !isOpen;
            icon.sprite = isOpen ? openIcon : closedIcon;
        }
        public void LoadData(Folder folder)
        {
            // Update fields
            data = folder;
            header.text = folder.name;
            border.color = folder.color;
        }
    }
}