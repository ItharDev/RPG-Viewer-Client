using UnityEngine;

namespace RPG
{
    public class TokenConfiguration : MonoBehaviour
    {
        [SerializeField] private Vector2 maxSize;

        [Header("Appearance")]
        [SerializeField] private GameObject appearancePanel;
        [SerializeField] private Vector2 appearanceSize;

        [Header("Lighting")]
        [SerializeField] private GameObject lightingPanel;
        [SerializeField] private Vector2 lightingSize;

        [Header("Conditions")]
        [SerializeField] private GameObject conditionsPanel;
        [SerializeField] private Vector2 conditionsSize;

        private ResizePanel resizePanel;
        private RectTransform rect;

        private void Awake()
        {
            // Update resize panel's range
            resizePanel = GetComponentInChildren<ResizePanel>();
            rect = GetComponent<RectTransform>();

            OpenAppearance();
        }

        public void OpenAppearance()
        {
            appearancePanel.SetActive(true);
            lightingPanel.SetActive(false);
            conditionsPanel.SetActive(false);
            rect.sizeDelta = appearanceSize;

            resizePanel.UpdateRange(appearanceSize, maxSize);
        }
        public void OpenLighting()
        {
            lightingPanel.SetActive(true);
            appearancePanel.SetActive(false);
            conditionsPanel.SetActive(false);
            rect.sizeDelta = lightingSize;

            resizePanel.UpdateRange(lightingSize, maxSize);
        }
        public void OpenConditions()
        {
            conditionsPanel.SetActive(true);
            appearancePanel.SetActive(false);
            lightingPanel.SetActive(false);
            rect.sizeDelta = conditionsSize;

            resizePanel.UpdateRange(conditionsSize, maxSize);
        }
    }
}