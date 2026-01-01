using UnityEngine;

namespace SkiGame.Model.Terrain
{
    public static class MapUtil
    {
        // Centers the entity on the tile (x + 0.5) and applies height.
        public static Vector3 GridToWorld(Vector2Int gridPos, float height = 0f)
        {
            return new Vector3(gridPos.x + 0.5f, height, gridPos.y + 0.5f);
        }

        public static Vector2Int WorldToGrid(Vector3 worldPos)
        {
            return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.z));
        }
    }
}
