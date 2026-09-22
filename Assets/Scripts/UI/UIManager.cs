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

            // Auto-create UI if missing (for minimal scene)
            if (SpeedText == null || StartButton == null)
                CreateRuntimeUI();

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

        void CreateRuntimeUI()
        {
            // Create Canvas for portrait
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            canvasGO.GetComponent<CanvasScaler>().matchWidthOrHeight = 1f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Speed Text
            var speedGO = new GameObject("SpeedText");
            speedGO.transform.SetParent(canvasGO.transform);
            var speedText = speedGO.AddComponent<Text>();
            speedText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            speedText.fontSize = 40;
            speedText.color = Color.white;
            var rt = speedGO.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.anchoredPosition = new Vector2(100, 100);
            rt.sizeDelta = new Vector2(300, 60);
            SpeedText = speedText;

            // Chase Status
            var statusGO = new GameObject("ChaseStatus");
            statusGO.transform.SetParent(canvasGO.transform);
            var statusText = statusGO.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 36;
            statusText.color = Color.yellow;
            var rt2 = statusGO.GetComponent<RectTransform>();
            rt2.anchorMin = new Vector2(0.5f, 1);
            rt2.anchorMax = new Vector2(0.5f, 1);
            rt2.anchoredPosition = new Vector2(0, -100);
            rt2.sizeDelta = new Vector2(400, 60);
            ChaseStatusText = statusText;

            // Start Button top right
            var btnGO = new GameObject("StartButton");
            btnGO.transform.SetParent(canvasGO.transform);
            var img = btnGO.AddComponent<Image>();
            img.color = Color.green;
            var btn = btnGO.AddComponent<Button>();
            var btnRT = btnGO.GetComponent<RectTransform>();
            btnRT.anchorMin = new Vector2(1, 1);
            btnRT.anchorMax = new Vector2(1, 1);
            btnRT.anchoredPosition = new Vector2(-150, -100);
            btnRT.sizeDelta = new Vector2(200, 80);

            var txtGO = new GameObject("Text");
            txtGO.transform.SetParent(btnGO.transform);
            var txt = txtGO.AddComponent<Text>();
            txt.text = "НАЧАТЬ";
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 32;
            txt.color = Color.black;
            txt.alignment = TextAnchor.MiddleCenter;
            var txtRT = txtGO.GetComponent<RectTransform>();
            txtRT.anchorMin = Vector2.zero;
            txtRT.anchorMax = Vector2.one;
            txtRT.sizeDelta = Vector2.zero;
            txtRT.anchoredPosition = Vector2.zero;

            StartButton = btn;

            // Nitro Slider
            var sliderGO = new GameObject("NitroSlider");
            sliderGO.transform.SetParent(canvasGO.transform);
            var slider = sliderGO.AddComponent<Slider>();
            var sliderRT = sliderGO.GetComponent<RectTransform>();
            sliderRT.anchorMin = new Vector2(0, 0.5f);
            sliderRT.anchorMax = new Vector2(0, 0.5f);
            sliderRT.anchoredPosition = new Vector2(50, 0);
            sliderRT.sizeDelta = new Vector2(20, 400);
            slider.direction = Slider.Direction.BottomToTop;
            NitroSlider = slider;

            // Arrest text
            var arrestGO = new GameObject("ArrestText");
            arrestGO.transform.SetParent(canvasGO.transform);
            var arrestText = arrestGO.AddComponent<Text>();
            arrestText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            arrestText.fontSize = 40;
            arrestText.color = Color.red;
            var rt3 = arrestGO.GetComponent<RectTransform>();
            rt3.anchorMin = new Vector2(0.5f, 0.5f);
            rt3.anchorMax = new Vector2(0.5f, 0.5f);
            rt3.anchoredPosition = new Vector2(0, 200);
            rt3.sizeDelta = new Vector2(400, 60);
            ArrestProgressText = arrestText;
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
