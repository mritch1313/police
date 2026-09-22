using UnityEngine;

namespace PoliceChase.Vehicle
{
    public interface ICarModel
    {
        void Initialize(VehicleConfig config, Transform parent);
        void UpdateWheels(float steerAngle, float[] wheelRPM, bool[] grounded);
        Transform GetBodyTransform();
        void SetColor(Color color);
    }
}
