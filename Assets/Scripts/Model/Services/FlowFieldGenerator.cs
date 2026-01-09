using System;
using System.Collections.Generic;
using SkiGame.Model.Data;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.Model.Services
{
    public static class FlowFieldGenerator
    {
        public static Vector2[] Generate(Map map)
        {
            Vector2[] field = new Vector2[map.Width * map.Height];
            Queue<Vector2Int> goals = new Queue<Vector2Int>();
            float[] cost = new float[map.Width * map.Height];

            // Initialize costs to maximum.
            for (int i = 0; i < cost.Length; i++)
            {
                cost[i] = float.MaxValue;
            }

            // Identify Lift goals.
            foreach (StructureManager.Lift lift in map.Structures.Lifts)
            {
                int index = map.GetIndex(lift.StartGrid.x, lift.StartGrid.y);
                cost[index] = 0f;
                goals.Enqueue(lift.StartGrid);
            }

            // Identify Lodge goals.
            if (
                map.Structures.Structures.TryGetValue(
                    StructureType.Lodge,
                    out List<Vector2Int> lodges
                )
            )
            {
                foreach (Vector2Int lodgePos in lodges)
                {
                    int index = map.GetIndex(lodgePos.x, lodgePos.y);
                    cost[index] = 0f;
                    goals.Enqueue(lodgePos);
                }
            }

            // Dijkstra Flood-Fill to generate the Cost Field.
            while (goals.Count > 0)
            {
                Vector2Int current = goals.Dequeue();
                float currentCost = cost[map.GetIndex(current.x, current.y)];

                for (int nz = -1; nz <= 1; nz++)
                {
                    for (int nx = -1; nx <= 1; nx++)
                    {
                        if (nx == 0 && nz == 0)
                        {
                            continue;
                        }

                        Vector2Int neighbor = new Vector2Int(current.x + nx, current.y + nz);

                        if (map.InBounds(neighbor))
                        {
                            TileData neighborTile = map.GetTile(neighbor);

                            // Only propagate costs through packed snow.
                            if (neighborTile.Type == TileType.PackedSnow)
                            {
                                float moveCost = (Mathf.Abs(nx) + Mathf.Abs(nz) > 1) ? 1.414f : 1f;
                                float newCost = currentCost + moveCost;

                                int nIndex = map.GetIndex(neighbor.x, neighbor.y);
                                if (newCost < cost[nIndex])
                                {
                                    cost[nIndex] = newCost;
                                    goals.Enqueue(neighbor);
                                }
                            }
                        }
                    }
                }
            }

            // Generate Flow Vectors based on the lowest neighbor cost.
            for (int z = 0; z < map.Height; z++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    int index = map.GetIndex(x, z);
                    TileData tile = map.GetTile(x, z);

                    if (tile.Type == TileType.PackedSnow)
                    {
                        float minCost = cost[index];
                        Vector2 bestDir = Vector2.zero;

                        for (int nz = -1; nz <= 1; nz++)
                        {
                            for (int nx = -1; nx <= 1; nx++)
                            {
                                Vector2Int neighbor = new Vector2Int(x + nx, z + nz);
                                if (map.InBounds(neighbor))
                                {
                                    float nCost = cost[map.GetIndex(neighbor.x, neighbor.y)];
                                    if (nCost < minCost)
                                    {
                                        minCost = nCost;
                                        bestDir = new Vector2(nx, nz);
                                    }
                                }
                            }
                        }
                        field[index] = bestDir.normalized;
                    }
                    else
                    {
                        field[index] = Vector2.zero;
                    }
                }
            }

            return field;
        }
    }
}
