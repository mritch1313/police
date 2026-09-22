using UnityEngine;
using PoliceChase.Core;
using PoliceChase.Vehicle;

namespace PoliceChase.AI
{
    public class ChaseManager : MonoBehaviour
    {
        public static ChaseManager Instance { get; private set; }

        [Header("Chase")]
        public Transform Player;
        public int PoliceCount = 3;

        private bool chaseActive = false;

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
            if (GameState.Instance != null)
            {
                GameState.Instance.OnChaseStarted += OnChaseStarted;
                GameState.Instance.OnChaseEnded += OnChaseEnded;
            }

            // Find player
            if (Player == null)
            {
                var p = FindObjectOfType<PlayerCar>();
                if (p != null) Player = p.transform;
            }
        }

        public void StartChase()
        {
            if (chaseActive) return;
            if (Player == null) return;

            chaseActive = true;
            GameState.Instance?.StartChase();

            // Initialize managers
            PoliceManager.Instance?.Initialize(Player);
            PoliceManager.Instance?.SpawnPolice(PoliceCount, GameState.Instance.Difficulty);

            Debug.Log("[ChaseManager] Chase started with " + PoliceCount + " police");
        }

        public void EndChase(bool arrested)
        {
            if (!chaseActive) return;
            chaseActive = false;
            GameState.Instance?.EndChase(arrested);
            PoliceManager.Instance?.ClearPolice();
        }

        void OnChaseStarted()
        {
            chaseActive = true;
        }

        void OnChaseEnded()
        {
            chaseActive = false;
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
