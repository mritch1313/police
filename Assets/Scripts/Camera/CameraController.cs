using UnityEngine;

namespace PoliceChase.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("Target")]
        public Transform Target;
        public Vector3 Offset = new Vector3(0, 3f, -7f);

        [Header("Rotation")]
        public float RotationSpeed = 2f;
        public float MinVerticalAngle = -20f;
        public float MaxVerticalAngle = 60f;
        public bool InvertY = false;

        [Header("Follow")]
        public float FollowSpeed = 5f;
        public float LookAtSpeed = 5f;

        [Header("Collision")]
        public float CollisionRadius = 0.3f;
        public LayerMask CollisionMask;
        public float MinDistance = 1f;

        private float yaw = 0f;
        private float pitch = 15f;
        private Vector3 currentVelocity;
        private float currentDistance;

        private void Start()
        {
            currentDistance = Offset.magnitude;
            if (Target == null)
            {
                var player = FindObjectOfType<Vehicle.PlayerCar>();
                if (player != null) Target = player.transform;
            }

            // Initialize yaw from target rotation
            if (Target != null)
                yaw = Target.eulerAngles.y;

            if (CollisionMask == 0)
                CollisionMask = LayerMask.GetMask("Building", "Obstacle", "Ground");
        }

        private void LateUpdate()
        {
            if (Target == null) return;

            HandleInput();
            UpdateCameraPosition();
        }

        void HandleInput()
        {
            // Mouse input for editor, touch handled via MobileInput
            if (Input.GetMouseButton(1) || Input.touchCount == 1)
            {
                float mouseX = 0f, mouseY = 0f;
                if (Input.touchCount == 1)
                {
                    Touch t = Input.GetTouch(0);
                    // Only if not over UI (simplified)
                    mouseX = t.deltaPosition.x * 0.1f;
                    mouseY = t.deltaPosition.y * 0.1f;
                }
                else
                {
                    mouseX = Input.GetAxis("Mouse X");
                    mouseY = Input.GetAxis("Mouse Y");
                }

                yaw += mouseX * RotationSpeed;
                pitch += (InvertY ? mouseY : -mouseY) * RotationSpeed;
                pitch = Mathf.Clamp(pitch, MinVerticalAngle, MaxVerticalAngle);
            }
        }

        public void AddRotationInput(Vector2 delta)
        {
            yaw += delta.x * RotationSpeed * 0.1f;
            pitch += (InvertY ? delta.y : -delta.y) * RotationSpeed * 0.1f;
            pitch = Mathf.Clamp(pitch, MinVerticalAngle, MaxVerticalAngle);
        }

        void UpdateCameraPosition()
        {
            // Calculate desired position
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 desiredPos = Target.position + rotation * Offset;

            // Collision check
            Vector3 targetPos = Target.position + Vector3.up * 1.5f;
            Vector3 dir = desiredPos - targetPos;
            float dist = dir.magnitude;

            RaycastHit hit;
            if (Physics.SphereCast(targetPos, CollisionRadius, dir.normalized, out hit, dist, CollisionMask))
            {
                float hitDist = Vector3.Distance(targetPos, hit.point);
                currentDistance = Mathf.Lerp(currentDistance, Mathf.Max(hitDist * 0.9f, MinDistance), Time.deltaTime * 10f);
            }
            else
            {
                currentDistance = Mathf.Lerp(currentDistance, Offset.magnitude, Time.deltaTime * 2f);
            }

            Vector3 finalPos = targetPos + dir.normalized * currentDistance;

            // Smooth follow
            transform.position = Vector3.SmoothDamp(transform.position, finalPos, ref currentVelocity, 1f / FollowSpeed);

            // Look at target
            Vector3 lookAt = Target.position + Vector3.up * 1f;
            Quaternion lookRot = Quaternion.LookRotation(lookAt - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * LookAtSpeed);
        }

        public void SetTarget(Transform t)
        {
            Target = t;
        }

        public void ResetCamera()
        {
            if (Target != null)
                yaw = Target.eulerAngles.y;
            pitch = 15f;
        }
    }
}
