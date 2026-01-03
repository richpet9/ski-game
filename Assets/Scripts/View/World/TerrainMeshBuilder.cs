using SkiGame.Model.Data;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.View.World
{
    public static class TerrainMeshBuilder
    {
        public static MeshData Build(Map map, int width, int height)
        {
            int vertexCount = (width + 1) * (height + 1);
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];
            int[] triangles = new int[width * height * 6];

            for (int z = 0; z <= height; z++)
            {
                for (int x = 0; x <= width; x++)
                {
                    int index = z * (width + 1) + x;
                    int count = 0;
                    float heightSum = 0;
                    bool isPiste = false;

                    for (int dx = -1; dx <= 0; dx++)
                    {
                        for (int dz = -1; dz <= 0; dz++)
                        {
                            int sampleX = x + dx;
                            int sampleZ = z + dz;

                            if (map.InBounds(sampleX, sampleZ))
                            {
                                TileData t = map.GetTile(sampleX, sampleZ);
                                heightSum += t.Height;
                                count++;
                                if (t.Type == TileType.PackedSnow)
                                {
                                    isPiste = true;
                                }
                            }
                        }
                    }

                    float avgHeight = count > 0 ? heightSum / count : 0;

                    vertices[index] = new Vector3(x, avgHeight, z);
                    uvs[index] = new Vector2((float)x / width, (float)z / height);

                    // Colors
                    if (isPiste)
                    {
                        colors[index] = Color.blue;
                    }
                    else if (avgHeight > 10) // Match your Snow Line constant
                    {
                        colors[index] = Color.white;
                    }
                    else
                    {
                        colors[index] = Color.green;
                    }
                }
            }

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
