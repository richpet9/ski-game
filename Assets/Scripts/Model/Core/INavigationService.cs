using UnityEngine;

namespace SkiGame.Model.Core
{
    public interface INavigationService
    {
        public bool SamplePosition(
            Vector3 sourcePosition,
            out Vector3 hitPosition,
            float maxDistance
        );
    }
}
