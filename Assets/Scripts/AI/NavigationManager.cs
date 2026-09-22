using UnityEngine;
using UnityEngine.AI;

namespace PoliceChase.AI
{
    public class NavigationManager : MonoBehaviour
    {
        public static NavigationManager Instance { get; private set; }

        [Header("NavMesh")]
        public bool UseNavMesh = true;
        public float SampleDistance = 5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public bool IsPositionOnNavMesh(Vector3 pos, out Vector3 result)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, SampleDistance, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
            result = pos;
            return false;
        }

        public bool IsPositionValid(Vector3 pos)
        {
            Vector3 result;
            return IsPositionOnNavMesh(pos, out result);
        }

        public Vector3 GetClosestValidPosition(Vector3 pos)
        {
            Vector3 result;
            if (IsPositionOnNavMesh(pos, out result))
                return result;
            return pos;
        }

        public bool HasValidPath(Vector3 from, Vector3 to)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path))
            {
                return path.status == NavMeshPathStatus.PathComplete;
            }
            return false;
        }
    }
}
