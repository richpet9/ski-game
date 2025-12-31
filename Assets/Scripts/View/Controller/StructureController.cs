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

        private StructureType _structureType = StructureType.Lodge;

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
            bool success = GameContext.Structures.TryBuild(gridPos, _structureType);
            if (!success)
            {
                Debug.Log($"Failed to build structure: {_structureType}");
            }
        }
    }
}
