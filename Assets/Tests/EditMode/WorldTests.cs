using NUnit.Framework;
using UnityEngine;
using PoliceChase.World;

namespace PoliceChase.Tests
{
    public class WorldTests
    {
        [Test]
        public void WorldChunk_Creation()
        {
            var go = new GameObject("Chunk");
            var chunk = go.AddComponent<WorldChunk>();
            chunk.Initialize(new Vector2Int(0, 0), 200f);

            Assert.AreEqual(new Vector2Int(0, 0), chunk.ChunkCoord);
            Assert.AreEqual(200f, chunk.ChunkSize);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void WorldChunk_LOD()
        {
            var go = new GameObject("Chunk");
            var chunk = go.AddComponent<WorldChunk>();
            chunk.Initialize(new Vector2Int(1, 1), 200f);
            chunk.SetLOD(0);
            Assert.AreEqual(0, chunk.LODLevel);
            chunk.SetLOD(2);
            Assert.AreEqual(2, chunk.LODLevel);

            Object.DestroyImmediate(go);
        }

        [Test]
        public void WorldGenerator_CreatesChunk()
        {
            var go = new GameObject("Generator");
            var gen = go.AddComponent<WorldGenerator>();
            var chunk = gen.GenerateChunk(new Vector2Int(0, 0), 200f);

            Assert.IsNotNull(chunk, "Chunk should be generated");
            Assert.IsTrue(chunk.IsLoaded, "Chunk should be loaded");

            Object.DestroyImmediate(chunk.gameObject);
            Object.DestroyImmediate(go);
        }
    }
}
