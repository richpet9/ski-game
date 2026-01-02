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

        private const float PREVIEW_CABLE_HEIGHT = 2f;

        private StructureType _structureType = StructureType.Lodge;
        private Vector2Int? _liftStartPos;

        private void Update()
        {
            if (_liftStartPos.HasValue)
            {
                DrawListPreviewEndpoint();
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

            (bool success, string error) = GameContext.Map.Structures.TryBuild(
                gridPos,
                _structureType
            );
            if (!success)
            {
                Debug.Log($"Failed to build {_structureType}: {error}");
            }
        }

        private void HandleLiftInput(Vector2Int gridPos)
        {
            if (_liftStartPos == null)
            {
                _liftStartPos = gridPos;

                float height = GameContext.Map.GetTile(_selector.GridPosition).Height;
                _previewCable.gameObject.SetActive(true);
                _previewCable.SetPosition(
                    0,
                    MapUtil.GridToWorld(gridPos, height + PREVIEW_CABLE_HEIGHT)
                );
            }
            else
            {
                Vector2Int endPos = gridPos;
                if (endPos != _liftStartPos)
                {
                    (bool success, string error) = GameContext.Map.Structures.TryBuildLift(
                        (Vector2Int)_liftStartPos,
                        endPos
                    );
                    if (!success)
                    {
                        Debug.Log($"Failed to build lift: {error}");
                    }
                }

                // Reset preview cable.
                _liftStartPos = null;
                _previewCable.gameObject.SetActive(false);
            }
        }

        private void DrawListPreviewEndpoint()
        {
            float height = GameContext.Map.GetTile(_selector.GridPosition).Height;
            _previewCable.SetPosition(
                1,
                MapUtil.GridToWorld(_selector.GridPosition, height + PREVIEW_CABLE_HEIGHT)
            );
        }
    }
}
