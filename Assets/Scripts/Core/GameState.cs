using UnityEngine;

namespace PoliceChase.Core
{
    public enum GameMode
    {
        FreeRoam,
        Chase
    }

    public enum AIDifficulty
    {
        Simple,
        Normal,
        Advanced
    }

    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        [Header("Game")]
        public GameMode CurrentMode = GameMode.FreeRoam;
        public AIDifficulty Difficulty = AIDifficulty.Normal;

        [Header("Chase")]
        public bool IsChaseActive = false;
        public float ChaseTime = 0f;
        public int PoliceCount = 3;
        public bool PlayerArrested = false;

        public System.Action OnChaseStarted;
        public System.Action OnChaseEnded;
        public System.Action OnPlayerArrested;

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

        public void StartChase()
        {
            if (IsChaseActive) return;
            IsChaseActive = true;
            CurrentMode = GameMode.Chase;
            ChaseTime = 0f;
            PlayerArrested = false;
            OnChaseStarted?.Invoke();
            Debug.Log("[GameState] Chase Started");
        }

        public void EndChase(bool arrested)
        {
            if (!IsChaseActive) return;
            IsChaseActive = false;
            CurrentMode = GameMode.FreeRoam;
            PlayerArrested = arrested;
            if (arrested)
            {
                OnPlayerArrested?.Invoke();
            }
            OnChaseEnded?.Invoke();
            Debug.Log($"[GameState] Chase Ended Arrested={arrested}");
        }

        private void Update()
        {
            if (IsChaseActive)
                ChaseTime += Time.deltaTime;
        }

        public void SetDifficulty(AIDifficulty diff)
        {
            Difficulty = diff;
            Debug.Log($"[GameState] Difficulty set to {diff}");
        }
    }
}
