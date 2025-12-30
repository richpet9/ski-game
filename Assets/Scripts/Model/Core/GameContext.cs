using SkiGame.Model.Terrain;

namespace SkiGame.Model.Core
{
    public static class GameContext
    {
        private static MapData _map;

        public static MapData Map
        {
            get
            {
                if (_map == null)
                {
                    UnityEngine.Debug.LogError("GameContext: Map accessed but is null.");
                }
                return _map;
            }
        }

        public static void RegisterMap(MapData map)
        {
            if (_map != null)
            {
                UnityEngine.Debug.LogWarning("GameContext: MapData is being overwritten!");
            }
            _map = map;
        }

        public static void Clear()
        {
            _map = null;
        }
    }
}
