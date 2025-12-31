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

            // TODO: Use world coord converter.
            Vector3 startPos = new Vector3(startGrid.x + 0.5f, 1f, startGrid.y + 0.5f);
            Vector3 endPos = new Vector3(endGrid.x + 0.5f, 1f, endGrid.y + 0.5f);
            GameObject linkObj = new GameObject("LiftLink");
            linkObj.transform.position = startPos;
            linkObj.transform.parent = transform;

            NavMeshLink link = linkObj.AddComponent<NavMeshLink>();

            // --- IMPROVED SNAPPING LOGIC ---

            // 1. Get the mask for "Walkable" only.
            // This prevents snapping to "Jump" areas or other non-traversable artifacts.
            int walkableMask = 1 << NavMesh.GetAreaFromName("Walkable");

            // 2. Query from slightly ABOVE the terrain (Vector3.up) to ensure we hit the top face.
            Vector3 queryStart = startPos + Vector3.up;
            Vector3 queryEnd = endPos + Vector3.up;

            Vector3 localStart = Vector3.zero;
            Vector3 localEnd = endPos - startPos; // Default fallbacks

            // 3. Snap Start
            if (NavMesh.SamplePosition(queryStart, out NavMeshHit hitStart, 3.0f, walkableMask))
            {
                localStart = linkObj.transform.InverseTransformPoint(hitStart.position);
            }
            else
            {
                Debug.LogError($"Could not find Walkable NavMesh at start {startPos}");
            }

            // 4. Snap End
            if (NavMesh.SamplePosition(queryEnd, out NavMeshHit hitEnd, 3.0f, walkableMask))
            {
                localEnd = linkObj.transform.InverseTransformPoint(hitEnd.position);
            }
            else
            {
                Debug.LogError($"Could not find Walkable NavMesh at end {endPos}");
            }

            // --- APPLY SETTINGS ---
            link.startPoint = localStart;
            link.endPoint = localEnd;
            link.width = 2f;
            link.bidirectional = true;
            link.costModifier = -1000000; // Use negative cost to make it preferred
            link.area = 0; // Walkable area

            // Force update logic
            link.agentTypeID = NavMesh.GetSettingsByIndex(0).agentTypeID;
            link.autoUpdate = true;
            link.enabled = false;
            link.enabled = true;
        }

        private void OnDrawGizmos()
        {
            // Visualize all links managed by this view
            var links = GetComponentsInChildren<NavMeshLink>();
            foreach (var link in links)
            {
                if (link.enabled)
                {
                    Gizmos.color = Color.green;
                    // Draw line between the actual snapped world points
                    Vector3 worldStart = link.transform.TransformPoint(link.startPoint);
                    Vector3 worldEnd = link.transform.TransformPoint(link.endPoint);
                    Gizmos.DrawLine(worldStart, worldEnd);
                    Gizmos.DrawSphere(worldStart, 0.5f);
                    Gizmos.DrawSphere(worldEnd, 0.5f);
                }
            }
        }
    }
}
