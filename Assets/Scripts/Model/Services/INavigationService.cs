using UnityEngine;

namespace SkiGame.Model.Services
{
    public interface INavigationService
    {
        public bool SamplePosition(
            Vector3 sourcePosition,
            out Vector3 hitPosition,
            float maxDistance
        );

        public Vector2 GetFlow(Vector3 worldPos);

        public Vector3 GetNextPathPosition(Vector3 currentPos, Vector3 targetPos);
    }
}
