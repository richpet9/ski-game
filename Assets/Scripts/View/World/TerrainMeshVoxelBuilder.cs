using System.Collections.Generic;
using SkiGame.Model.Data;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.View.World
{
    public static class TerrainMeshVoxelBuilder
    {
        private struct VoxelInfo
        {
            public float Height;
            public Color Color;
        }

        private const int VOXELS_PER_UNIT = 2;
        private const float VOXEL_SIZE = 1f / VOXELS_PER_UNIT;
        private const float NOISE_AMPLITUDE = 1f;
        private const float BOTTOM_HEIGHT = -10f;

        public static MeshData Build(Map map, int startX, int startZ, int chunkSize)
        {
            // 1. Setup Dimensions.
            int startVx = startX * VOXELS_PER_UNIT;
            int startVz = startZ * VOXELS_PER_UNIT;
            int voxelsPerChunk = chunkSize * VOXELS_PER_UNIT;

            // We need a 1-voxel buffer on all sides for neighbor checks.
            // Local array size: [0 .. voxelsPerChunk + 1] inclusive.
            int cacheWidth = voxelsPerChunk + 2;
            VoxelInfo[] voxelCache = new VoxelInfo[cacheWidth * cacheWidth];

            // 2. Fill Cache (Only fetch Map Data & Noise ONCE per voxel)
            // We iterate from -1 to voxelsPerChunk to cover the skirts.
            for (int z = -1; z <= voxelsPerChunk; z++)
            {
                for (int x = -1; x <= voxelsPerChunk; x++)
                {
                    int vx = startVx + x;
                    int vz = startVz + z;

                    // Store in 1D array for slight perf boost over 2D
                    int index = (z + 1) * cacheWidth + (x + 1);
                    voxelCache[index] = GetVoxelData(map, vx, vz);
                }
            }

            // 3. Pre-allocate Lists.
            // Estimate: Voxels * ~20 verts (1 top + 4 skirts worst case).
            int estimatedVerts = voxelsPerChunk * voxelsPerChunk * 20;
            List<Vector3> vertices = new List<Vector3>(estimatedVerts);
            List<int> triangles = new List<int>(estimatedVerts); // Indices count approx same as verts.
            List<Color> colors = new List<Color>(estimatedVerts);
            List<Vector2> uvs = new List<Vector2>(estimatedVerts);
            List<Vector3> normals = new List<Vector3>(estimatedVerts);

            float halfScale = VOXEL_SIZE * 0.5f;

            // 4. Build Mesh using Cache.
            for (int z = 0; z < voxelsPerChunk; z++)
            {
                for (int x = 0; x < voxelsPerChunk; x++)
                {
                    // Cache indices.
                    int currentIdx = (z + 1) * cacheWidth + (x + 1);
                    VoxelInfo current = voxelCache[currentIdx];

                    // Position.
                    float centerX = ((startVx + x) * VOXEL_SIZE) + halfScale;
                    float centerZ = ((startVz + z) * VOXEL_SIZE) + halfScale;
                    Vector3 center = new Vector3(centerX, current.Height, centerZ);

                    // A. Top Face.
                    AddQuad(
                        vertices,
                        triangles,
                        uvs,
                        colors,
                        normals,
                        center,
                        Vector3.right * halfScale,
                        Vector3.forward * halfScale,
                        current.Color,
                        Vector3.up // Manual Normal.
                    );

                    // B. Neighbors (Read from Cache).
                    // North (z+1).
                    CheckAndAddSkirt(
                        voxelCache[(z + 2) * cacheWidth + (x + 1)], // z+1 -> cache z+2.
                        current.Height,
                        new Vector3(centerX, 0, centerZ + halfScale),
                        Vector3.left * halfScale,
                        current.Color,
                        Vector3.forward, // Normal: Forward.
                        vertices,
                        triangles,
                        uvs,
                        colors,
                        normals
                    );

                    // South (z-1).
                    CheckAndAddSkirt(
                        voxelCache[z * cacheWidth + (x + 1)], // z-1 -> cache z.
                        current.Height,
                        new Vector3(centerX, 0, centerZ - halfScale),
                        Vector3.right * halfScale,
                        current.Color,
                        Vector3.back, // Normal: Back.
                        vertices,
                        triangles,
                        uvs,
                        colors,
                        normals
                    );

                    // East (x+1).
                    CheckAndAddSkirt(
                        voxelCache[(z + 1) * cacheWidth + (x + 2)], // x+1 -> cache x+2.
                        current.Height,
                        new Vector3(centerX + halfScale, 0, centerZ),
                        Vector3.forward * halfScale,
                        current.Color,
                        Vector3.right, // Normal: Right.
                        vertices,
                        triangles,
                        uvs,
                        colors,
                        normals
                    );

                    // West (x-1).
                    CheckAndAddSkirt(
                        voxelCache[(z + 1) * cacheWidth + x], // x-1 -> cache x.
                        current.Height,
                        new Vector3(centerX - halfScale, 0, centerZ),
                        Vector3.back * halfScale,
                        current.Color,
                        Vector3.left, // Normal: Left.
                        vertices,
                        triangles,
                        uvs,
                        colors,
                        normals
                    );
                }
            }

            return new MeshData
            {
                Vertices = vertices.ToArray(),
                Triangles = triangles.ToArray(),
                Colors = colors.ToArray(),
                Normals = normals.ToArray(),
                UVs = uvs.ToArray(),
            };
        }

        private static VoxelInfo GetVoxelData(Map map, int vx, int vz)
        {
            int mapX = Mathf.FloorToInt((float)vx / VOXELS_PER_UNIT);
            int mapZ = Mathf.FloorToInt((float)vz / VOXELS_PER_UNIT);

            if (!map.InBounds(mapX, mapZ))
            {
                return new VoxelInfo { Height = BOTTOM_HEIGHT, Color = Color.black };
            }

            TileData tile = map.GetTile(mapX, mapZ);

            // Noise.
            float noise = Mathf.Abs(vx * 73856093 ^ vz * 19349663) % 100 / 100f;
            float heightOffset = (noise - 0.5f) * NOISE_AMPLITUDE;
            Color c = GetColor(tile);

            return new VoxelInfo { Height = tile.Height + heightOffset, Color = c };
        }

        private static Color GetColor(TileData tile)
        {
            Color c;
            if (tile.Type == TileType.PackedSnow)
            {
                c = Color.blue;
            }
            else
            {
                // TODO: Replace with constant.
                c = tile.Height > 55 ? Color.white : Color.green;
            }

            return c;
        }

        private static void CheckAndAddSkirt(
            VoxelInfo neighbor,
            float currentHeight,
            Vector3 wallCenterXZ,
            Vector3 widthAxis,
            Color color,
            Vector3 normal,
            List<Vector3> verts,
            List<int> tris,
            List<Vector2> uvs,
            List<Color> cols,
            List<Vector3> norms
        )
        {
            if (currentHeight > neighbor.Height)
            {
                float faceHeight = currentHeight - neighbor.Height;
                float midY = neighbor.Height + (faceHeight * 0.5f);
                Vector3 center = new Vector3(wallCenterXZ.x, midY, wallCenterXZ.z);

                AddQuad(
                    verts,
                    tris,
                    uvs,
                    cols,
                    norms,
                    center,
                    widthAxis,
                    Vector3.up * (faceHeight * 0.5f),
                    color,
                    normal
                );
            }
        }

        private static void AddQuad(
            List<Vector3> verts,
            List<int> tris,
            List<Vector2> uvs,
            List<Color> cols,
            List<Vector3> norms,
            Vector3 center,
            Vector3 widthAxis,
            Vector3 heightAxis,
            Color color,
            Vector3 normal
        )
        {
            int startIndex = verts.Count;

            verts.Add(center - widthAxis - heightAxis);
            verts.Add(center - widthAxis + heightAxis);
            verts.Add(center + widthAxis + heightAxis);
            verts.Add(center + widthAxis - heightAxis);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));

            cols.Add(color);
            cols.Add(color);
            cols.Add(color);
            cols.Add(color);

            // Add Normals manually (4 times, one per vertex).
            norms.Add(normal);
            norms.Add(normal);
            norms.Add(normal);
            norms.Add(normal);

            tris.Add(startIndex + 0);
            tris.Add(startIndex + 1);
            tris.Add(startIndex + 2);
            tris.Add(startIndex + 0);
            tris.Add(startIndex + 2);
            tris.Add(startIndex + 3);
        }
    }
}
