using UnityEngine;

namespace PoliceChase.Core
{
    [System.Serializable]
    public class VehicleSettings
    {
        [Header("Player")]
        public float PlayerMass = 1500f;
        public float PlayerMaxSpeed = 30f; // m/s ~108 km/h
        public float PlayerAcceleration = 15f;
        public float PlayerBrakeForce = 8000f;
        public float PlayerSteerAngle = 30f;
        public float PlayerGrip = 1.0f;
        public float PlayerDrag = 0.3f;

        [Header("Police")]
        public float PoliceMaxSpeedMultiplier = 1.01f; // 1% faster
        public float PoliceMass = 1600f;
        public float PoliceAcceleration = 14f;
        public float PoliceBrakeForce = 8500f;

        [Header("Nitro")]
        public float NitroBoostMultiplier = 1.25f; // relative to police max
        public float NitroDuration = 3f;
        public float NitroMaxCharge = 100f;
        public float NitroConsumptionRate = 30f;
        public float NitroRechargeRate = 10f;

        [Header("World")]
        public float WorldChunkSize = 200f;
        public int WorldRenderDistance = 3;
        public float WorldDespawnDistance = 600f;

        [Header("Arrest")]
        public float ArrestImmobilizeTime = 3f;
        public float ArrestMinPolice = 2;
        public float ArrestRadius = 8f;
    }

    [System.Serializable]
    public class GraphicsSettings
    {
        public int QualityLevel = 1; // 0 Low, 1 Medium, 2 High
        public float RenderDistance = 500f;
        public bool Shadows = true;
        public int MaxPoliceActive = 6;
        public float LODDistance = 100f;
    }

    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }

        public VehicleSettings Vehicle = new VehicleSettings();
        public GraphicsSettings Graphics = new GraphicsSettings();

        [Header("Tuning")]
        public float PoliceMaxSpeed => Vehicle.PlayerMaxSpeed * Vehicle.PoliceMaxSpeedMultiplier;
        public float PlayerNitroMaxSpeed => PoliceMaxSpeed * Vehicle.NitroBoostMultiplier;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ApplyGraphics();
        }

        public void ApplyGraphics()
        {
            QualitySettings.SetQualityLevel(Graphics.QualityLevel, true);
            // Adjust further settings if needed
        }

        public void SetQuality(int level)
        {
            Graphics.QualityLevel = Mathf.Clamp(level, 0, 2);
            ApplyGraphics();
        }
    }
}
