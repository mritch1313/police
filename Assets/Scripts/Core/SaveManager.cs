using UnityEngine;

namespace PoliceChase.Core
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string QUALITY_KEY = "quality_level";
        private const string DIFFICULTY_KEY = "ai_difficulty";

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

        public void SaveSettings(int quality, int difficulty)
        {
            PlayerPrefs.SetInt(QUALITY_KEY, quality);
            PlayerPrefs.SetInt(DIFFICULTY_KEY, difficulty);
            PlayerPrefs.Save();
        }

        public void LoadSettings()
        {
            int q = PlayerPrefs.GetInt(QUALITY_KEY, 1);
            int d = PlayerPrefs.GetInt(DIFFICULTY_KEY, 1);
            if (SettingsManager.Instance != null)
                SettingsManager.Instance.SetQuality(q);
            if (GameState.Instance != null)
                GameState.Instance.Difficulty = (AIDifficulty)d;
        }
    }
}
