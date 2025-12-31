using SkiGame.Model.Core;
using SkiGame.Model.Structures;
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

        private void OnEnable()
        {
            if (GameContext.Structures != null)
            {
                GameContext.Structures.OnStructureBuilt += SpawnStructure;
            }
        }

        private void OnDisable()
        {
            if (GameContext.Structures != null)
            {
                GameContext.Structures.OnStructureBuilt -= SpawnStructure;
            }
        }

        private void SpawnStructure(Vector2Int pos, StructureType type)
        {
            float height = GameContext.Map.GetTile(pos).Height;
            Vector3 worldPos = new Vector3(pos.x + 0.5f, height, pos.y + 0.5f);

            GameObject prefab = type switch
            {
                StructureType.Lodge => _lodgePrefab,
                StructureType.ParkingLot => _parkingLotPrefab,
                _ => null,
            };

            if (prefab == null)
            {
                Debug.LogError("Failed to find a prefab to spawn.");
                return;
            }
            Instantiate(prefab, worldPos, Quaternion.identity, transform);
        }
    }
}
