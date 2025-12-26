using SkiGame.Tile;

namespace SkiGame.Maps
{
    public class MapData
    {
        TileType[,] _grid;

        public MapData(MapConfig mapConfig)
        {
            _grid = new TileType[mapConfig.Width, mapConfig.Height];
        }
    }
}
