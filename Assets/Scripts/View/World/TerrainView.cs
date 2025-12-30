using SkiGame.Model.Terrain;
using SkiGame.View.Data;
using UnityEngine;

namespace SkiGame.View.World
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class TerrainView : MonoBehaviour
    {
        public void Render(MeshData meshData)
        {
            Mesh mesh = new Mesh()
            {
                vertices = meshData.Vertices,
                triangles = meshData.Triangles,
                uv = meshData.UVs,
            };
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
            GetComponent<MeshCollider>().sharedMesh = mesh;

            Debug.Log("Rendered Terrain.");
        }

        public void ClearMesh()
        {
            GetComponent<MeshFilter>().mesh = null;
            GetComponent<MeshCollider>().sharedMesh = null;
        }
    }
}
