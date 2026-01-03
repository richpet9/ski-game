using SkiGame.Model.Terrain;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SkiGame.View.World
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    [RequireComponent(typeof(NavMeshSurface))]
    public class TerrainView : MonoBehaviour
    {
        private NavMeshSurface _surface;
        private MeshFilter _visualFilter;
        private MeshCollider _physicsCollider;
        private Map _map;
        private int _width;
        private int _height;
        private bool _isDirty;

        private void Awake()
        {
            _surface = GetComponent<NavMeshSurface>();
            _visualFilter = GetComponent<MeshFilter>();
            _physicsCollider = GetComponent<MeshCollider>();

            _surface.collectObjects = CollectObjects.Children;
            // Crucial: Only bake the physics collider, not the rendered mesh.
            // You might need to put the Physics mesh on a child object or specific layer
            // if NavMeshSurface picks up the MeshFilter automatically.
            _surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        }

        public void Initialize(Map map, int width, int height)
        {
            _map = map;
            _width = width;
            _height = height;
            _map.OnMapChanged += SetDirty;
        }

        private void OnDestroy()
        {
            if (_map != null)
            {
                _map.OnMapChanged -= SetDirty;
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
                RebuildMesh();
                _isDirty = false;
            }
        }

        private void RebuildMesh()
        {
            if (_map == null)
            {
                return;
            }

            MeshData visualData = TerrainVoxelBuilder.Build(_map, _width, _height);
            Mesh visualMesh = new Mesh
            {
                name = "Terrain Visual Mesh",
                vertices = visualData.Vertices,
                triangles = visualData.Triangles,
                uv = visualData.UVs,
                colors = visualData.Colors,
            };
            visualMesh.RecalculateNormals();
            _visualFilter.mesh = visualMesh;

            MeshData physicsData = TerrainMeshBuilder.Build(_map, _width, _height);
            Mesh physicsMesh = new Mesh
            {
                name = "Terrain Physics Mesh",
                vertices = physicsData.Vertices,
                triangles = physicsData.Triangles,
            };

            _physicsCollider.sharedMesh = physicsMesh;
            _surface.BuildNavMesh();
        }
    }
}
