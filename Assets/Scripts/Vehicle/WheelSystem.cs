using UnityEngine;

namespace PoliceChase.Vehicle
{
    [System.Serializable]
    public class WheelData
    {
        public Transform wheelTransform;
        public WheelCollider collider;
        public bool isSteerable;
        public bool isMotor;
        public float lastRPM;
        public bool isGrounded;
    }

    public class WheelSystem : MonoBehaviour
    {
        public WheelData[] wheels = new WheelData[4];
        public float wheelRadius = 0.33f;

        private VehicleConfig config;

        public void Initialize(VehicleConfig cfg, Rigidbody rb)
        {
            config = cfg;
            // If wheels not assigned, create them procedurally
            if (wheels[0].collider == null)
            {
                CreateWheelColliders(rb);
            }
        }

        void CreateWheelColliders(Rigidbody rb)
        {
            // Positions relative to car center
            Vector3[] positions = new Vector3[]
            {
                new Vector3(-0.9f, 0.3f, 1.2f),  // FL
                new Vector3(0.9f, 0.3f, 1.2f),   // FR
                new Vector3(-0.9f, 0.3f, -1.2f), // RL
                new Vector3(0.9f, 0.3f, -1.2f)   // RR
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject wcObj = new GameObject($"WheelCollider_{i}");
                wcObj.transform.SetParent(transform);
                wcObj.transform.localPosition = positions[i];
                wcObj.transform.localRotation = Quaternion.identity;

                WheelCollider wc = wcObj.AddComponent<WheelCollider>();
                wc.radius = wheelRadius;
                wc.wheelDampingRate = 25f;
                wc.suspensionDistance = 0.2f;
                wc.forceAppPointDistance = 0.1f;
                wc.mass = config != null ? config.WheelMass : 20f;

                JointSpring spring = wc.suspensionSpring;
                spring.spring = 20000f;
                spring.damper = 3500f;
                spring.targetPosition = 0.5f;
                wc.suspensionSpring = spring;

                WheelFrictionCurve forward = wc.forwardFriction;
                forward.stiffness = config != null ? config.ForwardGrip : 1f;
                wc.forwardFriction = forward;

                WheelFrictionCurve sideways = wc.sidewaysFriction;
                sideways.stiffness = config != null ? config.SidewaysGrip : 1f;
                wc.sidewaysFriction = sideways;

                if (wheels[i] == null) wheels[i] = new WheelData();
                wheels[i].collider = wc;
                wheels[i].isSteerable = i < 2;
                wheels[i].isMotor = i >= 2; // rear wheel drive
            }
        }

        public void UpdateFriction(float forwardGrip, float sidewaysGrip)
        {
            foreach (var w in wheels)
            {
                if (w.collider == null) continue;
                var f = w.collider.forwardFriction;
                f.stiffness = forwardGrip;
                w.collider.forwardFriction = f;

                var s = w.collider.sidewaysFriction;
                s.stiffness = sidewaysGrip;
                w.collider.sidewaysFriction = s;
            }
        }

        public void GetRPMs(out float[] rpms, out bool[] grounded)
        {
            rpms = new float[4];
            grounded = new bool[4];
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].collider != null)
                {
                    rpms[i] = wheels[i].collider.rpm;
                    grounded[i] = wheels[i].collider.isGrounded;
                    wheels[i].lastRPM = rpms[i];
                    wheels[i].isGrounded = grounded[i];
                }
            }
        }
    }
}
