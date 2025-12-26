using SkiGame.Tile;
using UnityEngine;

namespace SkiGame.Maps
{
    public class MapData : MonoBehaviour
    {
        [SerializeField]
        MapConfig mapConfig;

        TileType[,] _tiles;

        void Start()
        {
            _tiles = new TileType[mapConfig.Width, mapConfig.Height];
        }
    }
}
