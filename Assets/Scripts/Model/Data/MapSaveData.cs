using System;

namespace SkiGame.Model.Data
{
    [Serializable]
    public sealed class MapSaveData
    {
        public int Width;
        public int Height;
        public TileData[] Tiles;
    }
}
