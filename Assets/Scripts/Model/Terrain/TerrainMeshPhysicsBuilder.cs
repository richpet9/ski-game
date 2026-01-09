using SkiGame.Model.Data;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.View.World
{
    public static class TerrainMeshPhysicsBuilder
    {
        public static MeshData Build(Map map, int startX, int startZ, int chunkSize)
        {
            // We need vertices covering the gap to the next chunk, so +1.
            int widthVertices = chunkSize + 1;
            int heightVertices = chunkSize + 1;

            int vertexCount = widthVertices * heightVertices;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];
            int[] triangles = new int[chunkSize * chunkSize * 6];

            // 1. Generate Vertices
            for (int z = 0; z <= chunkSize; z++)
            {
                for (int x = 0; x <= chunkSize; x++)
                {
                    int worldX = startX + x;
                    int worldZ = startZ + z;

                    int index = z * widthVertices + x;

                    // Smooth Height Logic
                    int count = 0;
                    float heightSum = 0;

                    for (int dx = -1; dx <= 0; dx++)
                    {
                        for (int dz = -1; dz <= 0; dz++)
                        {
                            int sampleX = worldX + dx;
                            int sampleZ = worldZ + dz;

                            if (map.InBounds(sampleX, sampleZ))
                            {
                                TileData t = map.GetTile(sampleX, sampleZ);
                                heightSum += t.Height;
                                count++;
                            }
                        }
                    }

                    float avgHeight = count > 0 ? heightSum / count : 0;

                    vertices[index] = new Vector3(worldX, avgHeight, worldZ);
                    uvs[index] = new Vector2((float)worldX / map.Width, (float)worldZ / map.Height);
                }
            }

            // 2. Generate Triangles
            int tris = 0;
            int vert = 0;

            for (int z = 0; z < chunkSize; z++)
            {
                for (int x = 0; x < chunkSize; x++)
                {
                    // Indices for the 4 corners of the quad
                    int bl = vert;
                    int br = vert + 1;
                    int tl = vert + widthVertices;
                    int tr = vert + widthVertices + 1;

                    // Triangle 1: Bottom-Left -> Top-Left -> Top-Right
                    triangles[tris + 0] = bl;
                    triangles[tris + 1] = tl;
                    triangles[tris + 2] = tr;

                    // Triangle 2: Bottom-Left -> Top-Right -> Bottom-Right
                    triangles[tris + 3] = bl;
                    triangles[tris + 4] = tr;
                    triangles[tris + 5] = br;

                    vert++;
                    tris += 6;
                }
                // Skip the last vertex in the row (right edge) to jump to start of next row
                vert++;
            }

            return new MeshData
            {
                Vertices = vertices,
                Triangles = triangles,
                UVs = uvs,
                Colors = colors,
            };
        }
    }
}
