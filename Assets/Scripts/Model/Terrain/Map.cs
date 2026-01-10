using System;
using SkiGame.Model.Agents;
using SkiGame.Model.Data;
using SkiGame.Model.Economy;
using SkiGame.Model.Structures;
using UnityEngine;

namespace SkiGame.Model.Terrain
{
    public sealed class Map
    {
        public event Action OnMapChange;
        public event Action OnFoliageChange;

        public GuestManager Guests { get; private set; }
        public EconomyManager Economy { get; private set; }
        public StructureManager Structures { get; private set; }
        public readonly int Width;
        public readonly int Height;

        private const float PISTE_RADIUS = 1.8f;
        private const float PISTE_HEIGHT_LERP = 0.2f;
        private const float PISTE_FLATTEN_STRENGTH = 1f;

        private readonly TileData[] _grid;

        public Map(int width, int height)
        {
            Width = width;
            Height = height;

            _grid = new TileData[width * height];

            Guests = new GuestManager();
            Economy = new EconomyManager(1000);
            Structures = new StructureManager(this, Economy);
        }

        public MapSaveData GetSaveData()
        {
            TileData[] tiles = new TileData[_grid.Length];
            Array.Copy(_grid, tiles, _grid.Length);

            MapSaveData data = new MapSaveData
            {
                Width = Width,
                Height = Height,
                Tiles = tiles,
            };

            return data;
        }

        public void LoadSaveData(MapSaveData data)
        {
            if (data == null || data.Tiles == null || data.Tiles.Length != _grid.Length)
            {
                return;
            }

            Array.Copy(data.Tiles, _grid, _grid.Length);

            OnMapChange?.Invoke();
            OnFoliageChange?.Invoke();
        }

        public void PaintPiste(Vector2Int centerLoc)
        {
            if (!InBounds(centerLoc))
            {
                return;
            }

            // The target height for the "Cut" is the current height at the brush center.
            float centerHeight = GetTile(centerLoc).Height;
            int radiusCells = Mathf.CeilToInt(PISTE_RADIUS);
            bool terrainChanged = false;

            for (int x = centerLoc.x - radiusCells; x <= centerLoc.x + radiusCells; x++)
            {
                for (int z = centerLoc.y - radiusCells; z <= centerLoc.y + radiusCells; z++)
                {
                    if (!InBounds(x, z))
                    {
                        continue;
                    }

                    Vector2Int currentLoc = new Vector2Int(x, z);

                    float distance = Vector2.Distance(centerLoc, currentLoc);
                    if (distance > PISTE_RADIUS)
                    {
                        continue;
                    }

                    // Remove foliage and mark as packed snow.
                    int index = GetIndex(x, z);
                    if (_grid[index].Structure == StructureType.Tree)
                    {
                        RemoveStructure(currentLoc);
                    }

                    _grid[index].Type = TileType.PackedSnow;

                    // Step 2: Mellowing the Terrain (The "Cut").
                    // We lerp the height toward a blend of the local average and the
                    // center height.
                    float currentHeight = _grid[index].Height;
                    float averageHeight = GetAverageNeighborHeight(x, z);
                    float targetHeight = Mathf.Lerp(
                        averageHeight,
                        centerHeight,
                        PISTE_FLATTEN_STRENGTH
                    );

                    float falloff = 1f - Mathf.Clamp01(distance / PISTE_RADIUS);
                    float lerpAmount = PISTE_HEIGHT_LERP * falloff;

                    _grid[index].Height = Mathf.Lerp(currentHeight, targetHeight, lerpAmount);
                    terrainChanged = true;
                }
            }

            if (terrainChanged)
            {
                OnMapChange?.Invoke();
            }
        }

        public void PaintPisteStroke(Vector2Int start, Vector2Int end)
        {
            float distance = Vector2Int.Distance(start, end);
            int steps = Mathf.CeilToInt(distance * 2f);

            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0f : i / (float)steps;
                float x = Mathf.Lerp(start.x, end.x, t);
                float z = Mathf.Lerp(start.y, end.y, t);

                Vector2Int pos = new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(z));
                PaintPiste(pos);
            }
        }

        private float GetAverageNeighborHeight(int x, int z)
        {
            float totalHeight = 0f;
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
                return totalHeight / (float)count;
            }
            return _grid[GetIndex(x, z)].Height;
        }

        public int GetIndex(int x, int z)
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

        public void SetTileHeight(Vector2Int loc, float height)
        {
            SetTileHeight(loc.x, loc.y, height);
        }

        public void SetTileHeight(int x, int z, float height)
        {
            if (InBounds(x, z))
            {
                _grid[GetIndex(x, z)].Height = height;
                OnMapChange?.Invoke();
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
            if (structure == StructureType.None)
            {
                Debug.LogError(
                    "Do not use SetStructure to remove structures! Use RemoveStructure()."
                );
                return;
            }
            if (InBounds(x, z))
            {
                _grid[GetIndex(x, z)].Structure = structure;
                OnMapChange?.Invoke();
            }
        }

        public void ApplyTrees(bool[] trees)
        {
            for (int i = 0; i < trees.Length; i++)
            {
                if (trees[i])
                {
                    if (_grid[i].Structure == StructureType.None)
                    {
                        _grid[i].Structure = StructureType.Tree;
                    }
                }
            }
            OnFoliageChange?.Invoke();
        }

        public void RemoveStructure(Vector2Int loc)
        {
            RemoveStructure(loc.x, loc.y);
        }

        public void RemoveStructure(int x, int z)
        {
            if (InBounds(x, z))
            {
                StructureType oldStructure = _grid[GetIndex(x, z)].Structure;
                _grid[GetIndex(x, z)].Structure = StructureType.None;
                if (oldStructure == StructureType.Tree)
                {
                    OnFoliageChange?.Invoke();
                }
                OnMapChange?.Invoke();
            }
        }
    }
}
