using UnityEngine;

namespace PoliceChase.Vehicle
{
    /// <summary>
    /// Core vehicle controller. Uses WheelColliders for physical movement, not Transform.position.
    /// Architecture allows model replacement without touching physics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(VehiclePhysics))]
    [RequireComponent(typeof(WheelSystem))]
    public class VehicleController : MonoBehaviour
    {
        [Header("Config")]
        public VehicleConfig Config;

        [Header("Input")]
        [Range(-1f, 1f)] public float MotorInput;
        [Range(-1f, 1f)] public float SteerInput;
        [Range(0f, 1f)] public float BrakeInput;
        public bool Handbrake;

        [Header("State")]
        public float CurrentMaxSpeed;
        public bool IsGrounded;

        private VehiclePhysics physics;
        private WheelSystem wheelSystem;
        private Rigidbody rb;
        private ICarModel carModel;

        private float currentSteerAngle;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            physics = GetComponent<VehiclePhysics>();
            wheelSystem = GetComponent<WheelSystem>();

            if (Config == null)
            {
                Config = ScriptableObject.CreateInstance<VehicleConfig>();
            }

            physics.Initialize(Config);
            wheelSystem.Initialize(Config, rb);
            CurrentMaxSpeed = Config.MaxSpeed;

            // Create or find car model
            carModel = GetComponentInChildren<ICarModel>();
            if (carModel == null)
            {
                var factoryObj = new GameObject("CarModelFactory");
                factoryObj.transform.SetParent(transform);
                factoryObj.transform.localPosition = Vector3.zero;
                factoryObj.transform.localRotation = Quaternion.identity;
                var factory = factoryObj.AddComponent<CarModelFactory>();
                factory.Initialize(Config, factoryObj.transform);
                carModel = factory;
            }
            else
            {
                carModel.Initialize(Config, transform);
            }
        }

        private void FixedUpdate()
        {
            HandleMotor();
            HandleSteering();
            HandleBraking();
            HandleGrip();
            CheckGrounded();
            ClampSpeed();
            UpdateModel();
        }

        void HandleMotor()
        {
            float motor = Config.MotorForce * MotorInput;
            // Apply to motor wheels
            foreach (var w in wheelSystem.wheels)
            {
                if (w.isMotor && w.collider != null)
                {
                    w.collider.motorTorque = motor;
                }
            }
        }

        void HandleSteering()
        {
            currentSteerAngle = Config.SteerAngle * SteerInput;
            foreach (var w in wheelSystem.wheels)
            {
                if (w.isSteerable && w.collider != null)
                {
                    w.collider.steerAngle = currentSteerAngle;
                }
            }
        }

        void HandleBraking()
        {
            float brake = Config.BrakeForce * BrakeInput;
            if (Handbrake)
                brake = Config.BrakeForce * 2f;

            foreach (var w in wheelSystem.wheels)
            {
                if (w.collider == null) continue;
                if (Handbrake && w.isMotor) // handbrake on rear
                {
                    w.collider.brakeTorque = brake;
                }
                else if (!Handbrake)
                {
                    w.collider.brakeTorque = brake;
                }
            }
        }

        void HandleGrip()
        {
            float forwardGrip = Config.ForwardGrip;
            float sidewaysGrip = Config.SidewaysGrip;

            if (Handbrake)
            {
                forwardGrip *= Config.HandbrakeGripMultiplier;
                sidewaysGrip *= Config.HandbrakeGripMultiplier;
            }

            // Reduce grip at high speed for drift feeling
            float speedFactor = Mathf.Clamp01(physics.CurrentSpeed / CurrentMaxSpeed);
            if (speedFactor > 0.8f)
            {
                sidewaysGrip *= Mathf.Lerp(1f, 0.7f, (speedFactor - 0.8f) / 0.2f);
            }

            wheelSystem.UpdateFriction(forwardGrip, sidewaysGrip);
        }

        void CheckGrounded()
        {
            int groundedCount = 0;
            foreach (var w in wheelSystem.wheels)
            {
                if (w.collider != null && w.collider.isGrounded)
                    groundedCount++;
            }
            IsGrounded = groundedCount >= 3;
        }

        void ClampSpeed()
        {
            // If over max speed, apply opposite force
            if (physics.CurrentSpeed > CurrentMaxSpeed)
            {
                float excess = physics.CurrentSpeed - CurrentMaxSpeed;
                Vector3 brakeForce = -rb.linearVelocity.normalized * excess * 500f;
                rb.AddForce(brakeForce, ForceMode.Force);
            }
        }

        void UpdateModel()
        {
            if (carModel == null) return;
            wheelSystem.GetRPMs(out float[] rpms, out bool[] grounded);
            carModel.UpdateWheels(currentSteerAngle, rpms, grounded);
        }

        // Public API
        public void SetInputs(float motor, float steer, float brake, bool handbrake)
        {
            MotorInput = Mathf.Clamp(motor, -1f, 1f);
            SteerInput = Mathf.Clamp(steer, -1f, 1f);
            BrakeInput = Mathf.Clamp01(brake);
            Handbrake = handbrake;
        }

        public void SetMaxSpeed(float maxSpeed)
        {
            CurrentMaxSpeed = maxSpeed;
        }

        public float GetSpeed() => physics.CurrentSpeed;
        public float GetSpeedKmh() => physics.CurrentSpeedKmh;
        public Vector3 GetVelocity() => physics.GetVelocity();
        public Rigidbody GetRigidbody() => rb;
    }
}
