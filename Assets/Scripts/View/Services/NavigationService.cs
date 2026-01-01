using SkiGame.Model.Core;
using UnityEngine;
using UnityEngine.AI;

namespace SkiGame.View.Services
{
    public class NavigationService : INavigationService
    {
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
