using System.Collections;
using System.Transactions;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class Camera2D : MonoBehaviour
    {
        public Color BackgroundColor;
        public bool UsePan = false;

        [SerializeField] private CinemachineVirtualCamera mainVCam;

        [SerializeField] private float camZoomMax = 2.0f;
        [SerializeField] private float camZoomMin = 50.0f;
        [SerializeField] private float camZoomSpeed = 1.0f;

        private bool panActive;
        private Vector2 panPosition;
        private float targetOrthographicSize = 5;

        private void Update()
        {
            HandleCameraZoom();

            if (UsePan)
            {
                if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    panActive = true;
                    panPosition = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    FollowTarget(null);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                panActive = false;
                panPosition = Vector2.zero;
            }

            if (panActive) HandleCameraPan();
        }

        private void HandleCameraPan()
        {
            Vector2 direction = panPosition - (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mainVCam.transform.position += (Vector3)direction;
        }
        private void HandleCameraZoom()
        {
            targetOrthographicSize -= (Input.mouseScrollDelta.y * camZoomSpeed);
            targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, camZoomMax, camZoomMin);
            mainVCam.m_Lens.OrthographicSize = Mathf.Lerp(mainVCam.m_Lens.OrthographicSize, targetOrthographicSize, Time.deltaTime * 10.0f);
        }

        public void UpdateSettings(Texture2D texture)
        {
            camZoomMin = (texture.width >= texture.height ? texture.width : texture.height) * 0.01f;
            camZoomMax = (texture.width >= texture.height ? texture.width : texture.height) * 0.0004f;
            SessionManager.session.Loaders++;
        }
        public void MoveToPosition(Vector2 position, bool zoom = true)
        {
            StartCoroutine(MoveCoroutine(position, 0.1f, zoom));
        }
        public void FollowTarget(Transform target)
        {
            mainVCam.m_Follow = target;
        }

        private IEnumerator MoveCoroutine(Vector2 position, float time, bool zoom)
        {
            float t = 0f;
            var startPos = (Vector3)mainVCam.transform.position;
            var targetZoom = camZoomMin * 0.1f;
            var startZoom = mainVCam.m_Lens.OrthographicSize;

            while (t < 1)
            {
                t += Time.fixedDeltaTime / time;
                if (zoom)
                {
                    mainVCam.m_Lens.OrthographicSize = Mathf.Lerp(startZoom, targetZoom, t);
                    targetOrthographicSize = mainVCam.m_Lens.OrthographicSize;
                }

                mainVCam.transform.position = Vector3.Lerp(startPos, new Vector3(position.x, position.y, -10.0f), t);
                yield return null;
            }
        }
    }
}