using SkiGame.Model.Terrain;
using SkiGame.View.Data;
using UnityEngine;

namespace SkiGame.View.World
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class MountainGen : MonoBehaviour
    {
        private Mesh _mesh;
        private Vector3[] _vertices;
        private float[] _vertexHeights;

        public void ClearMesh()
        {
            GetComponent<MeshFilter>().mesh = null;
            GetComponent<MeshCollider>().sharedMesh = null;
        }

        public void Generate(MapConfig mapConfig, MapData mapData)
        {
            GenerateVertices(mapConfig);
            GenerateMapData(mapConfig, mapData);
            GenerateTriangles(mapConfig);
            Debug.Log("Generated!");
        }

        private void GenerateVertices(MapConfig mapConfig)
        {
            int width = mapConfig.Width;
            int height = mapConfig.Height;
            float scale = mapConfig.NoiseScale;

            _mesh = new Mesh();
            _vertices = new Vector3[(width + 1) * (height + 1)];
            _vertexHeights = new float[(width + 1) * (height + 1)];

            Vector2 center = new(width / 2f, height / 2f);
            float xOffset = mapConfig.Seed + 9999f;
            float zOffset = mapConfig.Seed + 9999f;

            for (int z = 0; z <= height; z++)
            {
                for (int x = 0; x <= width; x++)
                {
                    // 1. Calculate base noise.
                    float xCoord = (float)x / width * scale + xOffset;
                    float zCoord = (float)z / height * scale + zOffset;
                    float noise = Mathf.PerlinNoise(xCoord, zCoord);

                    // 2. Apply "Cone" shape to force a central peak.
                    float distFromCenter = Vector2.Distance(new Vector2(x, z), center);
                    float maxDist = width / 2f;
                    float mask = 1f - Mathf.Clamp01(distFromCenter / maxDist);

                    // 3. Apply Curve (Plateaus).
                    // Evaluate the combined noise + mask through your curve.
                    float finalHeight =
                        mapConfig.HeightCurve.Evaluate(noise * mask) * mapConfig.MountainHeight;

                    _vertices[z * (width + 1) + x] = new Vector3(x, finalHeight, z);
                    _vertexHeights[z * (width + 1) + x] = finalHeight;
                }
            }
        }

        private void GenerateMapData(MapConfig mapConfig, MapData mapData)
        {
            for (int z = 0; z < mapConfig.Height; z++)
            {
                for (int x = 0; x < mapConfig.Width; x++)
                {
                    // Calculate average height of the quad
                    float h1 = _vertexHeights[z * (mapConfig.Width + 1) + x];
                    float h2 = _vertexHeights[(z + 1) * (mapConfig.Width + 1) + x];
                    float h3 = _vertexHeights[z * (mapConfig.Width + 1) + x + 1];
                    float h4 = _vertexHeights[(z + 1) * (mapConfig.Width + 1) + x + 1];

                    if (z == 64 && x == 64)
                    {
                        Debug.Log($"h1: {h1} h2: {h2} h3: {h3} h4: {h4}");
                    }

                    float avgHeight = (h1 + h2 + h3 + h4) * 0.25f;

                    // Assign to your data structure
                    mapData.SetTile(x, z, avgHeight);
                }
            }
        }

        private void GenerateTriangles(MapConfig mapConfig)
        {
            int width = mapConfig.Width;
            int height = mapConfig.Height;

            int[] triangles = new int[width * height * 6];
            int tris = 0;
            int vert = 0;
            for (int z = 0; z < height; z++)
            {
                for (int x = 0; x < width; x++)
                {
                    triangles[tris + 0] = vert + 0;
                    triangles[tris + 1] = vert + width + 1;
                    triangles[tris + 2] = vert + 1;
                    triangles[tris + 3] = vert + 1;
                    triangles[tris + 4] = vert + width + 1;
                    triangles[tris + 5] = vert + width + 2;
                    vert++;
                    tris += 6;
                }
                vert++;
            }

            _mesh.vertices = _vertices;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals(); // Essential for lighting.
            GetComponent<MeshFilter>().mesh = _mesh;
            GetComponent<MeshCollider>().sharedMesh = _mesh;
        }
    }
}
