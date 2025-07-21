using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float panSpeed = 20f;
    public float edgePanThreshold = 20f;
    public float dragPanSpeed = 5f;
    public bool edgePanEnabled = true;

    [Header("Zoom Settings")]
    public float zoomSpeed = 20f;
    public float minZoom = 5f;
    public float maxZoom = 50f;
    public float zoomLerpSpeed = 10f;
    private float targetZoom;

    [Header("Camera Position")]
    public float cameraZPosition = -10f; // Explicit Z position control

    [Header("Cursor Settings")]
    public Texture2D dragCursor;
    public Texture2D defaultCursor;
    public Vector2 dragCursorHotspot = new Vector2(16, 16);

    private Vector2 mapBounds;
    private Camera cam;
    private Vector3 dragStartWorldPos;
    private bool isDragging = false;
    private bool cursorChanged = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component missing!");
            enabled = false;
            return;
        }
        targetZoom = cam.orthographicSize;
        SetDefaultCursor();

        // Ensure proper initial Z position
        transform.position = new Vector3(transform.position.x, transform.position.y, cameraZPosition);
    }

    public void InitializeCameraBounds(float mapWidth, float mapHeight)
    {
        mapBounds = new Vector2(mapWidth / 2f, mapHeight / 2f);
        CenterCameraOnMap();
    }

    void CenterCameraOnMap()
    {
        // Set position with explicit Z coordinate
        transform.position = new Vector3(0, 0, cameraZPosition);
        targetZoom = Mathf.Clamp(Mathf.Min(mapBounds.x, mapBounds.y) / 2f, minZoom, maxZoom);
        cam.orthographicSize = targetZoom;
        ClampCameraPosition();
    }

    void Update()
    {
        HandleKeyboardMovement();
        if (edgePanEnabled) HandleEdgePan();
        HandleDragPan();
        HandleZoom();
        UpdateZoom();

        // Constantly enforce Z position in case it gets modified elsewhere
        if (transform.position.z != cameraZPosition)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, cameraZPosition);
        }
    }


    void SetDragCursor()
    {
        if (dragCursor != null && !cursorChanged)
        {
            Cursor.SetCursor(dragCursor, dragCursorHotspot, CursorMode.Auto);
            cursorChanged = true;
        }
    }

    void SetDefaultCursor()
    {
        if (defaultCursor != null && cursorChanged)
        {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
            cursorChanged = false;
        }
        else if (cursorChanged)
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            cursorChanged = false;
        }
    }

    void HandleKeyboardMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveDirection.y += 1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveDirection.y -= 1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveDirection.x += 1;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveDirection.x -= 1;

        if (moveDirection != Vector3.zero)
        {
            transform.position += moveDirection.normalized * panSpeed * Time.deltaTime;
            ClampCameraPosition();
        }
    }

    void HandleEdgePan()
    {
        if (isDragging || !IsMouseInGameWindow()) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 moveDirection = Vector3.zero;

        // Calculate edge pan intensity (0-1)
        float rightEdge = Mathf.Clamp01((mousePos.x - (Screen.width - edgePanThreshold)) / edgePanThreshold);
        float leftEdge = Mathf.Clamp01((edgePanThreshold - mousePos.x) / edgePanThreshold);
        float topEdge = Mathf.Clamp01((mousePos.y - (Screen.height - edgePanThreshold)) / edgePanThreshold);
        float bottomEdge = Mathf.Clamp01((edgePanThreshold - mousePos.y) / edgePanThreshold);

        moveDirection.x = rightEdge - leftEdge;
        moveDirection.y = topEdge - bottomEdge;

        if (moveDirection.magnitude > 0.1f)
        {
            transform.position += moveDirection.normalized * panSpeed * Time.deltaTime;
            ClampCameraPosition();
        }
    }

    void HandleDragPan()
    {
        // Start drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
            SetDragCursor();
            return;
        }

        // During drag
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 currentWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 difference = dragStartWorldPos - currentWorldPos;

            // Apply movement directly (no deltaTime for immediate response)
            transform.position += difference * dragPanSpeed;
            ClampCameraPosition();

            // Update drag start position for smooth continuous dragging
            dragStartWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        // End drag
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            SetDefaultCursor();
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetZoom = Mathf.Clamp(
                targetZoom - scroll * zoomSpeed,
                minZoom,
                maxZoom
            );
        }
    }

    void UpdateZoom()
    {
        if (Mathf.Abs(cam.orthographicSize - targetZoom) > 0.01f)
        {
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                targetZoom,
                Time.deltaTime * zoomLerpSpeed
            );
            ClampCameraPosition();
        }
    }

      void ClampCameraPosition()
    {
        if (mapBounds == Vector2.zero) return;

        float vertExtent = cam.orthographicSize;
        float horizExtent = vertExtent * Screen.width / Screen.height;

        float minX = -mapBounds.x + horizExtent;
        float maxX = mapBounds.x - horizExtent;
        float minY = -mapBounds.y + vertExtent;
        float maxY = mapBounds.y - vertExtent;

        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        clampedPosition.z = cameraZPosition; // Enforce Z position
        transform.position = clampedPosition;
    }
    bool IsMouseInGameWindow()
    {
        Vector3 mousePos = Input.mousePosition;
        return mousePos.x >= 0 && mousePos.x <= Screen.width &&
               mousePos.y >= 0 && mousePos.y <= Screen.height;
    }
}