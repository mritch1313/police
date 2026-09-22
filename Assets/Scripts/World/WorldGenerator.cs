using UnityEngine;
using UnityEngine.AI;

namespace PoliceChase.World
{
    public class WorldGenerator : MonoBehaviour
    {
        [Header("World")]
        public float ChunkSize = 200f;
        public int Seed = 12345;

        [Header("City")]
        public float CityProbability = 0.3f;
        public float SuburbProbability = 0.3f;
        public float DesertProbability = 0.2f;

        private System.Random rng;

        private void Awake()
        {
            rng = new System.Random(Seed);
        }

        public WorldChunk GenerateChunk(Vector2Int coord, float size)
        {
            GameObject chunkObj = new GameObject($"Chunk_{coord.x}_{coord.y}");
            chunkObj.transform.position = new Vector3(coord.x * size, 0, coord.y * size);
            var chunk = chunkObj.AddComponent<WorldChunk>();
            chunk.Initialize(coord, size);

            // Determine biome based on distance from center
            float distFromCenter = coord.magnitude;
            float cityThreshold = 3f;
            float suburbThreshold = 7f;

            if (distFromCenter < cityThreshold)
                GenerateCityChunk(chunk);
            else if (distFromCenter < suburbThreshold)
                GenerateSuburbChunk(chunk);
            else if (distFromCenter < suburbThreshold + 3f)
                GenerateFieldsChunk(chunk);
            else
                GenerateDesertChunk(chunk);

            // Always generate roads
            GenerateRoads(chunk);

            // Bake NavMesh for chunk? We'll use NavMeshSurface if available, else rely on global
            // For performance, we don't bake per chunk in this simplified version

            chunk.Load();
            return chunk;
        }

        void GenerateCityChunk(WorldChunk chunk)
        {
            int buildingCount = Random.Range(8, 15);
            for (int i = 0; i < buildingCount; i++)
            {
                Vector3 pos = chunk.transform.position + new Vector3(Random.Range(10f, ChunkSize -10f), 0, Random.Range(10f, ChunkSize -10f));
                // Avoid roads - simple check
                if (IsNearRoad(pos)) continue;

                GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.transform.position = pos + Vector3.up * Random.Range(2f, 8f);
                building.transform.localScale = new Vector3(Random.Range(4f, 10f), Random.Range(4f, 16f), Random.Range(4f, 10f));
                building.transform.SetParent(chunk.transform);
                building.layer = LayerMask.NameToLayer("Building");
                if (building.layer == -1) building.layer = 0;
                building.isStatic = true;

                var rend = building.GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(Random.Range(0.7f, 0.9f), Random.Range(0.7f, 0.9f), Random.Range(0.7f, 0.9f));
                    rend.material = mat;
                }

                chunk.RegisterBuilding(building);
            }
            GenerateObstacles(chunk, 5);
        }

        void GenerateSuburbChunk(WorldChunk chunk)
        {
            int buildingCount = Random.Range(4, 8);
            for (int i = 0; i < buildingCount; i++)
            {
                Vector3 pos = chunk.transform.position + new Vector3(Random.Range(10f, ChunkSize -10f), 0, Random.Range(10f, ChunkSize -10f));
                if (IsNearRoad(pos)) continue;

                GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
                building.transform.position = pos + Vector3.up * Random.Range(1.5f, 4f);
                building.transform.localScale = new Vector3(Random.Range(5f, 12f), Random.Range(3f, 6f), Random.Range(5f, 12f));
                building.transform.SetParent(chunk.transform);
                building.layer = LayerMask.NameToLayer("Building");
                if (building.layer == -1) building.layer = 0;
                building.isStatic = true;

                var rend = building.GetComponent<Renderer>();
                if (rend != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    mat.color = new Color(Random.Range(0.8f, 1f), Random.Range(0.6f, 0.8f), Random.Range(0.5f, 0.7f));
                    rend.material = mat;
                }

                chunk.RegisterBuilding(building);
            }
            GenerateObstacles(chunk, 3);
            GenerateFences(chunk);
        }

        void GenerateFieldsChunk(WorldChunk chunk)
        {
            // Open fields with few obstacles
            GenerateObstacles(chunk, 2);
            // Add some walls
            for (int i = 0; i < 2; i++)
            {
                Vector3 pos = chunk.transform.position + new Vector3(Random.Range(0, ChunkSize), 0, Random.Range(0, ChunkSize));
                GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.transform.position = pos + Vector3.up * 1f;
                wall.transform.localScale = new Vector3(Random.Range(10f, 30f), 2f, 0.5f);
                wall.transform.SetParent(chunk.transform);
                wall.layer = LayerMask.NameToLayer("Obstacle");
                chunk.RegisterObstacle(wall);
            }
        }

        void GenerateDesertChunk(WorldChunk chunk)
        {
            // Desert - very open, occasional rocks
            for (int i = 0; i < Random.Range(0, 3); i++)
            {
                Vector3 pos = chunk.transform.position + new Vector3(Random.Range(0, ChunkSize), 0, Random.Range(0, ChunkSize));
                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.transform.position = pos + Vector3.up * 0.5f;
                rock.transform.localScale = Vector3.one * Random.Range(1f, 3f);
                rock.transform.SetParent(chunk.transform);
                rock.layer = LayerMask.NameToLayer("Obstacle");
                chunk.RegisterObstacle(rock);
            }
        }

        void GenerateRoads(WorldChunk chunk)
        {
            // Create simple road plane
            GameObject road = GameObject.CreatePrimitive(PrimitiveType.Plane);
            road.name = "Road";
            road.transform.position = chunk.transform.position + new Vector3(ChunkSize/2, 0.01f, ChunkSize/2);
            road.transform.localScale = new Vector3(ChunkSize/10f * 0.3f, 1, ChunkSize/10f * 0.3f); // plane is 10x10
            // Rotate? Keep simple
            road.transform.SetParent(chunk.transform);
            road.layer = LayerMask.NameToLayer("Road");
            if (road.layer == -1) road.layer = 0;

            var rend = road.GetComponent<Renderer>();
            if (rend != null)
            {
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(0.2f, 0.2f, 0.2f);
                rend.material = mat;
            }

            // Add NavMeshModifier if AI Navigation package present
            // For simplicity, we add a ground plane for NavMesh
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = chunk.transform.position + new Vector3(ChunkSize/2, 0, ChunkSize/2);
            ground.transform.localScale = new Vector3(ChunkSize/10f, 1, ChunkSize/10f);
            ground.transform.SetParent(chunk.transform);
            ground.layer = LayerMask.NameToLayer("Ground");
            if (ground.layer == -1) ground.layer = 0;
            ground.isStatic = true;

            chunk.RegisterRoad(road);
        }

        void GenerateObstacles(WorldChunk chunk, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = chunk.transform.position + new Vector3(Random.Range(5f, ChunkSize -5f), 0, Random.Range(5f, ChunkSize -5f));
                if (IsNearRoad(pos)) continue;

                GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obs.transform.position = pos + Vector3.up * 0.5f;
                obs.transform.localScale = new Vector3(Random.Range(1f, 3f), 1f, Random.Range(1f, 3f));
                obs.transform.SetParent(chunk.transform);
                obs.layer = LayerMask.NameToLayer("Obstacle");
                chunk.RegisterObstacle(obs);
            }
        }

        void GenerateFences(WorldChunk chunk)
        {
            // Simple fence around some areas
            Vector3 center = chunk.transform.position + new Vector3(ChunkSize/2, 0, ChunkSize/2);
            for (int i = 0; i < 4; i++)
            {
                GameObject fence = GameObject.CreatePrimitive(PrimitiveType.Cube);
                fence.transform.position = center + new Vector3(Random.Range(-20f, 20f), 0.5f, Random.Range(-20f, 20f));
                fence.transform.localScale = new Vector3(10f, 1f, 0.2f);
                fence.transform.SetParent(chunk.transform);
                fence.layer = LayerMask.NameToLayer("Obstacle");
                chunk.RegisterObstacle(fence);
            }
        }

        bool IsNearRoad(Vector3 pos)
        {
            // Simplified: if within 10 units of chunk center line, consider road
            // Roads are at chunk center in this simplified version
            // So avoid center
            return false; // for now allow everywhere
        }
    }
}
