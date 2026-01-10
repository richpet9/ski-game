using System.Collections.Generic;
using SkiGame.Model.Terrain;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SkiGame.View.World
{
    // The NavMeshSurface stays here to bake over all chunks.
    [RequireComponent(typeof(NavMeshSurface))]
    public sealed class TerrainView : MonoBehaviour
    {
        [Header("Assets")]
        [SerializeField]
        private Material _terrainMaterial;

        [SerializeField]
        private Mesh _treeMesh;

        [SerializeField]
        private Material _treeMaterial;

        [SerializeField]
        private LayerMask _terrainLayer;

        private const int CHUNK_SIZE = 32;

        private readonly List<TerrainChunk> _chunks = new List<TerrainChunk>();
        private NavMeshSurface _surface;
        private Map _map;
        private bool _isDirty;

        private void Awake()
        {
            _surface = GetComponent<NavMeshSurface>();

            // Important: We still want to bake physics colliders,
            // which are now located on the child Chunk objects.
            _surface.collectObjects = CollectObjects.Children;
            _surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        }

        public void Initialize(Map map, int width, int height)
        {
            _map = map;

            // Clear existing chunks if any.
            foreach (TerrainChunk chunk in _chunks)
            {
                Destroy(chunk.gameObject);
            }
            _chunks.Clear();

            // Create Chunks.
            for (int z = 0; z < height; z += CHUNK_SIZE)
            {
                for (int x = 0; x < width; x += CHUNK_SIZE)
                {
                    CreateChunk(x, z);
                }
            }

            _map.OnMapChange += SetDirty;
            _map.OnFoliageChange += SetDirty;
        }

        private void CreateChunk(int x, int z)
        {
            GameObject chunkObj = new GameObject($"Chunk_{x}_{z}");
            chunkObj.transform.SetParent(transform, false);

            // Setup components.
            TerrainChunk chunk = chunkObj.AddComponent<TerrainChunk>();
            MeshRenderer mr = chunkObj.GetComponent<MeshRenderer>();
            FoliageView foliage = chunkObj.AddComponent<FoliageView>();

            // TODO: Get treescale from mapconfig.
            foliage.Initialize(_map, treeScale: 1f, x, z, CHUNK_SIZE, _treeMesh, _treeMaterial);

            // Assign shared material (ensure one is set in Inspector or loaded).
            if (_terrainMaterial != null)
            {
                mr.sharedMaterial = _terrainMaterial;
            }

            chunk.Initialize(_map, x, z, CHUNK_SIZE, gameObject.layer);
            _chunks.Add(chunk);
        }

        private void OnDestroy()
        {
            if (_map != null)
            {
                _map.OnMapChange -= SetDirty;
                _map.OnFoliageChange -= SetDirty;
            }
        }

        private void SetDirty()
        {
            _isDirty = true;
        }

        private void LateUpdate()
        {
            if (_isDirty)
            {
                // TODO: Check which chunks need rebuilding, do not rebuild all.
                RebuildAllChunks();
                _isDirty = false;
            }
        }

        private void RebuildAllChunks()
        {
            if (_map == null)
            {
                return;
            }

            foreach (TerrainChunk chunk in _chunks)
            {
                chunk.Rebuild();
            }

            // Re-bake NavMesh after all chunks are updated.
            _surface.BuildNavMesh();
        }
    }
}
