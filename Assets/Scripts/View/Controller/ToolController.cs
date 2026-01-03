using SkiGame.Model.Core;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.View.Controller
{
    public class ToolController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SelectorController _selector;

        [SerializeField]
        private LineRenderer _previewCable;

        private const float PREVIEW_CABLE_HEIGHT = 2f;

        private enum ToolMode
        {
            Build,
            Piste,
        }

        private ToolMode _currentMode = ToolMode.Build;
        private StructureType _structureType = StructureType.Lodge;
        private Vector2Int? _liftStartPos;
        private Vector2Int? _lastPisteGridPos;

        private void Update()
        {
            // Tool Switching
            if (Input.GetKeyDown(KeyCode.P))
            {
                _currentMode = ToolMode.Piste;
                _liftStartPos = null;
                _previewCable.gameObject.SetActive(false);
                _lastPisteGridPos = null;
                Debug.Log("Selected: Piste Tool");
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _currentMode = ToolMode.Build;
                _structureType = StructureType.Lodge;
                Debug.Log("Selected: Lodge");
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _currentMode = ToolMode.Build;
                _structureType = StructureType.ParkingLot;
                Debug.Log("Selected: Parking Lot");
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _currentMode = ToolMode.Build;
                _structureType = StructureType.Lift;
                Debug.Log("Selected: Lift");
            }

            // Tool Logic
            if (_currentMode == ToolMode.Build && _liftStartPos.HasValue)
            {
                DrawListPreviewEndpoint();
            }
            else if (_currentMode == ToolMode.Piste && Input.GetMouseButton(0) && _selector.IsValid)
            {
                // Paint piste while holding mouse
                if (_lastPisteGridPos != _selector.GridPosition)
                {
                    _lastPisteGridPos = _selector.GridPosition;
                    GameContext.Map.PaintPiste(_selector.GridPosition);
                }
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
            if (_currentMode != ToolMode.Build)
            {
                return;
            }

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
