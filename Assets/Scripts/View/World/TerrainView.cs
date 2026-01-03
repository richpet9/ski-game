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
        private Map _map;
        private int _width;
        private int _height;

        private void Awake()
        {
            _surface = GetComponent<NavMeshSurface>();
            _surface.collectObjects = CollectObjects.Children;
            _surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        }

        public void Initialize(Map map, int width, int height)
        {
            _map = map;
            _width = width;
            _height = height;

            // Subscribe to model changes
            _map.OnMapChanged += RebuildMesh;

            // Initial build
            RebuildMesh();
        }

        private void OnDestroy()
        {
            if (_map != null)
            {
                _map.OnMapChanged -= RebuildMesh;
            }
        }

        private void RebuildMesh()
        {
            if (_map == null)
                return;

            MeshData meshData = TerrainMeshBuilder.Build(_map, _width, _height);

            Mesh mesh = new Mesh
            {
                vertices = meshData.Vertices,
                triangles = meshData.Triangles,
                uv = meshData.UVs,
                colors = meshData.Colors,
            };

            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;

            // Note: For performance, we might want to delay NavMesh rebaking or debounce it.
            _surface.BuildNavMesh();
        }
    }
}
