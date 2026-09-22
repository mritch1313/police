using UnityEngine;
using UnityEngine.UI;
using PoliceChase.Core;
using PoliceChase.Vehicle;

namespace PoliceChase.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("HUD")]
        public Text SpeedText;
        public Text ChaseStatusText;
        public Slider NitroSlider;
        public Button StartButton;
        public Dropdown DifficultyDropdown;
        public Text ArrestProgressText;

        [Header("References")]
        public PlayerCar PlayerCar;
        public AI.ArrestSystem ArrestSystem;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (PlayerCar == null)
                PlayerCar = FindObjectOfType<PlayerCar>();
            if (ArrestSystem == null)
                ArrestSystem = FindObjectOfType<AI.ArrestSystem>();

            if (StartButton != null)
                StartButton.onClick.AddListener(OnStartClicked);

            if (DifficultyDropdown != null)
            {
                DifficultyDropdown.ClearOptions();
                DifficultyDropdown.AddOptions(new System.Collections.Generic.List<string> { "Simple", "Normal", "Advanced" });
                DifficultyDropdown.value = 1;
                DifficultyDropdown.onValueChanged.AddListener(OnDifficultyChanged);
            }

            if (GameState.Instance != null)
            {
                GameState.Instance.OnChaseStarted += OnChaseStarted;
                GameState.Instance.OnChaseEnded += OnChaseEnded;
            }

            UpdateChaseStatus();
        }

        private void Update()
        {
            UpdateSpeed();
            UpdateNitro();
            UpdateArrest();
        }

        void UpdateSpeed()
        {
            if (SpeedText != null && PlayerCar != null)
            {
                float kmh = PlayerCar.GetSpeedKmh();
                SpeedText.text = $"{kmh:F0} km/h";
            }
        }

        void UpdateNitro()
        {
            if (NitroSlider != null && PlayerCar != null)
            {
                var nitro = PlayerCar.GetComponent<NitroSystem>();
                if (nitro != null)
                    NitroSlider.value = nitro.GetChargeNormalized();
            }
        }

        void UpdateArrest()
        {
            if (ArrestProgressText != null && ArrestSystem != null)
            {
                float prog = ArrestSystem.GetImmobilizeProgress();
                if (prog > 0.01f)
                    ArrestProgressText.text = $"Arrest: {prog*100:F0}%";
                else
                    ArrestProgressText.text = "";
            }
        }

        void UpdateChaseStatus()
        {
            if (ChaseStatusText != null)
            {
                if (GameState.Instance != null && GameState.Instance.IsChaseActive)
                    ChaseStatusText.text = $"CHASE: {GameState.Instance.ChaseTime:F1}s";
                else
                    ChaseStatusText.text = "Free Roam";
            }
        }

        void OnStartClicked()
        {
            AI.ChaseManager.Instance?.StartChase();
        }

        void OnDifficultyChanged(int index)
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.Difficulty = (AIDifficulty)index;
            }
        }

        void OnChaseStarted()
        {
            UpdateChaseStatus();
            if (StartButton != null) StartButton.interactable = false;
        }

        void OnChaseEnded()
        {
            UpdateChaseStatus();
            if (StartButton != null) StartButton.interactable = true;
        }

        private void OnDestroy()
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.OnChaseStarted -= OnChaseStarted;
                GameState.Instance.OnChaseEnded -= OnChaseEnded;
            }
        }
    }
}
