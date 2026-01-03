using System;
using SkiGame.Model.Core;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.View.Controller
{
    public class SelectorController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private Camera _cam;

        [SerializeField]
        private LayerMask _terrainLayer;

        [Header("Visuals")]
        [SerializeField]
        private GameObject _cursorVisual;

        private const float VERTICAL_OFFSET = 0.1f;
        private const float RAY_HEIGHT = 2000f;

        public Vector2Int GridPosition { get; private set; }
        public bool IsValid { get; private set; }
        public event Action<Vector2Int> OnTileClicked;

        private void Update()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, RAY_HEIGHT, _terrainLayer))
            {
                IsValid = true;
                GridPosition = MapUtil.WorldToGrid(hit.point);
                _cursorVisual.transform.position = MapUtil.GridToWorld(
                    GridPosition,
                    hit.point.y + VERTICAL_OFFSET
                );
                _cursorVisual.SetActive(true);

                // Debug interaction.
                if (Input.GetMouseButtonDown(0))
                {
                    OnTileClicked?.Invoke(GridPosition);
                }
            }
            else
            {
                IsValid = false;
                _cursorVisual.SetActive(false);
            }
        }
    }
}
