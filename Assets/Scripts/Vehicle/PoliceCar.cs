using UnityEngine;
using PoliceChase.Core;

namespace PoliceChase.Vehicle
{
    public class PoliceCar : MonoBehaviour
    {
        public VehicleController Controller;
        public bool IsActive = false;

        private void Awake()
        {
            if (Controller == null)
                Controller = GetComponent<VehicleController>();

            if (Controller != null && SettingsManager.Instance != null)
            {
                Controller.Config.MaxSpeed = SettingsManager.Instance.PoliceMaxSpeed;
                Controller.CurrentMaxSpeed = SettingsManager.Instance.PoliceMaxSpeed;
                Controller.Config.Mass = SettingsManager.Instance.Vehicle.PoliceMass;
            }
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            if (Controller != null)
                Controller.enabled = active;
        }

        public void SetInput(float motor, float steer, float brake, bool handbrake)
        {
            if (!IsActive) return;
            Controller?.SetInputs(motor, steer, brake, handbrake);
        }

        public Vector3 GetPosition() => transform.position;
        public float GetSpeed() => Controller != null ? Controller.GetSpeed() : 0f;
    }
}
