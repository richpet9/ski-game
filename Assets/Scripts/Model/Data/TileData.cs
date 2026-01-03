using System;
using SkiGame.Model.Structures;

namespace SkiGame.Model.Data
{
    [Serializable]
    public struct TileData
    {
        public StructureType Structure;
        public TileType Type;
        public float Height;
    }
}
