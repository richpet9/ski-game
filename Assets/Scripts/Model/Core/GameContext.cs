using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;

namespace SkiGame.Model.Core
{
    public static class GameContext
    {
        private static MapData _map;
        private static StructureManager _structures;

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

        public static StructureManager Structures
        {
            get
            {
                if (_structures == null)
                {
                    UnityEngine.Debug.LogError("GameContext: Structures accessed but is null.");
                }
                return _structures;
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

        public static void RegisterStructures(StructureManager structures)
        {
            if (_structures != null)
            {
                UnityEngine.Debug.LogWarning("GameContext: StructureManager is being overwritten!");
            }
            _structures = structures;
        }

        public static void Clear()
        {
            _map = null;
            _structures = null;
        }
    }
}
