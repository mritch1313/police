using UnityEngine;

namespace PoliceChase.Utils
{
    public class LODManager : MonoBehaviour
    {
        public Transform Player;
        public float HighDetailDistance = 50f;
        public float MediumDetailDistance = 150f;
        public float LowDetailDistance = 300f;

        private LODGroup[] lodGroups;

        private void Start()
        {
            if (Player == null)
            {
                var p = FindObjectOfType<Vehicle.PlayerCar>();
                if (p != null) Player = p.transform;
            }
            lodGroups = FindObjectsOfType<LODGroup>();
        }

        private void Update()
        {
            if (Player == null) return;
            // Simple LOD culling based on distance
            foreach (var lod in lodGroups)
            {
                if (lod == null) continue;
                float dist = Vector3.Distance(Player.position, lod.transform.position);
                if (dist > LowDetailDistance)
                    lod.gameObject.SetActive(false);
                else
                    lod.gameObject.SetActive(true);
            }
        }
    }
}
