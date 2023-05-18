using UnityEngine;

namespace RPG
{
    // This script will show the area of the light
    public class LightView : MonoBehaviour
    {
        [SerializeField] private LayerMask blockingMask;

        private Light source;
        private float radius;

        private void OnEnable()
        {
            if (source == null) source = GetComponent<Light>();
        }
    }
}