using SkiGame.Model.Core;
using SkiGame.Model.Structures;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SkiGame.View.World
{
    public class StructureView : MonoBehaviour
    {
        [Header("Assets")]
        [SerializeField]
        private GameObject _lodgePrefab;

        [SerializeField]
        private GameObject _parkingLotPrefab;

        [SerializeField]
        private GameObject _liftPrefab;

        private void OnEnable()
        {
            if (GameContext.Structures != null)
            {
                GameContext.Structures.OnStructureBuilt += SpawnStructure;
                GameContext.Structures.OnLiftBuilt += ConfigureLift;
            }
        }

        private void OnDisable()
        {
            if (GameContext.Structures != null)
            {
                GameContext.Structures.OnStructureBuilt -= SpawnStructure;
                GameContext.Structures.OnLiftBuilt -= ConfigureLift;
            }
        }

        private void SpawnStructure(Vector2Int pos, StructureType type)
        {
            float height = GameContext.Map.GetTile(pos).Height;
            // TODO: Use world coord converter.
            Vector3 worldPos = new Vector3(pos.x + 0.5f, height, pos.y + 0.5f);

            GameObject prefab = type switch
            {
                StructureType.Lodge => _lodgePrefab,
                StructureType.ParkingLot => _parkingLotPrefab,
                StructureType.Lift => _liftPrefab,
                _ => null,
            };

            if (prefab == null)
            {
                Debug.LogError("Failed to find a prefab to spawn.");
                return;
            }
            Instantiate(prefab, worldPos, Quaternion.identity, transform);
        }

        private void ConfigureLift(Vector2Int startGrid, Vector2Int endGrid)
        {
            // TODO: Draw cables as well...

            GameObject linkObj = new GameObject("LiftPath");
            NavMeshLink link = linkObj.AddComponent<NavMeshLink>();
            linkObj.transform.parent = transform;

            // TODO: Use world coord converter.
            Vector3 startPos = new Vector3(startGrid.x + 0.5f, 1f, startGrid.y + 0.5f);
            Vector3 endPos = new Vector3(endGrid.x + 0.5f, 1f, endGrid.y + 0.5f);

            // 1. Define idealized points (Center of Start Tower -> Center of End Tower)
            Vector3 localStart = Vector3.zero;
            Vector3 localEnd = endPos - startPos; // Relative vector to top

            // 2. Snap Start Point to closest valid NavMesh (Radius 2.0f)
            if (NavMesh.SamplePosition(startPos, out NavMeshHit hitStart, 2.0f, NavMesh.AllAreas))
            {
                localStart = linkObj.transform.InverseTransformPoint(hitStart.position);
            }

            // 3. Snap End Point to closest valid NavMesh
            if (NavMesh.SamplePosition(endPos, out NavMeshHit hitEnd, 2.0f, NavMesh.AllAreas))
            {
                localEnd = linkObj.transform.InverseTransformPoint(hitEnd.position);
            }

            link.startPoint = localStart;
            link.endPoint = localEnd;
            link.width = 2f;
            link.costModifier = -999;
            link.bidirectional = true;
            link.autoUpdate = true;

            // 1. Get the correct ID. If this doesn't match the Guest's Agent Type, they will ignore it.
            int agentTypeId = UnityEngine.AI.NavMesh.GetSettingsByIndex(0).agentTypeID;
            link.agentTypeID = agentTypeId;

            // 2. Refresh the link. Sometimes runtime links don't "snap" instantly on the first frame.
            link.enabled = false;
            link.enabled = true;

            // 3. (Optional) Visual Debugging - Check the console to see if it actually found the floor
            if (link.startPoint == Vector3.zero && link.endPoint == Vector3.zero)
            {
                Debug.LogError("Lift Link failed to find NavMesh positions!");
            }
        }
    }
}
