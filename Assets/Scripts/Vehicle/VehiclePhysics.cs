using UnityEngine;

namespace PoliceChase.Vehicle
{
    /// <summary>
    /// Handles pure physics calculations for vehicle. No input, no visual.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class VehiclePhysics : MonoBehaviour
    {
        public VehicleConfig Config;
        private Rigidbody rb;

        [Header("Runtime")]
        public float CurrentSpeed; // m/s
        public float CurrentSpeedKmh => CurrentSpeed * 3.6f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (Config != null)
                ApplyConfig();
        }

        public void Initialize(VehicleConfig config)
        {
            Config = config;
            rb = GetComponent<Rigidbody>();
            ApplyConfig();
        }

        void ApplyConfig()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            rb.mass = Config.Mass;
            rb.drag = Config.Drag;
            rb.angularDrag = Config.AngularDrag;
            rb.centerOfMass = new Vector3(0, -0.5f, 0.3f);
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

        private void FixedUpdate()
        {
            CurrentSpeed = rb.linearVelocity.magnitude;
            // Limit max speed by drag? We will clamp in controller
        }

        public Rigidbody GetRigidbody() => rb;

        public Vector3 GetVelocity() => rb.linearVelocity;

        public void AddForceAtPosition(Vector3 force, Vector3 pos)
        {
            rb.AddForceAtPosition(force, pos, ForceMode.Force);
        }
    }
}
