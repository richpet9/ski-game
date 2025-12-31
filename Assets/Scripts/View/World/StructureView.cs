using SkiGame.Model.Core;
using SkiGame.Model.Structures;
using Unity.AI.Navigation;
using UnityEngine;

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
            float startHeight = GameContext.Map.GetTile(startGrid).Height;
            float endHeight = GameContext.Map.GetTile(endGrid).Height;

            Vector3 startPos = new Vector3(startGrid.x + 0.5f, startHeight, startGrid.y + 0.5f);
            Vector3 endPos = new Vector3(endGrid.x + 0.5f, endHeight, endGrid.y + 0.5f);
            GameObject linkObj = new GameObject("LiftLink");
            linkObj.transform.position = startPos;
            linkObj.transform.parent = transform;

            NavMeshLink link = linkObj.AddComponent<NavMeshLink>();

            Vector3 localStart = Vector3.zero;
            localStart.y += 0.5f;

            Vector3 localEnd = endPos - startPos;
            localEnd.y += 0.5f;

            link.startPoint = localStart;
            link.endPoint = localEnd;
            link.width = 2f;
            link.bidirectional = false;
            link.costModifier = -1000000;
            link.area = 0;
        }
    }
}
