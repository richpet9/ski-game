using UnityEngine;

namespace SkiGame.Model.Terrain
{
    public static class TreesGenerator
    {
        public static bool[] GenerateTrees(
            int width,
            int height,
            float[] heightMap,
            float maxMountainHeight,
            int seed,
            float density,
            int noiseScale,
            float treeLinePercent
        )
        {
            bool[] trees = new bool[width * height];
            float xOffset = seed + 5555f;
            float zOffset = seed + 5555f;

            for (int i = 0; i < trees.Length; i++)
            {
                int x = i % width;
                int z = i / width;
                float currentHeight = heightMap[(z * (width + 1)) + x];

                if (currentHeight > maxMountainHeight * treeLinePercent)
                {
                    continue;
                }

                if (currentHeight < 1f)
                {
                    continue;
                }

                float xCoord = (float)x / width * noiseScale + xOffset;
                float zCoord = (float)z / height * noiseScale + zOffset;
                float noise = Mathf.PerlinNoise(xCoord, zCoord);

                if (noise < density)
                {
                    trees[i] = true;
                }
            }

            return trees;
        }
    }
}
