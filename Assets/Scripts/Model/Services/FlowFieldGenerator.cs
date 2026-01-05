using SkiGame.Model.Data;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.Model.Services
{
    public static class FlowFieldGenerator
    {
        public static Vector2[,] Generate(Map map)
        {
            Vector2[,] field = new Vector2[map.Width, map.Height];

            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    TileData currentTile = map.GetTile(x, z);

                    // 1. If not a piste, no flow (agents use other navigation).
                    if (currentTile.Type != TileType.PackedSnow)
                    {
                        field[x, z] = Vector2.zero;
                        continue;
                    }

                    float lowestHeight = currentTile.Height;
                    Vector2Int bestTarget = new Vector2Int(x, z);
                    bool foundSlope = false;

                    for (int nx = x - 1; nx <= x + 1; nx++)
                    {
                        for (int nz = z - 1; nz <= z + 1; nz++)
                        {
                            if (nx == x && nz == z)
                            {
                                continue;
                            }

                            if (!map.InBounds(nx, nz))
                            {
                                continue;
                            }

                            TileData neighbor = map.GetTile(nx, nz);

                            // Skiers prefer Piste, but will ski onto snow if forced
                            // (optional) For now, strict Piste following.
                            if (neighbor.Type != TileType.PackedSnow)
                            {
                                continue;
                            }

                            // Use a small epsilon to prefer movement even on slight
                            // grades.
                            if (neighbor.Height < lowestHeight - 0.01f)
                            {
                                lowestHeight = neighbor.Height;
                                bestTarget = new Vector2Int(nx, nz);
                                foundSlope = true;
                            }
                        }
                    }

                    if (foundSlope)
                    {
                        Vector2 dir = new Vector2(bestTarget.x - x, bestTarget.y - z);
                        field[x, z] = dir.normalized;
                    }
                    else
                    {
                        // Flat area or local minimum:
                        // In a robust system, we'd flood-fill distance to base.
                        // For Step 1, we just stop (or maintain momentum in Agent).
                        field[x, z] = Vector2.zero;
                    }
                }
            }

            return field;
        }
    }
}
