using UnityEngine;
using System.Collections.Generic;
using PoliceChase.Core;
using PoliceChase.Vehicle;

namespace PoliceChase.AI
{
    public class PoliceManager : MonoBehaviour
    {
        public static PoliceManager Instance { get; private set; }

        [Header("Police")]
        public GameObject PolicePrefab;
        public int MaxPolice = 6;
        public float SpawnRadiusMin = 50f;
        public float SpawnRadiusMax = 150f;

        private List<PoliceAI> activePolice = new List<PoliceAI>();
        private Transform playerTransform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(Transform player)
        {
            playerTransform = player;
        }

        public void SpawnPolice(int count, AIDifficulty difficulty)
        {
            if (playerTransform == null) return;
            ClearPolice();

            for (int i = 0; i < count; i++)
            {
                Vector3 spawnPos;
                Quaternion spawnRot;
                if (SpawnManager.Instance != null && SpawnManager.Instance.FindSafeSpawnPosition(playerTransform.position, out spawnPos, out spawnRot))
                {
                    GameObject policeObj = Instantiate(PolicePrefab, spawnPos, spawnRot);
                    var policeAI = policeObj.GetComponent<PoliceAI>();
                    var policeCar = policeObj.GetComponent<PoliceCar>();

                    if (policeAI != null)
                    {
                        policeAI.Initialize(playerTransform, difficulty);
                        activePolice.Add(policeAI);
                    }
                    if (policeCar != null)
                    {
                        policeCar.SetActive(true);
                    }
                }
            }

            AssignRoles();
        }

        public void AssignRoles()
        {
            if (activePolice.Count == 0) return;

            // Dynamic role distribution
            // Example for Advanced: 1 Chase, 1 Intercept, 1 Block, rest Support/Pin
            if (GameState.Instance != null && GameState.Instance.Difficulty == AIDifficulty.Advanced)
            {
                if (activePolice.Count >= 1) activePolice[0].ChooseRole(PoliceRole.Chase);
                if (activePolice.Count >= 2) activePolice[1].ChooseRole(PoliceRole.Intercept);
                if (activePolice.Count >= 3) activePolice[2].ChooseRole(PoliceRole.Block);
                for (int i = 3; i < activePolice.Count; i++)
                {
                    activePolice[i].ChooseRole(i % 2 == 0 ? PoliceRole.Pin : PoliceRole.Support);
                }
            }
            else if (GameState.Instance != null && GameState.Instance.Difficulty == AIDifficulty.Normal)
            {
                // 1 Chase, 1 Intercept, rest Support
                if (activePolice.Count >= 1) activePolice[0].ChooseRole(PoliceRole.Chase);
                if (activePolice.Count >= 2) activePolice[1].ChooseRole(PoliceRole.Intercept);
                for (int i = 2; i < activePolice.Count; i++)
                    activePolice[i].ChooseRole(PoliceRole.Support);
            }
            else
            {
                // Simple: all chase
                foreach (var p in activePolice)
                    p.ChooseRole(PoliceRole.Chase);
            }
        }

        public void UpdateRoles()
        {
            // Re-evaluate every few seconds
            AssignRoles();
        }

        public void ClearPolice()
        {
            foreach (var p in activePolice)
            {
                if (p != null)
                    Destroy(p.gameObject);
            }
            activePolice.Clear();
        }

        public List<PoliceAI> GetActivePolice() => activePolice;

        public int GetPoliceCount() => activePolice.Count;

        public bool AreAllPoliceStuck()
        {
            // If all police are far and stuck, maybe need respawn
            return false;
        }
    }
}
