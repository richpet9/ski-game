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
        public event Action OnMapChanged;

        public GuestManager Guests { get; private set; }
        public EconomyManager Economy { get; private set; }
        public StructureManager Structures { get; private set; }
        public readonly int Width;
        public readonly int Height;

        private const int SNOW_LINE_HEIGHT = 10;
        private const float FLATTEN_LERP_FACTOR = 0.5f;

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

        public bool InBounds(Vector2Int loc)
        {
            return InBounds(loc.x, loc.y);
        }

        public bool InBounds(int x, int z)
        {
            return x >= 0 && x < Width && z >= 0 && z < Height;
        }

        public void PaintPiste(Vector2Int loc)
        {
            if (InBounds(loc))
            {
                SetTileType(loc, TileType.PackedSnow);
                FlattenTerrain(loc.x, loc.y);

                OnMapChanged?.Invoke();
            }
        }

        private void FlattenTerrain(int x, int z)
        {
            float totalHeight = 0;
            int count = 0;

            for (int nx = x - 1; nx <= x + 1; nx++)
            {
                for (int nz = z - 1; nz <= z + 1; nz++)
                {
                    if (InBounds(nx, nz))
                    {
                        totalHeight += _grid[GetIndex(nx, nz)].Height;
                        count++;
                    }
                }
            }

            if (count > 0)
            {
                float average = totalHeight / count;
                // Apply a gentle lerp towards the average to "pack" it rather than instantly flatten.
                _grid[GetIndex(x, z)].Height = Mathf.Lerp(
                    _grid[GetIndex(x, z)].Height,
                    average,
                    FLATTEN_LERP_FACTOR
                );
            }
        }

        public void SetTile(Vector2Int loc, float height)
        {
            SetTile(loc.x, loc.y, height);
        }

        public void SetTile(int x, int z, float height)
        {
            if (InBounds(x, z))
            {
                _grid[GetIndex(x, z)].Height = height;
                _grid[GetIndex(x, z)].Type = GetTerrainTypeFromHeight(height);
            }
        }

        public void SetTileType(Vector2Int loc, TileType type)
        {
            SetTileType(loc.x, loc.y, type);
        }

        public void SetTileType(int x, int z, TileType type)
        {
            if (InBounds(x, z))
            {
                _grid[GetIndex(x, z)].Type = type;
            }
        }

        public TileData GetTile(Vector2Int loc)
        {
            return GetTile(loc.x, loc.y);
        }

        public TileData GetTile(int x, int z)
        {
            if (InBounds(x, z))
            {
                return _grid[GetIndex(x, z)];
            }
            return default;
        }

        public void SetStructure(Vector2Int loc, StructureType structure)
        {
            SetStructure(loc.x, loc.y, structure);
        }

        public void SetStructure(int x, int z, StructureType structure)
        {
            if (InBounds(x, z))
            {
                _grid[GetIndex(x, z)].Structure = structure;
            }
        }

        private TileType GetTerrainTypeFromHeight(float height)
        {
            // Uses a constant in the shader and here.
            if (height > SNOW_LINE_HEIGHT)
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
