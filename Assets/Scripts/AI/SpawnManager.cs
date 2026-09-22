using UnityEngine;
using UnityEngine.AI;

namespace PoliceChase.AI
{
    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager Instance { get; private set; }

        [Header("Spawn")]
        public float MinDistanceFromPlayer = 60f;
        public float MaxDistanceFromPlayer = 150f;
        public float SafeRadius = 5f;
        public LayerMask ObstacleMask;
        public LayerMask GroundMask;
        public int MaxAttempts = 30;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            // Default masks if not set
            if (GroundMask == 0) GroundMask = LayerMask.GetMask("Ground", "Road", "Default");
            if (ObstacleMask == 0) ObstacleMask = LayerMask.GetMask("Building", "Obstacle");
        }

        public bool FindSafeSpawnPosition(Vector3 playerPos, out Vector3 spawnPos, out Quaternion spawnRot)
        {
            spawnPos = Vector3.zero;
            spawnRot = Quaternion.identity;

            for (int i = 0; i < MaxAttempts; i++)
            {
                // Random direction
                Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(MinDistanceFromPlayer, MaxDistanceFromPlayer);
                Vector3 candidate = playerPos + new Vector3(randomCircle.x, 0, randomCircle.y);

                // Check ground
                if (!HasGround(candidate)) continue;

                // Check free space
                if (!IsFreeSpace(candidate)) continue;

                // Check not inside building/wall
                if (IsInsideObstacle(candidate)) continue;

                // Check NavMesh
                if (!IsOnNavMesh(candidate, out Vector3 navPos)) continue;

                // Check distance to player is enough and not in front directly
                Vector3 toPlayer = playerPos - candidate;
                float dist = toPlayer.magnitude;
                if (dist < MinDistanceFromPlayer) continue;

                // Check if player is looking at spawn point? Avoid spawning directly in front
                // For simplicity, check angle between player forward and to spawn
                // We don't have player forward here, but we can still avoid too close front

                // Check route availability
                if (!HasRouteToPlayer(navPos, playerPos)) continue;

                spawnPos = navPos;
                // Face towards player
                Vector3 dir = (playerPos - spawnPos).normalized;
                dir.y = 0;
                if (dir.magnitude > 0.1f)
                    spawnRot = Quaternion.LookRotation(dir);
                else
                    spawnRot = Quaternion.identity;

                return true;
            }

            return false;
        }

        bool HasGround(Vector3 pos)
        {
            RaycastHit hit;
            if (Physics.Raycast(pos + Vector3.up * 50f, Vector3.down, out hit, 100f, GroundMask))
            {
                return true;
            }
            // Also check NavMesh as ground
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(pos, out navHit, 10f, NavMesh.AllAreas))
                return true;

            return false;
        }

        bool IsFreeSpace(Vector3 pos)
        {
            // Check overlap sphere for obstacles
            Collider[] cols = Physics.OverlapSphere(pos, SafeRadius, ObstacleMask);
            return cols.Length == 0;
        }

        bool IsInsideObstacle(Vector3 pos)
        {
            // Raycast up to check if inside building?
            // Simple: check if there is collider overlapping at pos
            Collider[] cols = Physics.OverlapSphere(pos, 1f, ObstacleMask);
            return cols.Length > 0;
        }

        bool IsOnNavMesh(Vector3 pos, out Vector3 result)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, 10f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
            result = pos;
            return false;
        }

        bool HasRouteToPlayer(Vector3 from, Vector3 to)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            {
                return path.status == NavMeshPathStatus.PathComplete || path.status == NavMeshPathStatus.PathPartial;
            }
            return false;
        }

        public void DrawDebug(Vector3 pos)
        {
            Debug.DrawLine(pos, pos + Vector3.up * 5f, Color.green, 2f);
        }
    }
}
