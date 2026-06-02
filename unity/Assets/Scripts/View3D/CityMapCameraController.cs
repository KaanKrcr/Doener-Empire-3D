using UnityEngine;

namespace DoenerEmpire.View3D
{
    [RequireComponent(typeof(Camera))]
    public sealed class CityMapCameraController : MonoBehaviour
    {
        [SerializeField] private float mousePanSpeed = 0.025f;
        [SerializeField] private float touchPanSpeed = 0.015f;
        [SerializeField] private float zoomSpeed = 0.45f;
        [SerializeField] private float minZoom = 4.8f;
        [SerializeField] private float maxZoom = 9.0f;
        [SerializeField] private Vector2 xBounds = new(-6f, 6f);
        [SerializeField] private Vector2 zBounds = new(-8f, 4f);

        private Camera targetCamera;
        private Vector3 lastMousePosition;
        private bool mousePanning;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            HandleMousePan();
            HandleTouchPanAndZoom();
            HandleWheelZoom();
            ClampPosition();
        }

        private void HandleMousePan()
        {
            if (Input.GetMouseButtonDown(1))
            {
                mousePanning = true;
                lastMousePosition = Input.mousePosition;
            }

            if (!Input.GetMouseButton(1))
            {
                mousePanning = false;
                return;
            }

            Vector3 delta = Input.mousePosition - lastMousePosition;
            Pan(delta, mousePanSpeed);
            lastMousePosition = Input.mousePosition;
        }

        private void HandleTouchPanAndZoom()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    Pan(touch.deltaPosition, touchPanSpeed);
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch first = Input.GetTouch(0);
                Touch second = Input.GetTouch(1);
                Vector2 firstPrevious = first.position - first.deltaPosition;
                Vector2 secondPrevious = second.position - second.deltaPosition;
                float previousDistance = Vector2.Distance(firstPrevious, secondPrevious);
                float currentDistance = Vector2.Distance(first.position, second.position);
                Zoom((previousDistance - currentDistance) * 0.01f);
            }
        }

        private void HandleWheelZoom()
        {
            if (Input.mouseScrollDelta.y != 0f)
            {
                Zoom(-Input.mouseScrollDelta.y * zoomSpeed);
            }
        }

        private void Pan(Vector2 screenDelta, float speed)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            transform.position += (-transform.right * screenDelta.x - flatForward * screenDelta.y) * speed;
        }

        private void Zoom(float delta)
        {
            targetCamera.orthographicSize = Mathf.Clamp(targetCamera.orthographicSize + delta, minZoom, maxZoom);
        }

        private void ClampPosition()
        {
            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, xBounds.x, xBounds.y);
            position.z = Mathf.Clamp(position.z, zBounds.x, zBounds.y);
            transform.position = position;
        }
    }
}
