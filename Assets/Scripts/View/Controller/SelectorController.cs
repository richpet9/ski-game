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
        private Camera cam;

        [SerializeField]
        private LayerMask terrainLayer;

        [Header("Visuals")]
        [SerializeField]
        private GameObject cursorVisual;

        private const float VERTICAL_OFFSET = 0.1f;
        private const float RAY_HEIGHT = 2000f;

        public Vector2Int GridPosition { get; private set; }
        public bool IsValid { get; private set; }
        public event Action<Vector2Int> OnTileClicked;

        private void Update()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, RAY_HEIGHT, terrainLayer))
            {
                IsValid = true;
                GridPosition = MapUtil.WorldToGrid(hit.point);
                cursorVisual.transform.position = MapUtil.GridToWorld(
                    GridPosition,
                    hit.point.y + VERTICAL_OFFSET
                );
                cursorVisual.SetActive(true);

                // Debug interaction.
                if (Input.GetMouseButtonDown(0))
                {
                    OnTileClicked?.Invoke(GridPosition);
                }
            }
            else
            {
                IsValid = false;
                cursorVisual.SetActive(false);
            }
        }
    }
}
