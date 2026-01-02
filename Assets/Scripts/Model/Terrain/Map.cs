using System;
using SkiGame.Model.Agents;
using SkiGame.Model.Data;
using SkiGame.Model.Economy;
using SkiGame.Model.Structures;
using UnityEngine;

namespace SkiGame.Model.Terrain
{
    public class Map
    {
        public GuestManager Guests { get; private set; }
        public EconomyManager Economy { get; private set; }
        public StructureManager Structures { get; private set; }
        public readonly int Width;
        public readonly int Height;

        private readonly TileData[] _grid;

        public Map(int width, int height)
        {
            Width = width;
            Height = height;

            _grid = new TileData[width * height];

            Guests = new GuestManager();
            Economy = new EconomyManager(300);
            Structures = new StructureManager(this, Economy);
        }

        private int GetIndex(int x, int z)
        {
            return x + z * Width;
        }

        public void SetTile(Vector2Int loc, float height)
        {
            SetTile(loc.x, loc.y, height);
        }

        public void SetTile(int x, int z, float height)
        {
            if (!InBounds(x, z))
            {
                Debug.LogError($"Arrempting access out of bounds map index: {x}, {z}");
                return;
            }

            _grid[GetIndex(x, z)].Height = height;
            _grid[GetIndex(x, z)].Type = GetTerrainTypeFromHeight(height);
        }

        public TileData GetTile(Vector2Int loc)
        {
            return GetTile(loc.x, loc.y);
        }

        public TileData GetTile(int x, int z)
        {
            if (!InBounds(x, z))
            {
                Debug.LogError($"Arrempting access out of bounds map index: {x}, {z}");
                return new TileData();
            }

            return _grid[GetIndex(x, z)];
        }

        public void SetStructure(Vector2Int loc, StructureType structure)
        {
            SetStructure(loc.x, loc.y, structure);
        }

        public void SetStructure(int x, int z, StructureType structure)
        {
            if (!InBounds(x, z))
            {
                Debug.LogError($"Arrempting access out of bounds map index: {x}, {z}");
                return;
            }

            _grid[GetIndex(x, z)].Structure = structure;
        }

        public bool InBounds(Vector2Int loc)
        {
            return InBounds(loc.x, loc.y);
        }

        public bool InBounds(int x, int z)
        {
            return GetIndex(x, z) >= 0 && GetIndex(x, z) < _grid.Length;
        }

        private TileType GetTerrainTypeFromHeight(float height)
        {
            // Use a constant in the shader and here.
            if (height > 10)
            {
                return TileType.Snow;
            }
            else
            {
                return TileType.Grass;
            }
        }

        public void ApplyTrees(bool[] trees)
        {
            for (int i = 0; i < trees.Length; i++)
            {
                if (trees[i])
                {
                    // Only place a tree if the spot is empty.
                    if (_grid[i].Structure == StructureType.None)
                    {
                        _grid[i].Structure = StructureType.Tree;
                    }
                }
            }
        }
    }
}
