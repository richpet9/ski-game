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
                    int i = z * (width + 1) + x;

                    // Clamping to map bounds for vertex height retrieval.
                    int mapX = Mathf.Min(x, width - 1);
                    int mapZ = Mathf.Min(z, height - 1);
                    TileData tile = map.GetTile(mapX, mapZ);

                    vertices[i] = new Vector3(x, tile.Height, z);
                    uvs[i] = new Vector2((float)x / width, (float)z / height);

                    // Color encoding for Shader.
                    if (tile.Type == TileType.PackedSnow)
                    {
                        colors[i] = new Color(1f, 1f, 0f, 1f); // White/Yellow for Piste.
                    }
                    else if (tile.Type == TileType.Snow)
                    {
                        colors[i] = new Color(1f, 0f, 0f, 1f); // White for Snow.
                    }
                    else
                    {
                        colors[i] = new Color(0f, 0f, 0f, 1f); // Black for Grass.
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
