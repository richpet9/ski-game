using SkiGame.Model.Core;
using SkiGame.Model.Structure;
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
            MapData map = GameContext.Map;
            if (map == null)
            {
                return;
            }

            bool success = map.TrySetStructure(gridPos, StructureType.Lodge);
            if (!success)
            {
                Debug.Log("Cannot build here.");
                return;
            }
            SpawnStructureVisual(gridPos, map.GetTile(gridPos).Height);
        }

        private void SpawnStructureVisual(Vector2Int gridPos, float height)
        {
            Vector3 worldPos = new(gridPos.x + 0.5f, height, gridPos.y + 0.5f);
            GameObject structure = Instantiate(_lodgePrefab, worldPos, Quaternion.identity);
            structure.transform.parent = transform;
        }
    }
}
