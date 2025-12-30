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

        private void Awake()
        {
            _surface = GetComponent<NavMeshSurface>();
            // Settings for the bake.
            _surface.collectObjects = CollectObjects.Children;
            _surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
        }

        public void Render(MeshData meshData)
        {
            Mesh mesh = new Mesh()
            {
                vertices = meshData.Vertices,
                triangles = meshData.Triangles,
                uv = meshData.UVs,
            };
            mesh.RecalculateNormals();
            UpdateTerrainMesh(mesh);

            Debug.Log("Rendered Terrain.");
        }

        public void ClearMesh()
        {
            GetComponent<MeshFilter>().mesh = null;
            GetComponent<MeshCollider>().sharedMesh = null;
        }

        public void UpdateTerrainMesh(Mesh mesh)
        {
            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;
            _surface.BuildNavMesh();
        }
    }
}
