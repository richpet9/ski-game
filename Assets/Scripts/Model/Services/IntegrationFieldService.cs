// Assets/Scripts/Model/AI/IntegrationFieldService.cs
using System.Collections.Generic;
using SkiGame.Model.AI;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.Model.Services
{
    public enum NavigationGoal : byte
    {
        None = 0,
        Lodge = 1,
        LiftEntrance = 2,
        ParkingLot = 3,
        Wander = 4,
    }

    public sealed class IntegrationFieldService
    {
        private readonly Map _map;
        private readonly Dictionary<NavigationGoal, float[,]> _distanceFields;

        public IntegrationFieldService(Map map)
        {
            _map = map;
            _distanceFields = new Dictionary<NavigationGoal, float[,]>();
            _map.OnMapChanged += RequestRecalculation;
        }

        public void RequestRecalculation()
        {
            Debug.Log("Recalculating distance fields...");
            // In a production scenario, this should be staggered or threaded.
            _distanceFields[NavigationGoal.LiftEntrance] = FlowFieldGenerator.Generate(
                _map,
                NavigationGoal.LiftEntrance
            );
            _distanceFields[NavigationGoal.Lodge] = FlowFieldGenerator.Generate(
                _map,
                NavigationGoal.Lodge
            );
            _distanceFields[NavigationGoal.ParkingLot] = FlowFieldGenerator.Generate(
                _map,
                NavigationGoal.ParkingLot
            );
        }

        public Vector2 GetDirection(Vector3 worldPos, NavigationGoal goal)
        {
            if (!_distanceFields.ContainsKey(goal))
            {
                return Vector2.zero;
            }

            Vector2Int gridPos = MapUtil.WorldToGrid(worldPos);
            float[,] field = _distanceFields[goal];

            float minDistance = field[gridPos.x, gridPos.y];
            Vector2Int bestNeighbor = gridPos;

            // Check 8 neighbors to find the steepest descent in the distance field.
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector2Int neighbor = new Vector2Int(gridPos.x + x, gridPos.y + z);
                    if (_map.InBounds(neighbor) && field[neighbor.x, neighbor.y] < minDistance)
                    {
                        minDistance = field[neighbor.x, neighbor.y];
                        bestNeighbor = neighbor;
                    }
                }
            }

            if (bestNeighbor == gridPos)
            {
                return Vector2.zero;
            }

            Vector2 direction = new Vector2(bestNeighbor.x - gridPos.x, bestNeighbor.y - gridPos.y);
            return direction.normalized;
        }
    }
}
