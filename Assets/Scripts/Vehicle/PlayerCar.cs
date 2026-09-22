using UnityEngine;
using PoliceChase.Core;

namespace PoliceChase.Vehicle
{
    public class PlayerCar : MonoBehaviour
    {
        public VehicleController Controller;
        public NitroSystem Nitro;

        private void Awake()
        {
            if (Controller == null)
                Controller = GetComponent<VehicleController>();
            if (Nitro == null)
                Nitro = GetComponent<NitroSystem>();

            if (Controller != null && SettingsManager.Instance != null)
            {
                Controller.Config.MaxSpeed = SettingsManager.Instance.Vehicle.PlayerMaxSpeed;
                Controller.CurrentMaxSpeed = SettingsManager.Instance.Vehicle.PlayerMaxSpeed;
            }
        }

        public void SetInput(float motor, float steer, float brake, bool handbrake)
        {
            Controller?.SetInputs(motor, steer, brake, handbrake);
        }

        public void TriggerNitro()
        {
            Nitro?.ActivateNitro();
        }

        public float GetSpeedKmh() => Controller != null ? Controller.GetSpeedKmh() : 0f;
        public Vector3 GetVelocity() => Controller != null ? Controller.GetVelocity() : Vector3.zero;
        public Vector3 GetPosition() => transform.position;
    }
}
