using UnityEngine;

namespace RPG
{
    public class PointerHandler : MonoBehaviour
    {
        [SerializeField] private TrailRenderer trailRenderer;

        private void Start()
        {
            trailRenderer.startColor = GetColor();
        }
        public void UpdatePointer(Vector2 newPos)
        {
            transform.position = newPos;
        }

        private Color GetColor()
        {
            // Generate random color with 13 variants
            float randomHue = GetRandomHue();

            return Color.HSVToRGB(randomHue, 1.0f, 1.0f);
        }
        private float GetRandomHue()
        {
            return Random.Range(0.0f, 1.0f);
        }
    }
}