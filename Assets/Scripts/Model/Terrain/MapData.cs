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

        private TileType GetTerrainTypeFromHeight(float height)
        {
            // Use a constant in the shader and here.
            if (height > 10)
            {
                return TileType.SNOW;
            }
            else
            {
                return TileType.GRASS;
            }
        }
    }
}
