using UnityEngine;
using System.Collections.Generic;
using PoliceChase.Core;

namespace PoliceChase.World
{
    public class WorldStreamer : MonoBehaviour
    {
        public static WorldStreamer Instance { get; private set; }

        [Header("Streaming")]
        public Transform Player;
        public float ChunkSize = 200f;
        public int RenderDistance = 3;
        public float DespawnDistance = 600f;
        public float UpdateInterval = 0.5f;

        private Dictionary<Vector2Int, WorldChunk> loadedChunks = new Dictionary<Vector2Int, WorldChunk>();
        private WorldGenerator generator;
        private float lastUpdateTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            generator = GetComponent<WorldGenerator>();
            if (generator == null) generator = gameObject.AddComponent<WorldGenerator>();

            if (SettingsManager.Instance != null)
            {
                ChunkSize = SettingsManager.Instance.Vehicle.WorldChunkSize;
                RenderDistance = SettingsManager.Instance.Vehicle.WorldRenderDistance;
                DespawnDistance = SettingsManager.Instance.Vehicle.WorldDespawnDistance;
            }
        }

        private void Start()
        {
            if (Player == null)
            {
                var p = FindObjectOfType<Vehicle.PlayerCar>();
                if (p != null) Player = p.transform;
            }
            UpdateChunks(true);
        }

        private void Update()
        {
            if (Time.time - lastUpdateTime > UpdateInterval)
            {
                lastUpdateTime = Time.time;
                UpdateChunks(false);
            }
        }

        void UpdateChunks(bool force)
        {
            if (Player == null) return;

            Vector2Int playerChunk = WorldToChunkCoord(Player.position);

            // Load nearby chunks
            for (int x = -RenderDistance; x <= RenderDistance; x++)
            {
                for (int y = -RenderDistance; y <= RenderDistance; y++)
                {
                    Vector2Int coord = new Vector2Int(playerChunk.x + x, playerChunk.y + y);
                    if (!loadedChunks.ContainsKey(coord))
                    {
                        var chunk = generator.GenerateChunk(coord, ChunkSize);
                        loadedChunks[coord] = chunk;
                    }
                    // Update LOD based on distance
                    var chunkObj = loadedChunks[coord];
                    float dist = Vector2Int.Distance(playerChunk, coord);
                    int lod = dist > RenderDistance * 0.7f ? 2 : (dist > RenderDistance * 0.4f ? 1 : 0);
                    chunkObj.SetLOD(lod);
                    chunkObj.SetVisible(true);
                }
            }

            // Unload far chunks
            List<Vector2Int> toRemove = new List<Vector2Int>();
            foreach (var kvp in loadedChunks)
            {
                float dist = Vector3.Distance(Player.position, kvp.Value.GetCenter());
                if (dist > DespawnDistance)
                {
                    kvp.Value.Unload();
                    toRemove.Add(kvp.Key);
                    Destroy(kvp.Value.gameObject);
                }
            }
            foreach (var key in toRemove)
                loadedChunks.Remove(key);
        }

        Vector2Int WorldToChunkCoord(Vector3 worldPos)
        {
            int x = Mathf.FloorToInt(worldPos.x / ChunkSize);
            int y = Mathf.FloorToInt(worldPos.z / ChunkSize);
            return new Vector2Int(x, y);
        }

        public WorldChunk GetChunk(Vector2Int coord)
        {
            loadedChunks.TryGetValue(coord, out var chunk);
            return chunk;
        }
    }
}
