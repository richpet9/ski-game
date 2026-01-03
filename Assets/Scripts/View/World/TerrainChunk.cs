using SkiGame.Model.Terrain;
using UnityEngine;
using UnityEngine.Rendering;

namespace SkiGame.View.World
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public sealed class TerrainChunk : MonoBehaviour
    {
        private MeshFilter _visualFilter;
        private MeshCollider _physicsCollider;

        private Map _map;
        private int _startX;
        private int _startZ;
        private int _chunkSize;

        private void Awake()
        {
            _visualFilter = GetComponent<MeshFilter>();
            _physicsCollider = GetComponent<MeshCollider>();
        }

        public void Initialize(Map map, int startX, int startZ, int size, int terrainLayer)
        {
            _map = map;
            _startX = startX;
            _startZ = startZ;
            _chunkSize = size;
            gameObject.layer = terrainLayer;

            Rebuild();
        }

        public void Rebuild()
        {
            // 1. Visual Mesh (Voxels).
            MeshData visualData = TerrainMeshVoxelBuilder.Build(_map, _startX, _startZ, _chunkSize);
            Mesh visualMesh = new Mesh
            {
                name = $"Chunk_Visual_{_startX}_{_startZ}",
                indexFormat = IndexFormat.UInt32,
            };

            visualMesh.SetVertices(visualData.Vertices);
            visualMesh.SetTriangles(visualData.Triangles, 0);
            visualMesh.SetColors(visualData.Colors);
            visualMesh.SetUVs(0, visualData.UVs);
            visualMesh.RecalculateNormals();

            _visualFilter.mesh = visualMesh;

            // 2. Physics Mesh (Smooth).
            MeshData physicsData = TerrainMeshPhysicsBuilder.Build(
                _map,
                _startX,
                _startZ,
                _chunkSize
            );
            Mesh physicsMesh = new Mesh
            {
                name = $"Chunk_Physics_{_startX}_{_startZ}",
                vertices = physicsData.Vertices,
                triangles = physicsData.Triangles,
            };

            // Physics mesh doesn't strictly need normals for collision, but Recalc is
            // cheap for low poly.
            physicsMesh.RecalculateNormals();

            _physicsCollider.sharedMesh = physicsMesh;
        }
    }
}
