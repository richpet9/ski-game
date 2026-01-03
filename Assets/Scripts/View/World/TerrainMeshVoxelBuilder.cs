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

        // 2 voxels per map unit = 4x density.
        private const int VOXELS_PER_UNIT = 1;
        private const float VOXEL_SIZE = 1f / VOXELS_PER_UNIT;
        private const float NOISE_AMPLITUDE = 0.5f;
        private const float BOTTOM_HEIGHT = -10f;

        public static MeshData Build(Map map, int startX, int startZ, int chunkSize)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Color> colors = new List<Color>();
            List<Vector2> uvs = new List<Vector2>();

            float halfScale = VOXEL_SIZE * 0.5f;

            // Calculate voxel ranges for this chunk.
            int startVx = startX * VOXELS_PER_UNIT;
            int startVz = startZ * VOXELS_PER_UNIT;
            int endVx = (startX + chunkSize) * VOXELS_PER_UNIT;
            int endVz = (startZ + chunkSize) * VOXELS_PER_UNIT;

            for (int vz = startVz; vz < endVz; vz++)
            {
                for (int vx = startVx; vx < endVx; vx++)
                {
                    // Get data for the current voxel (Height + Color).
                    VoxelInfo current = GetVoxelData(map, vx, vz);

                    // World Position of the voxel center.
                    float centerX = (vx * VOXEL_SIZE) + halfScale;
                    float centerZ = (vz * VOXEL_SIZE) + halfScale;

                    Vector3 center = new Vector3(centerX, current.Height, centerZ);

                    // 1. Top Face.
                    AddQuad(
                        vertices,
                        triangles,
                        uvs,
                        colors,
                        center,
                        widthAxis: Vector3.right * halfScale,
                        heightAxis: Vector3.forward * halfScale,
                        color: current.Color
                    );

                    // 2. Check Neighbors and Add Skirts.
                    // We now check VOXEL neighbors, not Map neighbors.

                    // North (vz + 1).
                    CheckAndAddSkirt(
                        map,
                        current.Height,
                        neighborVx: vx,
                        neighborVz: vz + 1,
                        wallCenterXZ: new Vector3(centerX, 0, centerZ + halfScale),
                        widthAxis: Vector3.left * halfScale,
                        color: current.Color,
                        vertices,
                        triangles,
                        uvs,
                        colors
                    );

                    // South (vz - 1).
                    CheckAndAddSkirt(
                        map,
                        current.Height,
                        neighborVx: vx,
                        neighborVz: vz - 1,
                        wallCenterXZ: new Vector3(centerX, 0, centerZ - halfScale),
                        widthAxis: Vector3.right * halfScale,
                        color: current.Color,
                        vertices,
                        triangles,
                        uvs,
                        colors
                    );

                    // East (vx + 1).
                    CheckAndAddSkirt(
                        map,
                        current.Height,
                        neighborVx: vx + 1,
                        neighborVz: vz,
                        wallCenterXZ: new Vector3(centerX + halfScale, 0, centerZ),
                        widthAxis: Vector3.forward * halfScale,
                        color: current.Color,
                        vertices,
                        triangles,
                        uvs,
                        colors
                    );

                    // West (vx - 1).
                    CheckAndAddSkirt(
                        map,
                        current.Height,
                        neighborVx: vx - 1,
                        neighborVz: vz,
                        wallCenterXZ: new Vector3(centerX - halfScale, 0, centerZ),
                        widthAxis: Vector3.back * halfScale,
                        color: current.Color,
                        vertices,
                        triangles,
                        uvs,
                        colors
                    );
                }
            }

            return new MeshData
            {
                Vertices = vertices.ToArray(),
                Triangles = triangles.ToArray(),
                Colors = colors.ToArray(),
                UVs = uvs.ToArray(),
            };
        }

        private static VoxelInfo GetVoxelData(Map map, int vx, int vz)
        {
            // Map voxel coordinate back to map coordinate.
            int mapX = vx / VOXELS_PER_UNIT;
            int mapZ = vz / VOXELS_PER_UNIT;

            // Boundary check.
            if (!map.InBounds(mapX, mapZ))
            {
                return new VoxelInfo { Height = BOTTOM_HEIGHT, Color = Color.black };
            }

            TileData tile = map.GetTile(mapX, mapZ);

            // Deterministic Noise (Pseudo-Random Hash based on coordinates).
            // This ensures the noise is static and doesn't flicker when rebuilding mesh.
            // Using primes to scatter the bits.
            float noise = (vx * 73856093 ^ vz * 19349663) % 100 / 100f;
            // Map 0..1 to -Amplitude..+Amplitude.
            float heightOffset = (noise - 0.5f) * NOISE_AMPLITUDE;

            Color c;
            if (tile.Type == TileType.PackedSnow)
            {
                c = Color.blue;
            }
            else
            {
                c = tile.Height > 10 ? Color.white : Color.green;
            }

            return new VoxelInfo { Height = tile.Height + heightOffset, Color = c };
        }

        private static void CheckAndAddSkirt(
            Map map,
            float currentHeight,
            int neighborVx,
            int neighborVz,
            Vector3 wallCenterXZ,
            Vector3 widthAxis,
            Color color,
            List<Vector3> verts,
            List<int> tris,
            List<Vector2> uvs,
            List<Color> cols
        )
        {
            // Get height of the neighboring voxel.
            float neighborHeight = GetVoxelData(map, neighborVx, neighborVz).Height;

            // Only draw if we are taller than the neighbor.
            if (currentHeight > neighborHeight)
            {
                float faceHeight = currentHeight - neighborHeight;
                float midY = neighborHeight + (faceHeight * 0.5f);
                Vector3 center = new Vector3(wallCenterXZ.x, midY, wallCenterXZ.z);

                AddQuad(
                    verts,
                    tris,
                    uvs,
                    cols,
                    center,
                    widthAxis,
                    Vector3.up * (faceHeight * 0.5f),
                    color
                );
            }
        }

        private static void AddQuad(
            List<Vector3> verts,
            List<int> tris,
            List<Vector2> uvs,
            List<Color> cols,
            Vector3 center,
            Vector3 widthAxis,
            Vector3 heightAxis,
            Color color
        )
        {
            int startIndex = verts.Count;

            // P0: Bottom-Left.
            verts.Add(center - widthAxis - heightAxis);
            // P1: Top-Left.
            verts.Add(center - widthAxis + heightAxis);
            // P2: Top-Right.
            verts.Add(center + widthAxis + heightAxis);
            // P3: Bottom-Right.
            verts.Add(center + widthAxis - heightAxis);

            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(1, 0));

            cols.Add(color);
            cols.Add(color);
            cols.Add(color);
            cols.Add(color);

            // Clockwise Winding.
            tris.Add(startIndex + 0);
            tris.Add(startIndex + 1);
            tris.Add(startIndex + 2);
            tris.Add(startIndex + 0);
            tris.Add(startIndex + 2);
            tris.Add(startIndex + 3);
        }
    }
}
