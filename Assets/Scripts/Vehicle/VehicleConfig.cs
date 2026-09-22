using UnityEngine;

namespace PoliceChase.Vehicle
{
    [CreateAssetMenu(fileName = "VehicleConfig", menuName = "PoliceChase/VehicleConfig")]
    public class VehicleConfig : ScriptableObject
    {
        [Header("Physics")]
        public float Mass = 1500f;
        public float MaxSpeed = 30f; // m/s
        public float MotorForce = 2500f;
        public float BrakeForce = 8000f;
        public float SteerAngle = 30f;
        public float Drag = 0.3f;
        public float AngularDrag = 0.5f;

        [Header("Grip")]
        public float ForwardGrip = 1f;
        public float SidewaysGrip = 1f;
        public float HandbrakeGripMultiplier = 0.2f;

        [Header("Visual")]
        public Color BodyColor = Color.red;
        public Material BodyMaterial;
        public GameObject CarModelPrefab;

        [Header("Wheels")]
        public float WheelRadius = 0.33f;
        public float WheelMass = 20f;

        public VehicleConfig Clone()
        {
            var c = CreateInstance<VehicleConfig>();
            c.Mass = Mass;
            c.MaxSpeed = MaxSpeed;
            c.MotorForce = MotorForce;
            c.BrakeForce = BrakeForce;
            c.SteerAngle = SteerAngle;
            c.Drag = Drag;
            c.AngularDrag = AngularDrag;
            c.ForwardGrip = ForwardGrip;
            c.SidewaysGrip = SidewaysGrip;
            c.HandbrakeGripMultiplier = HandbrakeGripMultiplier;
            c.BodyColor = BodyColor;
            c.BodyMaterial = BodyMaterial;
            c.CarModelPrefab = CarModelPrefab;
            c.WheelRadius = WheelRadius;
            c.WheelMass = WheelMass;
            return c;
        }
    }
}
