using UnityEngine;
using PoliceChase.Core;

namespace PoliceChase.Vehicle
{
    public class NitroSystem : MonoBehaviour
    {
        [Header("Nitro")]
        public float MaxCharge = 100f;
        public float CurrentCharge = 100f;
        public float ConsumptionRate = 30f;
        public float RechargeRate = 10f;
        public float Duration = 3f;
        public bool IsActive = false;

        private VehicleController vehicleController;
        private float baseMaxSpeed;
        private float nitroMaxSpeed;
        private float activeTime;

        public System.Action<float> OnChargeChanged;
        public System.Action<bool> OnNitroStateChanged;

        private void Awake()
        {
            vehicleController = GetComponent<VehicleController>();
        }

        private void Start()
        {
            if (SettingsManager.Instance != null)
            {
                MaxCharge = SettingsManager.Instance.Vehicle.NitroMaxCharge;
                ConsumptionRate = SettingsManager.Instance.Vehicle.NitroConsumptionRate;
                RechargeRate = SettingsManager.Instance.Vehicle.NitroRechargeRate;
                Duration = SettingsManager.Instance.Vehicle.NitroDuration;
                baseMaxSpeed = SettingsManager.Instance.Vehicle.PlayerMaxSpeed;
                nitroMaxSpeed = SettingsManager.Instance.PlayerNitroMaxSpeed;
            }
            else
            {
                baseMaxSpeed = vehicleController != null ? vehicleController.Config.MaxSpeed : 30f;
                nitroMaxSpeed = baseMaxSpeed * 1.25f;
            }
            CurrentCharge = MaxCharge;
        }

        private void Update()
        {
            if (IsActive)
            {
                CurrentCharge -= ConsumptionRate * Time.deltaTime;
                activeTime += Time.deltaTime;

                if (CurrentCharge <= 0f || activeTime >= Duration)
                {
                    DeactivateNitro();
                }
                OnChargeChanged?.Invoke(CurrentCharge / MaxCharge);
            }
            else
            {
                // Recharge
                if (CurrentCharge < MaxCharge)
                {
                    CurrentCharge += RechargeRate * Time.deltaTime;
                    CurrentCharge = Mathf.Min(CurrentCharge, MaxCharge);
                    OnChargeChanged?.Invoke(CurrentCharge / MaxCharge);
                }
            }
        }

        public bool CanActivate()
        {
            return CurrentCharge > 10f && !IsActive;
        }

        public void ActivateNitro()
        {
            if (!CanActivate()) return;

            IsActive = true;
            activeTime = 0f;
            if (vehicleController != null)
            {
                // Real acceleration to increased max speed, not teleport
                vehicleController.SetMaxSpeed(nitroMaxSpeed);
                // Add forward boost force
                var rb = vehicleController.GetRigidbody();
                if (rb != null)
                {
                    rb.AddForce(transform.forward * 5000f, ForceMode.Impulse);
                }
            }
            OnNitroStateChanged?.Invoke(true);
            Debug.Log("[Nitro] Activated");
        }

        public void DeactivateNitro()
        {
            if (!IsActive) return;
            IsActive = false;
            activeTime = 0f;
            if (vehicleController != null)
            {
                vehicleController.SetMaxSpeed(baseMaxSpeed);
            }
            OnNitroStateChanged?.Invoke(false);
            Debug.Log("[Nitro] Deactivated");
        }

        public float GetChargeNormalized() => CurrentCharge / MaxCharge;
    }
}
