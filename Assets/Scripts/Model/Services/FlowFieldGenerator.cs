// Assets/Scripts/Model/AI/FlowFieldGenerator.cs
using System.Collections.Generic;
using System.Linq;
using SkiGame.Model.Data;
using SkiGame.Model.Services;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.Model.AI
{
    public static class FlowFieldGenerator
    {
        public static float[,] Generate(Map map, NavigationGoal goal)
        {
            float[,] distances = new float[map.Width, map.Height];
            for (int x = 0; x < map.Width; x++)
            {
                for (int z = 0; z < map.Height; z++)
                {
                    distances[x, z] = float.MaxValue;
                }
            }

            Queue<Vector2Int> frontier = new Queue<Vector2Int>();
            List<Vector2Int> goalTiles = GetGoalTiles(map, goal);

            foreach (Vector2Int tile in goalTiles)
            {
                distances[tile.x, tile.y] = 0;
                frontier.Enqueue(tile);
            }

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                float currentDist = distances[current.x, current.y];

                foreach (Vector2Int neighbor in GetNeighbors(current))
                {
                    if (!map.InBounds(neighbor))
                    {
                        continue;
                    }

                    float moveCost = CalculateCost(map, current, neighbor);
                    float newDist = currentDist + moveCost;

                    if (newDist < distances[neighbor.x, neighbor.y])
                    {
                        distances[neighbor.x, neighbor.y] = newDist;
                        frontier.Enqueue(neighbor);
                    }
                }
            }

            return distances;
        }

        private static float CalculateCost(Map map, Vector2Int from, Vector2Int to)
        {
            TileData toTile = map.GetTile(to);
            float cost = 1.0f;

            // Pistes are faster/preferred.
            if (toTile.Type == TileType.PackedSnow)
            {
                cost *= 0.5f;
            }

            // Trees and structures act as obstacles.
            if (toTile.Structure != StructureType.None && toTile.Structure != StructureType.Tree)
            {
                cost += 10.0f;
            }

            return cost;
        }

        private static List<Vector2Int> GetGoalTiles(Map map, NavigationGoal goal)
        {
            List<Vector2Int> tiles = new List<Vector2Int>();
            switch (goal)
            {
                case NavigationGoal.Lodge:
                    map.Structures.Structures[StructureType.Lodge].ForEach(tile => tiles.Add(tile));
                    break;

                case NavigationGoal.LiftEntrance:
                    map.Structures.Lifts.ForEach(lift => tiles.Add(lift.StartGrid));
                    break;

                case NavigationGoal.ParkingLot:
                    map.Structures.Structures[StructureType.ParkingLot]
                        .ForEach(tile => tiles.Add(tile));
                    break;

                case NavigationGoal.None:
                case NavigationGoal.Wander:
                default:
                    // No specific goal tiles for other NavigationGoals.
                    break;
            }

            return tiles;
        }

        private static IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            yield return new Vector2Int(pos.x + 1, pos.y);
            yield return new Vector2Int(pos.x - 1, pos.y);
            yield return new Vector2Int(pos.x, pos.y + 1);
            yield return new Vector2Int(pos.x, pos.y - 1);
        }
    }
}
