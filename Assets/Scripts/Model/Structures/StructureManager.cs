using System;
using System.Collections.Generic;
using SkiGame.Model.Economy;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.Model.Structures
{
    public class StructureManager
    {
        public readonly List<Vector2Int> Lodges = new List<Vector2Int>();
        public readonly List<Vector2Int> ParkingLots = new List<Vector2Int>();
        public readonly List<Vector2Int> Lifts = new List<Vector2Int>();

        public event Action<Vector2Int, StructureType> OnStructureBuilt;
        public event Action<Vector2Int, Vector2Int> OnLiftBuilt;

        private readonly Map _map;
        private readonly EconomyManager _economy;

        public StructureManager(Map map, EconomyManager economy)
        {
            _map = map;
            _economy = economy;
        }

        public int GetCost(StructureType type) =>
            type switch
            {
                StructureType.Lodge => 100,
                StructureType.ParkingLot => 50,
                _ => 0,
            };

        public bool TryBuild(Vector2Int gridPos, StructureType structure)
        {
            if (!_map.InBounds(gridPos))
            {
                return false;
            }

            if (_map.GetTile(gridPos).Structure != StructureType.None)
            {
                Debug.Log("Tile already has a structure!");
                return false;
            }

            int cost = GetCost(structure);
            if (!_economy.TrySpendMoney(cost))
            {
                Debug.Log("Not enough money!");
                return false;
            }

            Build(gridPos, structure);
            OnStructureBuilt?.Invoke(gridPos, structure);
            return true;
        }

        public bool TryBuildLift(Vector2Int startPos, Vector2Int endPos)
        {
            if (!_map.InBounds(startPos) || !_map.InBounds(endPos))
            {
                return false;
            }

            if (_map.GetTile(startPos).Structure != StructureType.None)
            {
                Debug.Log("Start position is occupied!");
                return false;
            }

            if (_map.GetTile(endPos).Structure != StructureType.None)
            {
                Debug.Log("End position is occupied!");
                return false;
            }

            Build(startPos, StructureType.Lift);
            Build(endPos, StructureType.Lift);
            OnLiftBuilt?.Invoke(startPos, endPos);
            return true;
        }

        private void Build(Vector2Int gridPos, StructureType structure)
        {
            _map.SetStructure(gridPos, structure);

            // TODO: Replace these lists with a dictionary, probably. Or a typed getter.
            if (structure == StructureType.Lodge)
            {
                Lodges.Add(gridPos);
            }
            else if (structure == StructureType.ParkingLot)
            {
                ParkingLots.Add(gridPos);
            }
            else if (structure == StructureType.Lift)
            {
                Lifts.Add(gridPos);
            }
        }
    }
}
