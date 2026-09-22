using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

namespace PoliceChase.AI
{
    public class RoutePlanner : MonoBehaviour
    {
        public float WaypointReachDistance = 5f;
        public float ReplanDistance = 10f;

        private NavMeshPath currentPath;
        private int currentWaypointIndex = 0;
        private Vector3 targetPosition;

        private void Awake()
        {
            currentPath = new NavMeshPath();
        }

        public bool CalculateRoute(Vector3 from, Vector3 to, int areaMask = NavMesh.AllAreas)
        {
            targetPosition = to;
            bool found = NavMesh.CalculatePath(from, to, areaMask, currentPath);
            if (found && currentPath.corners.Length > 0)
            {
                currentWaypointIndex = 0;
                return true;
            }
            return false;
        }

        public bool CalculateAlternativeRoutes(Vector3 from, Vector3 to, int numAlternatives, out List<NavMeshPath> alternatives)
        {
            alternatives = new List<NavMeshPath>();
            // Main route
            if (CalculateRoute(from, to))
            {
                alternatives.Add(currentPath);
            }

            // Try offset targets for alternatives
            for (int i = 0; i < numAlternatives -1; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 10f;
                offset.y = 0;
                Vector3 altTarget = to + offset;
                NavMeshPath altPath = new NavMeshPath();
                if (NavMesh.CalculatePath(from, altTarget, NavMesh.AllAreas, altPath) && altPath.corners.Length > 1)
                {
                    alternatives.Add(altPath);
                }
            }

            return alternatives.Count > 0;
        }

        public Vector3 GetCurrentWaypoint()
        {
            if (currentPath == null || currentPath.corners.Length == 0)
                return targetPosition;

            if (currentWaypointIndex >= currentPath.corners.Length)
                return currentPath.corners[currentPath.corners.Length -1];

            return currentPath.corners[currentWaypointIndex];
        }

        public bool AdvanceWaypoint(Vector3 currentPos)
        {
            Vector3 wp = GetCurrentWaypoint();
            if (Vector3.Distance(currentPos, wp) < WaypointReachDistance)
            {
                currentWaypointIndex++;
                return true;
            }
            return false;
        }

        public bool IsRouteFinished(Vector3 currentPos)
        {
            if (currentPath == null) return true;
            if (currentWaypointIndex >= currentPath.corners.Length) return true;
            return Vector3.Distance(currentPos, targetPosition) < WaypointReachDistance;
        }

        public NavMeshPath GetPath() => currentPath;

        public float GetRemainingDistance(Vector3 from)
        {
            if (currentPath == null || currentPath.corners.Length == 0) return float.MaxValue;
            float dist = 0f;
            Vector3 prev = from;
            for (int i = currentWaypointIndex; i < currentPath.corners.Length; i++)
            {
                dist += Vector3.Distance(prev, currentPath.corners[i]);
                prev = currentPath.corners[i];
            }
            return dist;
        }

        public void Clear()
        {
            currentPath?.ClearCorners();
            currentWaypointIndex = 0;
        }
    }
}
