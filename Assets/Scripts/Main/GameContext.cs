using SkiGame.Terrain;

namespace SkiGame.Main
{
    public static class GameContext
    {
        public static MapData Map { get; private set; }

        public static void RegisterMap(MapData map)
        {
            if (Map != null)
            {
                UnityEngine.Debug.LogWarning("GameContext: MapData is being overwritten!");
            }
            Map = map;
        }

        public static void Clear()
        {
            Map = null;
        }
    }
}
