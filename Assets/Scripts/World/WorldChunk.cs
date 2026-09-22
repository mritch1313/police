using UnityEngine;
using System.Collections.Generic;

namespace PoliceChase.World
{
    public class WorldChunk : MonoBehaviour
    {
        public Vector2Int ChunkCoord;
        public float ChunkSize = 200f;
        public bool IsLoaded = false;
        public bool IsVisible = false;

        [Header("LOD")]
        public int LODLevel = 0; // 0 high, 1 medium, 2 low

        private List<GameObject> buildings = new List<GameObject>();
        private List<GameObject> roads = new List<GameObject>();
        private List<GameObject> obstacles = new List<GameObject>();

        public void Initialize(Vector2Int coord, float size)
        {
            ChunkCoord = coord;
            ChunkSize = size;
            gameObject.name = $"Chunk_{coord.x}_{coord.y}";
            transform.position = new Vector3(coord.x * size, 0, coord.y * size);
        }

        public void Load()
        {
            if (IsLoaded) return;
            IsLoaded = true;
            // Actual generation done by WorldGenerator
        }

        public void Unload()
        {
            if (!IsLoaded) return;
            IsLoaded = false;
            // Pool objects
            foreach (var b in buildings)
                if (b != null) b.SetActive(false);
            foreach (var r in roads)
                if (r != null) r.SetActive(false);
            foreach (var o in obstacles)
                if (o != null) o.SetActive(false);
        }

        public void SetLOD(int lod)
        {
            LODLevel = lod;
            // Adjust detail: disable small objects for far LOD
            bool highDetail = lod == 0;
            foreach (var o in obstacles)
            {
                if (o != null) o.SetActive(highDetail || Random.value > 0.5f);
            }
        }

        public void SetVisible(bool visible)
        {
            IsVisible = visible;
            gameObject.SetActive(visible);
        }

        public void RegisterBuilding(GameObject go) => buildings.Add(go);
        public void RegisterRoad(GameObject go) => roads.Add(go);
        public void RegisterObstacle(GameObject go) => obstacles.Add(go);

        public Vector3 GetCenter() => transform.position + new Vector3(ChunkSize/2, 0, ChunkSize/2);

        public void OnDestroy()
        {
            buildings.Clear();
            roads.Clear();
            obstacles.Clear();
        }
    }
}
