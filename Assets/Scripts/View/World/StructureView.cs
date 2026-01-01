using SkiGame.Model.Core;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using Unity.AI.Navigation;
using UnityEngine;

namespace SkiGame.View.World
{
    public class StructureView : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject _lodgePrefab;

        [SerializeField]
        private GameObject _parkingLotPrefab;

        [SerializeField]
        private GameObject _liftPrefab;

        [Header("Materials")]
        [SerializeField]
        private Material _liftCableMaterial;

        private void OnEnable()
        {
            if (GameContext.Map.Structures != null)
            {
                GameContext.Map.Structures.OnStructureBuilt += SpawnStructure;
                GameContext.Map.Structures.OnLiftBuilt += SpawnLiftStructure;
            }
        }

        private void OnDisable()
        {
            if (GameContext.Map.Structures != null)
            {
                GameContext.Map.Structures.OnStructureBuilt -= SpawnStructure;
                GameContext.Map.Structures.OnLiftBuilt -= SpawnLiftStructure;
            }
        }

        private void SpawnStructure(Vector2Int gridPos, StructureType structureType)
        {
            SpawnStructure(gridPos, structureType, transform);
        }

        private void SpawnStructure(
            Vector2Int gridPos,
            StructureType structureType,
            Transform parent
        )
        {
            float height = GameContext.Map.GetTile(gridPos).Height;
            Vector3 worldPos = MapUtil.GridToWorld(gridPos, height);

            GameObject prefab = structureType switch
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
            Instantiate(prefab, worldPos, Quaternion.identity, parent);
        }

        private void SpawnLiftStructure(Vector2Int startGrid, Vector2Int endGrid)
        {
            float startHeight = GameContext.Map.GetTile(startGrid).Height;
            float endHeight = GameContext.Map.GetTile(endGrid).Height;
            Vector3 startPos = MapUtil.GridToWorld(startGrid, startHeight);
            Vector3 endPos = MapUtil.GridToWorld(endGrid, endHeight);

            GameObject liftParent = new GameObject("LiftContainer");
            liftParent.transform.parent = transform;

            SpawnStructure(startGrid, StructureType.Lift, liftParent.transform);
            SpawnStructure(endGrid, StructureType.Lift, liftParent.transform);

            LineRenderer lineRenderer = liftParent.AddComponent<LineRenderer>();
            lineRenderer.material = _liftCableMaterial;
            lineRenderer.startColor = Color.gray;
            lineRenderer.endColor = Color.gray;
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            lineRenderer.SetPosition(0, startPos + Vector3.up * 3f);
            lineRenderer.SetPosition(1, endPos + Vector3.up * 3f);

            GameObject linkObj = new GameObject("LiftLink");
            linkObj.transform.position = startPos;
            linkObj.transform.parent = liftParent.transform;

            NavMeshLink link = linkObj.AddComponent<NavMeshLink>();

            Vector3 localStart = Vector3.zero + Vector3.up * 0.5f;
            Vector3 localEnd = endPos - startPos + Vector3.up * 0.5f;

            link.startPoint = localStart;
            link.endPoint = localEnd;
            link.width = 1f;
            link.bidirectional = false;
            link.costModifier = -1000000;
            link.area = 0;
        }
    }
}
