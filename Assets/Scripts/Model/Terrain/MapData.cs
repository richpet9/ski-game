using PlasticPipe.PlasticProtocol.Messages;
using SkiGame.Model.Structure;
using UnityEngine;

namespace SkiGame.Model.Terrain
{
    public class MapData
    {
        private readonly TileData[,] _grid;

        public MapData(int width, int height)
        {
            _grid = new TileData[width + 1, height + 1];
        }

        public void SetTile(Vector2Int loc, float height)
        {
            SetTile(loc.x, loc.y, height);
        }

        public void SetTile(int x, int z, float height)
        {
            _grid[x, z].Height = height;
            _grid[x, z].Type = GetTerrainTypeFromHeight(height);
        }

        public TileData GetTile(Vector2Int loc)
        {
            return GetTile(loc.x, loc.y);
        }

        public TileData GetTile(int x, int z)
        {
            return _grid[x, z];
        }

        public bool TrySetStructure(Vector2Int loc, StructureType structure)
        {
            return TrySetStructure(loc.x, loc.y, structure);
        }

        public bool TrySetStructure(int x, int z, StructureType structure)
        {
            if (!InBounds(x, z))
            {
                return false;
            }

            if (_grid[x, z].Structure != StructureType.None)
            {
                return false;
            }

            _grid[x, z].Structure = structure;
            return true;
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

        private bool InBounds(int x, int z)
        {
            return x >= 0 && x < _grid.GetLength(0) && z >= 0 && z < _grid.GetLength(1);
        }
    }
}
