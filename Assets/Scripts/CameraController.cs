using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Transform cameraTransform;

    [SerializeField]
    LayerMask terrainLayer;

    [Header("Movement (Rig Translation)")]
    [SerializeField]
    float moveSpeed = 20f;

    [SerializeField]
    float shiftMult = 2f;

    [SerializeField]
    float smoothTime = 0.2f;

    [Header("Rotation (Rig Yaw)")]
    [SerializeField]
    float rotateSpeed = 150f;

    [Header("Tilt (Camera Pitch)")]
    [SerializeField]
    Vector2 tiltLimits = new(20f, 85f); // Min/Max angle X

    [SerializeField]
    float tiltSpeed = 150f;

    [Header("Zoom (Camera Local Z)")]
    [SerializeField]
    float zoomStep = 5f;

    [SerializeField]
    Vector2 zoomLimits = new(5f, 50f); // Min/Max distance

    // Smoothing Targets
    Vector3 _targetPos;
    float _targetYaw;
    float _targetPitch;
    float _targetZoom;

    // Current State (Where we actually are - used for smoothing)
    float _currentZoom;
    float _currentYaw;
    float _currentPitch;

    // Velocities for SmoothDamp (Reference variables)
    Vector3 _currentPosVel;
    float _currentYawVel;
    float _currentPitchVel;
    float _currentZoomVel;

    // Constant
    const float RAY_HEIGHT = 500f;

    void Start()
    {
        // 1. Snap targets to current transform to prevent initial jerk
        _targetPos = transform.position;
        _targetYaw = transform.eulerAngles.y;
        _targetPitch = cameraTransform.localEulerAngles.x;

        // 2. Initialize zoom based on distance, not just Z axis
        _targetZoom = Mathf.Clamp(
            Vector3.Distance(transform.position, cameraTransform.position),
            zoomLimits.x,
            zoomLimits.y
        );

        // 3. Initialize "Current" variables so we don't start at 0
        _currentZoom = _targetZoom;
        _currentYaw = _targetYaw;
        _currentPitch = _targetPitch;
    }

    void Update()
    {
        HandleInput();
        ApplyTransform();
    }

    void HandleInput()
    {
        float dt = Time.deltaTime;
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftMult : 1f);

        // --- Movement ---
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (
            forward * Input.GetAxisRaw("Vertical") + right * Input.GetAxisRaw("Horizontal")
        ).normalized;

        if (moveDir.sqrMagnitude > 0)
        {
            _targetPos += dt * speed * moveDir;
            _targetPos.y = GetTerrainHeight(new Vector2(_targetPos.x, _targetPos.z));
        }

        // --- Rotation & Tilt ---
        if (Input.GetMouseButton(1))
        {
            _targetYaw += Input.GetAxis("Mouse X") * rotateSpeed * dt;
            _targetPitch -= Input.GetAxis("Mouse Y") * tiltSpeed * dt; // Subtract for intuitive tilt
        }

        // Keyboard Rotation
        if (Input.GetKey(KeyCode.Q))
        {
            _targetYaw -= rotateSpeed * dt;
        }

        if (Input.GetKey(KeyCode.E))
        {
            _targetYaw += rotateSpeed * dt;
        }

        // Clamp Pitch
        _targetPitch = Mathf.Clamp(_targetPitch, tiltLimits.x, tiltLimits.y);

        // --- Zoom ---
        if (Input.mouseScrollDelta.y != 0)
        {
            _targetZoom -= Input.mouseScrollDelta.y * zoomStep;
            _targetZoom = Mathf.Clamp(_targetZoom, zoomLimits.x, zoomLimits.y);
        }
    }

    void ApplyTransform()
    {
        // SmoothDamp is framerate independent and overshoot-proof
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _targetPos,
            ref _currentPosVel,
            smoothTime
        );

        _currentYaw = Mathf.SmoothDampAngle(
            _currentYaw,
            _targetYaw,
            ref _currentYawVel,
            smoothTime
        );
        _currentPitch = Mathf.SmoothDampAngle(
            _currentPitch,
            _targetPitch,
            ref _currentPitchVel,
            smoothTime
        );
        _currentZoom = Mathf.SmoothDamp(_currentZoom, _targetZoom, ref _currentZoomVel, smoothTime);

        // Apply Rotations
        transform.rotation = Quaternion.Euler(0, _currentYaw, 0);

        Quaternion pitchRotation = Quaternion.Euler(_currentPitch, 0, 0);
        cameraTransform.localRotation = pitchRotation;
        cameraTransform.localPosition = pitchRotation * Vector3.back * _currentZoom;
    }

    // Renamed for clarity. Uses X/Z from input, ignores input Y.
    float GetTerrainHeight(Vector2 xzPos)
    {
        Vector3 origin = new(xzPos.x, RAY_HEIGHT, xzPos.y);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1000f, terrainLayer))
        {
            return hit.point.y;
        }
        return 0f;
    }
}
