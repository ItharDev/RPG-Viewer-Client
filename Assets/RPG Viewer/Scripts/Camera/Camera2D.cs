using System.Collections;
using System.IO;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RPG
{
    public class Camera2D : MonoBehaviour
    {
        public float Zoom { get { return mainVCam.m_Lens.OrthographicSize; } }
        public bool UsePan = false;

        [SerializeField] private CinemachineVirtualCamera mainVCam;

        [Space]
        [SerializeField] private float camZoomMax = 2.0f;
        [SerializeField] private float camZoomMin = 50.0f;
        [SerializeField] private float camZoomSpeed = 1.0f;

        [Space]
        [SerializeField] private BoxCollider2D cullingCollider;

        private bool panActive;
        private Vector2 panPosition;
        private float targetOrthographicSize = 5;

        private void Update()
        {
            HandleCameraZoom();

            if (UsePan)
            {
                if (!Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
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
            targetOrthographicSize -= Input.mouseScrollDelta.y * camZoomSpeed;
            targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, camZoomMax, camZoomMin);
            mainVCam.m_Lens.OrthographicSize = Mathf.Lerp(mainVCam.m_Lens.OrthographicSize, targetOrthographicSize, Time.deltaTime * 10.0f);
            cullingCollider.size = new Vector2(targetOrthographicSize * 2.0f * (16.0f / 9.0f), targetOrthographicSize * 2.0f);
        }

        public void UpdateSettings(float cellSize, Vector2Int dimensions)
        {
            camZoomMin = (dimensions.x >= dimensions.y ? dimensions.x : dimensions.y) * cellSize;
            camZoomMax = cellSize * 2.0f;
        }
        public void MoveToPosition(Vector2 position, bool zoom = true)
        {
            StartCoroutine(MoveCoroutine(position, 0.1f, zoom));
        }
        public void FollowTarget(Transform target, bool instant = false)
        {
            CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
            mainVCam.Follow = target;
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