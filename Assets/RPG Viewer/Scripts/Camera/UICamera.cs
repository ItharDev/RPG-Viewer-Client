using UnityEngine;

namespace RPG
{
    public class UICamera : MonoBehaviour
    {
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private Camera cam;
        
        private void FixedUpdate()
        {
            cam.orthographicSize = Camera.main.orthographicSize;
        }
    }
}