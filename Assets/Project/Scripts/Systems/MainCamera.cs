using UnityEngine;

public class MainCamera : MonoBehaviour
{
    float zoomSpeed = 10f;
    float moveSpeed = 0.5f;
    float rotateSpeed = 5f;

    private Vector3 lastMousePos;

    public float minX = -2f;
    public float maxX = 10f;
    public float minZ = -10f;
    public float maxZ = -1f;
    public float minY = 5f;
    public float maxY = 6f;

    public float minZoomDistance = 5f;
    public float maxZoomDistance = 15f;
    public Vector3 pivot = Vector3.zero;

    void Update()
    {
        HandleZoom();
        HandleMove();
        HandleRotate();
    }

    // ===================== ZOOM =====================
    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) < 0.01f)
            return;

        Vector3 zoomDirection = transform.forward * scroll * zoomSpeed;
        Vector3 newPosition = transform.position + zoomDirection;

        float distance = Vector3.Distance(newPosition, pivot);

        if (distance >= minZoomDistance && distance <= maxZoomDistance)
        {
            transform.position = newPosition;
        }
    }

    // ===================== MOVE =====================
    void HandleMove()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;

            Vector3 move = new Vector3(-delta.x, -delta.y, 0) * moveSpeed * Time.deltaTime;

            transform.Translate(move, Space.Self);
            lastMousePos = Input.mousePosition;

            ClampPosition();
        }
    }

    // ===================== ROTATE =====================
    void HandleRotate()
    {
        if (Input.GetMouseButtonDown(2)) 
        {
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;

            float rotationY = delta.x * rotateSpeed * Time.deltaTime;
            transform.RotateAround(pivot, Vector3.up, rotationY);

            lastMousePos = Input.mousePosition;
        }
    }

    // ===================== LIMIT =====================
    void ClampPosition()
    {
        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        transform.position = pos;
    }
}