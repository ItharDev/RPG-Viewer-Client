using System.Collections;
using FunkyCode;
using UnityEngine;
using UnityEngine.UI;

namespace RPG
{
    public class Light : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Sprite onIcon;
        [SerializeField] private Sprite offIcon;

        private Light2D source;
        private Canvas canvas;
        private LightData data;
        private bool loaded;

        private void OnEnable()
        {
            // Get reference of our light source and canvas
            if (source == null) source = GetComponent<Light2D>();
            if (canvas == null) canvas = GetComponentInChildren<Canvas>();

            // Add event listeners
            Events.OnToolChanged.AddListener(ToggleUI);
        }
        private void OnDisable()
        {
            // Remove event listeners
            Events.OnToolChanged.RemoveListener(ToggleUI);
        }
        private void Update()
        {
            if (!loaded && Session.Instance.Grid.Grid != null)
            {
                loaded = true;
                UpdateData();
            }
        }

        private void UpdateData()
        {
            float cellSize = Session.Instance.Grid.CellSize;
            transform.localScale = new Vector3(cellSize * 0.03f, cellSize * 0.03f, 1.0f);

            // Update our position and light source
            transform.position = data.position;
            source.enabled = data.enabled;
            source.color = data.color;
            icon.sprite = data.enabled ? onIcon : offIcon;
            ToggleUI(true);
            StartCoroutine(Resize(data.radius * 0.2f * cellSize));
        }

        private void ToggleUI(ToolState state)
        {
            ToggleUI(state == ToolState.Light);
        }

        public void LoadData(LightData _data)
        {
            data = _data;

            loaded = false;
            ToggleUI(false);
        }
        public void ToggleUI(bool enabled)
        {
            // Enable / disable UI
            canvas.enabled = enabled;
        }

        private IEnumerator Resize(float targetSize)
        {
            float originalSize = source.size;
            for (float t = 0f; t < 1.0f; t += Time.deltaTime)
            {
                float normalizedTime = t / 1.0f;
                source.size = Mathf.Lerp(originalSize, targetSize, normalizedTime);
                yield return null;
            }

            source.size = targetSize;
        }
    }
}