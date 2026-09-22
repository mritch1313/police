using UnityEngine;
using System.Collections.Generic;
using PoliceChase.Core;

namespace PoliceChase.AI
{
    public class PoliceStrategy : MonoBehaviour
    {
        public static PoliceStrategy Instance { get; private set; }

        [Header("Strategy")]
        public float StrategyReevaluationInterval = 3f;
        private float timer = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            if (GameState.Instance == null || !GameState.Instance.IsChaseActive) return;

            timer += Time.deltaTime;
            if (timer > StrategyReevaluationInterval)
            {
                timer = 0f;
                EvaluateStrategy();
            }
        }

        void EvaluateStrategy()
        {
            if (PoliceManager.Instance == null) return;
            var police = PoliceManager.Instance.GetActivePolice();
            if (police.Count == 0) return;

            // Find player
            var player = FindObjectOfType<Vehicle.PlayerCar>();
            if (player == null) return;

            Vector3 playerPos = player.transform.position;
            Vector3 playerVel = player.GetVelocity();

            // If player is off-road, some police should go off-road too, others stay on road
            bool playerOffRoad = IsOffRoad(playerPos);

            // If player stopped, pin
            bool playerStopped = playerVel.magnitude < 1f;

            if (playerStopped)
            {
                foreach (var p in police)
                {
                    if (Vector3.Distance(p.transform.position, playerPos) < 20f)
                        p.ChooseRole(PoliceRole.Pin);
                    else
                        p.ChooseRole(PoliceRole.Block);
                }
                return;
            }

            // If player is fast, use intercept
            if (playerVel.magnitude > 20f)
            {
                // At least 2 intercept
                int interceptCount = 0;
                foreach (var p in police)
                {
                    if (interceptCount < 2)
                    {
                        p.ChooseRole(PoliceRole.Intercept);
                        interceptCount++;
                    }
                }
            }

            // Coordinate to avoid all following same path
            // Spread out
            for (int i = 0; i < police.Count; i++)
            {
                var p = police[i];
                // If two police too close and same role, change one
                for (int j = i + 1; j < police.Count; j++)
                {
                    var other = police[j];
                    if (Vector3.Distance(p.transform.position, other.transform.position) < 10f && p.CurrentRole == other.CurrentRole)
                    {
                        // Change role of one
                        if (other.CurrentRole == PoliceRole.Chase)
                            other.ChooseRole(PoliceRole.Support);
                    }
                }
            }
        }

        bool IsOffRoad(Vector3 pos)
        {
            // Simple check: if not near road collider?
            // For now, check NavMesh area - if not walkable road, consider off-road
            // We'll approximate by checking if position is far from road layer
            Collider[] hits = Physics.OverlapSphere(pos, 5f, LayerMask.GetMask("Road"));
            return hits.Length == 0;
        }

        public void OnPlayerChangedDirection()
        {
            // Force re-evaluation
            timer = StrategyReevaluationInterval;
        }

        public void OnPlayerUsedNitro()
        {
            if (PoliceManager.Instance == null) return;
            foreach (var p in PoliceManager.Instance.GetActivePolice())
            {
                p.OnPlayerNitro();
            }
        }
    }
}
