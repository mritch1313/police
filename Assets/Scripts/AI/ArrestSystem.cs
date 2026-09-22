using UnityEngine;
using PoliceChase.Core;
using PoliceChase.Vehicle;
using System.Collections.Generic;

namespace PoliceChase.AI
{
    public class ArrestSystem : MonoBehaviour
    {
        [Header("Arrest")]
        public float ImmobilizeTimeRequired = 3f;
        public float ArrestRadius = 8f;
        public int MinPoliceForArrest = 2;
        public float MinPlayerSpeedForArrest = 1f;
        public float FreeSpaceCheckRadius = 6f;

        private float immobilizeTimer = 0f;
        private PlayerCar playerCar;
        private bool isChecking = false;

        public System.Action OnArrested;

        private void Start()
        {
            playerCar = FindObjectOfType<PlayerCar>();
            if (SettingsManager.Instance != null)
            {
                ImmobilizeTimeRequired = SettingsManager.Instance.Vehicle.ArrestImmobilizeTime;
                MinPoliceForArrest = (int)SettingsManager.Instance.Vehicle.ArrestMinPolice;
                ArrestRadius = SettingsManager.Instance.Vehicle.ArrestRadius;
            }
        }

        private void Update()
        {
            if (GameState.Instance == null || !GameState.Instance.IsChaseActive) return;
            if (playerCar == null) return;

            EvaluateArrestCondition();
        }

        void EvaluateArrestCondition()
        {
            // Player must be slow
            float speed = playerCar.GetSpeedKmh();
            if (speed > MinPlayerSpeedForArrest * 3.6f) // convert
            {
                immobilizeTimer = 0f;
                return;
            }

            // Check police around
            var policeList = PoliceManager.Instance != null ? PoliceManager.Instance.GetActivePolice() : null;
            if (policeList == null || policeList.Count < MinPoliceForArrest)
            {
                immobilizeTimer = 0f;
                return;
            }

            int policeClose = 0;
            bool hasFrontBlock = false;
            bool hasSideBlock = false;
            bool hasRearBlock = false;

            Vector3 playerPos = playerCar.transform.position;
            Vector3 playerForward = playerCar.transform.forward;

            foreach (var police in policeList)
            {
                float dist = Vector3.Distance(police.transform.position, playerPos);
                if (dist > ArrestRadius) continue;

                policeClose++;

                Vector3 toPolice = (police.transform.position - playerPos).normalized;
                float dot = Vector3.Dot(playerForward, toPolice);

                if (dot > 0.5f) hasFrontBlock = true;
                else if (dot < -0.5f) hasRearBlock = true;
                else hasSideBlock = true;
            }

            // Need at least front and side or 2 sides etc.
            bool blocked = false;
            if (policeClose >= MinPoliceForArrest)
            {
                // Check free space around player
                float freeSpace = CalculateFreeSpace(playerPos);
                if (freeSpace < 0.3f) // less than 30% free
                    blocked = true;

                // Also need front block or two side blocks
                if (hasFrontBlock && (hasSideBlock || hasRearBlock))
                    blocked = true;
                else if (policeClose >= 3 && hasSideBlock)
                    blocked = true;
            }

            if (blocked)
            {
                immobilizeTimer += Time.deltaTime;
                if (immobilizeTimer >= ImmobilizeTimeRequired)
                {
                    ArrestPlayer();
                }
            }
            else
            {
                immobilizeTimer = Mathf.Max(0f, immobilizeTimer - Time.deltaTime);
            }
        }

        float CalculateFreeSpace(Vector3 pos)
        {
            // Raycast in 8 directions to check free space
            int freeDirections = 0;
            int total = 8;
            for (int i = 0; i < total; i++)
            {
                float angle = i * 45f;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                if (!Physics.Raycast(pos + Vector3.up, dir, FreeSpaceCheckRadius, LayerMask.GetMask("Building", "Obstacle", "Police")))
                {
                    freeDirections++;
                }
            }
            return (float)freeDirections / total;
        }

        void ArrestPlayer()
        {
            Debug.Log("[ArrestSystem] Player Arrested!");
            OnArrested?.Invoke();
            GameState.Instance?.EndChase(true);
            ChaseManager.Instance?.EndChase(true);
        }

        public float GetImmobilizeProgress() => Mathf.Clamp01(immobilizeTimer / ImmobilizeTimeRequired);
    }
}
