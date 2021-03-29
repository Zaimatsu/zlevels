using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ZLevels.GameOfLife
{
    // potentially Utils
    public class MouseCameraController : MonoBehaviour
    {
        [SerializeField] private PolygonCollider2D polygonCollider2D;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float zoomSpeed = 500.0f;
        [SerializeField] private LayerMask boardMask;
        [SerializeField] private EventSystem eventSystem;

        private Transform cameraFollowTarget;
        private float maxOrthographicSize;
        private float orthographicSize;

        public void Initialize(Vector2 worldSize)
        {
            if (cameraFollowTarget == null)
            {
                cameraFollowTarget = new GameObject("CameraFollowTarget").transform;
            }

            virtualCamera.Follow = cameraFollowTarget;
            
            polygonCollider2D.SetPath(0, new[]
            {
                new Vector2(-worldSize.x / 2, -worldSize.y / 2),
                new Vector2(-worldSize.x / 2, worldSize.y / 2),
                new Vector2(worldSize.x / 2, worldSize.y / 2),
                new Vector2(worldSize.x / 2, -worldSize.y / 2)
            });

            var cinemachineConfiner = virtualCamera.GetComponent<CinemachineConfiner>();
            if (cinemachineConfiner == null)
                cinemachineConfiner = virtualCamera.gameObject.AddComponent<CinemachineConfiner>();
            cinemachineConfiner.m_BoundingShape2D = polygonCollider2D;
            
            cinemachineConfiner.InvalidatePathCache();
            orthographicSize = maxOrthographicSize = Mathf.Min(worldSize.x, worldSize.y) / 2.0f;
            virtualCamera.m_Lens.OrthographicSize = orthographicSize;
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                var m_PointerEventData = new PointerEventData(eventSystem);
                m_PointerEventData.position = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(m_PointerEventData, results);
                if (results.Count == 1)
                {
                    RaycastResult hitInfo = results[0];
                    cameraFollowTarget.position = new Vector3(hitInfo.worldPosition.x, hitInfo.worldPosition.y, 0);
                }
            }

            orthographicSize = Mathf.Clamp(orthographicSize - Input.mouseScrollDelta.y * Time.deltaTime * zoomSpeed, 10,
                maxOrthographicSize);
            virtualCamera.m_Lens.OrthographicSize = orthographicSize;
        }
    }
}