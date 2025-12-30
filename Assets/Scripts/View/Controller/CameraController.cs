using UnityEngine;

namespace SkiGame.View.Controller
{
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform cameraTransform;

        [SerializeField]
        private LayerMask terrainLayer;

        [Header("Movement (Rig Translation)")]
        [SerializeField]
        private float _moveSpeed = 20f;

        [SerializeField]
        private float _shiftMult = 2f;

        [SerializeField]
        private float _smoothTime = 0.2f;

        [Header("Rotation (Rig Yaw)")]
        [SerializeField]
        private float _rotateSpeed = 150f;

        [Header("Tilt (Camera Pitch)")]
        [SerializeField]
        private Vector2 _tiltLimits = new(20f, 85f); // Min/Max angle X.

        [SerializeField]
        private float _tiltSpeed = 150f;

        [Header("Zoom (Camera Local Z)")]
        [SerializeField]
        private float _zoomStep = 5f;

        [SerializeField]
        private Vector2 _zoomLimits = new(5f, 50f); // Min/Max distance.

        // Smoothing Targets.
        private Vector3 _targetPos;
        private float _targetYaw;
        private float _targetPitch;
        private float _targetZoom;

        // Current State (Where we actually are - used for smoothing).
        private float _currentZoom;
        private float _currentYaw;
        private float _currentPitch;

        // Velocities for SmoothDamp (Reference variables).
        private Vector3 _currentPosVel;
        private float _currentYawVel;
        private float _currentPitchVel;
        private float _currentZoomVel;

        // Constant.
        private const float RAY_HEIGHT = 500f;

        private void Start()
        {
            // 1. Snap targets to current transform to prevent initial jerk.
            _targetPos = transform.position;
            _targetYaw = transform.eulerAngles.y;
            _targetPitch = cameraTransform.localEulerAngles.x;

            // 2. Initialize zoom based on distance, not just Z axis.
            _targetZoom = Mathf.Clamp(
                Vector3.Distance(transform.position, cameraTransform.position),
                _zoomLimits.x,
                _zoomLimits.y
            );

            // 3. Initialize "Current" variables so we don't start at 0.
            _currentZoom = _targetZoom;
            _currentYaw = _targetYaw;
            _currentPitch = _targetPitch;
        }

        private void Update()
        {
            HandleInput();
            ApplyTransform();
        }

        private void HandleInput()
        {
            float dt = Time.deltaTime;
            float speed = _moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? _shiftMult : 1f);

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
                _targetYaw += Input.GetAxis("Mouse X") * _rotateSpeed * dt;
                _targetPitch -= Input.GetAxis("Mouse Y") * _tiltSpeed * dt; // Subtract for intuitive tilt.
            }

            // Keyboard Rotation.
            if (Input.GetKey(KeyCode.Q))
            {
                _targetYaw -= _rotateSpeed * dt;
            }

            if (Input.GetKey(KeyCode.E))
            {
                _targetYaw += _rotateSpeed * dt;
            }

            // Clamp Pitch.
            _targetPitch = Mathf.Clamp(_targetPitch, _tiltLimits.x, _tiltLimits.y);

            // --- Zoom ---
            if (Input.mouseScrollDelta.y != 0)
            {
                _targetZoom -= Input.mouseScrollDelta.y * _zoomStep;
                _targetZoom = Mathf.Clamp(_targetZoom, _zoomLimits.x, _zoomLimits.y);
            }
        }

        private void ApplyTransform()
        {
            // SmoothDamp is framerate independent and overshoot-proof.
            transform.position = Vector3.SmoothDamp(
                transform.position,
                _targetPos,
                ref _currentPosVel,
                _smoothTime
            );

            _currentYaw = Mathf.SmoothDampAngle(
                _currentYaw,
                _targetYaw,
                ref _currentYawVel,
                _smoothTime
            );
            _currentPitch = Mathf.SmoothDampAngle(
                _currentPitch,
                _targetPitch,
                ref _currentPitchVel,
                _smoothTime
            );
            _currentZoom = Mathf.SmoothDamp(
                _currentZoom,
                _targetZoom,
                ref _currentZoomVel,
                _smoothTime
            );

            // Apply Rotations.
            transform.rotation = Quaternion.Euler(0, _currentYaw, 0);

            Quaternion pitchRotation = Quaternion.Euler(_currentPitch, 0, 0);
            cameraTransform.SetLocalPositionAndRotation(
                pitchRotation * Vector3.back * _currentZoom,
                pitchRotation
            );
        }

        // Renamed for clarity. Uses X/Z from input, ignores input Y.
        private float GetTerrainHeight(Vector2 xzPos)
        {
            Vector3 origin = new(xzPos.x, RAY_HEIGHT, xzPos.y);

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1000f, terrainLayer))
            {
                return hit.point.y;
            }
            return 0f;
        }
    }
}
