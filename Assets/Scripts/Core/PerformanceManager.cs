using UnityEngine;

namespace PoliceChase.Core
{
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        [Header("Settings")]
        public float TargetFrameRateLow = 30f;
        public float TargetFrameRateMedium = 45f;
        public float TargetFrameRateHigh = 60f;

        public float UpdateInterval = 1f;
        private float lastUpdate;
        private int frameCount;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            ApplyTargetFrameRate();
        }

        void ApplyTargetFrameRate()
        {
            if (SettingsManager.Instance == null) return;
            float target = TargetFrameRateMedium;
            switch (SettingsManager.Instance.Graphics.QualityLevel)
            {
                case 0: target = TargetFrameRateLow; break;
                case 1: target = TargetFrameRateMedium; break;
                case 2: target = TargetFrameRateHigh; break;
            }
            Application.targetFrameRate = (int)target;
            QualitySettings.vSyncCount = 0;
        }

        private void Update()
        {
            frameCount++;
            if (Time.time - lastUpdate > UpdateInterval)
            {
                float fps = frameCount / (Time.time - lastUpdate);
                // Simple adaptive: if fps < 25 on high, drop quality
                if (fps < 25f && SettingsManager.Instance.Graphics.QualityLevel > 0)
                {
                    // Could auto downgrade, but we keep manual for now
                }
                frameCount = 0;
                lastUpdate = Time.time;
            }
        }

        public void SetQuality(int level)
        {
            SettingsManager.Instance.SetQuality(level);
            ApplyTargetFrameRate();
        }
    }
}
