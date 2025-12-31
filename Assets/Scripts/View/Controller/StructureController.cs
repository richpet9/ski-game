using SkiGame.Model.Core;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.View.Controller
{
    public class StructureController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SelectorController _selector;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject _lodgePrefab;

        [SerializeField]
        private GameObject _parkingLotPrefab;

        private StructureType _structureType = StructureType.Lodge;

        private int GetStructureCost(StructureType structure) =>
            structure switch
            {
                StructureType.Lodge => 100,
                StructureType.ParkingLot => 50,
                _ => 0,
            };

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _structureType = StructureType.Lodge;
                Debug.Log("Selected: Lodge");
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _structureType = StructureType.ParkingLot;
                Debug.Log("Selected: Parking Lot");
            }
        }

        private void OnEnable()
        {
            if (_selector != null)
            {
                _selector.OnTileClicked += HandleTileClick;
            }
        }

        private void OnDisable()
        {
            if (_selector != null)
            {
                _selector.OnTileClicked -= HandleTileClick;
            }
        }

        private void HandleTileClick(Vector2Int gridPos)
        {
            int cost = GetStructureCost(_structureType);
            if (!GameContext.Economy.TrySpendMoney(cost))
            {
                Debug.Log("Not enough money!");
                return;
            }

            MapData map = GameContext.Map;
            if (map.TrySetStructure(gridPos, _structureType))
            {
                if (_structureType == StructureType.Lodge)
                {
                    GameContext.Structures.Lodges.Add(gridPos);
                }
                SpawnStructureVisual(gridPos, map.GetTile(gridPos).Height);
                Debug.Log(
                    $"Built {_structureType} for ${cost}. Remaining: ${GameContext.Economy.Money}"
                );
            }
            else
            {
                Debug.Log("Failed to build structure, refunding cost.");
                GameContext.Economy.AddMoney(cost);
            }
        }

        private void SpawnStructureVisual(Vector2Int gridPos, float height)
        {
            Vector3 worldPos = new Vector3(gridPos.x + 0.5f, height, gridPos.y + 0.5f);
            GameObject prefab = _structureType switch
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

            GameObject structure = Instantiate(prefab, worldPos, Quaternion.identity);
            structure.transform.parent = transform;
        }
    }
}
