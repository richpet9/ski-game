using SkiGame.Model.Services;
using SkiGame.Model.Terrain;
using UnityEngine;
using UnityEngine.AI;

namespace SkiGame.View.Services
{
    public class NavigationService : INavigationService
    {
        private Vector2[,] _flowField;
        private Map _map;

        public void Initialize(Map map)
        {
            _map = map;
            _map.OnMapChanged += RebuildFlowField;
            RebuildFlowField();
        }

        private void RebuildFlowField()
        {
            _flowField = FlowFieldGenerator.Generate(_map);
        }

        public Vector2 GetFlow(Vector3 worldPos)
        {
            if (_flowField == null)
            {
                return Vector2.zero;
            }

            Vector2Int gridPos = MapUtil.WorldToGrid(worldPos);
            if (_map.InBounds(gridPos))
            {
                return _flowField[gridPos.x, gridPos.y];
            }

            return Vector2.zero;
        }

        public Vector3 GetNextPathPosition(Vector3 currentPos, Vector3 targetPos)
        {
            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(currentPos, targetPos, NavMesh.AllAreas, path))
            {
                if (path.corners.Length > 1)
                {
                    return path.corners[1]; // 0 is the current location.
                }
            }

            // Fallback: Line of sight.
            return targetPos;
        }

        public bool SamplePosition(
            Vector3 sourcePosition,
            out Vector3 hitPosition,
            float maxDistance
        )
        {
            if (
                NavMesh.SamplePosition(
                    sourcePosition,
                    out NavMeshHit hit,
                    maxDistance,
                    NavMesh.AllAreas
                )
            )
            {
                hitPosition = hit.position;
                return true;
            }

            hitPosition = Vector3.zero;
            return false;
        }
    }
}
