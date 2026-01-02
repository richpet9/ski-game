using UnityEngine;

namespace SkiGame.Model.Terrain
{
    public static class MountainGenerator
    {
        // Pure Logic: Returns the height map data, doesn't touch MeshFilters
        public static float[] GenerateHeights(
            int width,
            int height,
            int seed,
            float noiseScale,
            float noiseIntensity,
            float mountainHeight,
            AnimationCurve curve
        )
        {
            float[] heights = new float[(width + 1) * (height + 1)];
            Vector2 center = new Vector2(width / 2f, height / 2f);
            float xOffset = seed + 9999f;
            float zOffset = seed + 9999f;

            for (int z = 0; z <= height; z++)
            {
                for (int x = 0; x <= width; x++)
                {
                    float xCoord = (float)x / width * noiseScale + xOffset;
                    float zCoord = (float)z / height * noiseScale + zOffset;
                    float distFromCenter = Vector2.Distance(new Vector2(x, z), center);

                    float mask = 1f - Mathf.Clamp01(distFromCenter / (width / 2f));
                    float noise =
                        (Mathf.PerlinNoise(xCoord, zCoord) - 0.5f) * noiseIntensity * mask;

                    float heightValue = (curve.Evaluate(mask) + noise) * mountainHeight;
                    heights[z * (width + 1) + x] = heightValue;
                }
            }
            return heights;
        }

        // Helper to generate mesh data arrays (Vertices/Indices) purely in memory
        public static MeshData GenerateMeshData(int width, int height, float[] heights)
        {
            int[] triangles = new int[width * height * 6];
            Vector3[] vertices = new Vector3[heights.Length];
            Vector2[] uvs = new Vector2[vertices.Length];
            int tris = 0;
            int vert = 0;

            for (int i = 0; i < heights.Length; i++)
            {
                int x = i % (width + 1);
                int z = i / (width + 1);
                vertices[i] = new Vector3(x, heights[i], z);
                uvs[i] = new Vector2((float)x / width, (float)z / height);
            }

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

            return new MeshData()
            {
                Vertices = vertices,
                Triangles = triangles,
                UVs = uvs,
            };
        }
    }

    public struct MeshData
    {
        public int[] Triangles;
        public Vector3[] Vertices;
        public Vector2[] UVs;
    }
}
