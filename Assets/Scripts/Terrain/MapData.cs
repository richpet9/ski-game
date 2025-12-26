namespace SkiGame.Terrain
{
    public class MapData
    {
        private TileType[,] _grid;

        public MapData(MapConfig mapConfig)
        {
            _grid = new TileType[mapConfig.Width, mapConfig.Height];
        }
    }
}
