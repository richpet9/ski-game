using System;
using System.Collections.Generic;
using SkiGame.Model.Economy;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.Model.Structures
{
    public class StructureManager
    {
        public struct Lift
        {
            public Vector2Int StartGrid;
            public Vector2Int EndGrid;
        }

        public readonly List<Vector2Int> Lodges = new List<Vector2Int>();
        public readonly List<Vector2Int> ParkingLots = new List<Vector2Int>();
        public readonly List<Lift> Lifts = new List<Lift>();

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
                StructureType.Lift => 25,
                _ => 0,
            };

        public (bool, string) TryBuild(Vector2Int gridPos, StructureType structure)
        {
            if (!_map.InBounds(gridPos))
            {
                return (false, "Position out of bounds");
            }

            if (_map.GetTile(gridPos).Structure != StructureType.None)
            {
                return (false, "Tile already has a structure");
            }

            int cost = GetCost(structure);
            if (!_economy.TrySpendMoney(cost))
            {
                return (false, "Not enough money");
            }

            Build(gridPos, structure);
            OnStructureBuilt?.Invoke(gridPos, structure);
            return (true, null);
        }

        public (bool, string) TryBuildLift(Vector2Int startPos, Vector2Int endPos)
        {
            if (!_map.InBounds(startPos) || !_map.InBounds(endPos))
            {
                return (false, "Position out of bounds");
            }

            if (_map.GetTile(startPos).Structure != StructureType.None)
            {
                return (false, "Start position is occupied");
            }

            if (_map.GetTile(endPos).Structure != StructureType.None)
            {
                return (false, "End position is occupied");
            }

            // TODO: Base cost on distance.
            int cost = GetCost(StructureType.Lift) * 2; // There are two lift towers.
            if (!_economy.TrySpendMoney(cost))
            {
                return (false, "Not enough money");
            }

            Build(startPos, StructureType.Lift);
            Build(endPos, StructureType.Lift);
            OnLiftBuilt?.Invoke(startPos, endPos);
            Lifts.Add(new Lift { StartGrid = startPos, EndGrid = endPos });
            return (true, null);
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
        }
    }
}
