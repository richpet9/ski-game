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

        [SerializeField]
        private LineRenderer _previewCable;

        private StructureType _structureType = StructureType.Lodge;
        private Vector2Int? _liftStartPos;

        private void Update()
        {
            if (_liftStartPos.HasValue)
            {
                HandleLiftPreview();
            }

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

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _structureType = StructureType.Lift;
                Debug.Log("Selected: Lift");
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
            if (_structureType == StructureType.Lift)
            {
                HandleLiftInput(gridPos);
                return;
            }

            bool success = GameContext.Structures.TryBuild(gridPos, _structureType);
            if (!success)
            {
                Debug.Log($"Failed to build structure: {_structureType}");
            }
        }

        private void HandleLiftInput(Vector2Int gridPos)
        {
            // TODO: Use world coord converter.
            Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 1f, gridPos.y + 0.5f);
            if (_liftStartPos == null)
            {
                _liftStartPos = gridPos;

                _previewCable.gameObject.SetActive(true);
                _previewCable.SetPosition(0, worldPos);
            }
            else
            {
                Vector2Int endPos = gridPos;
                if (endPos != _liftStartPos)
                {
                    GameContext.Structures.TryBuildLift((Vector2Int)_liftStartPos, endPos);
                }

                // Reset
                _liftStartPos = null;
                _previewCable.gameObject.SetActive(false);
            }
        }

        private void HandleLiftPreview()
        {
            _previewCable.SetPosition(1, _selector.WorldPosition);
        }
    }
}
