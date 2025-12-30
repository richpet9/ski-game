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

        public event Action<Vector2Int> OnTileClicked;

        private const float RAY_HEIGHT = 2000f;

        private void Update()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, RAY_HEIGHT, terrainLayer))
            {
                // Snap to integer grid.
                int x = Mathf.FloorToInt(hit.point.x);
                int z = Mathf.FloorToInt(hit.point.z);

                Vector3 snappedPos = new(x + 0.5f, hit.point.y + 0.1f, z + 0.5f);

                cursorVisual.transform.position = snappedPos;
                cursorVisual.SetActive(true);

                // Debug interaction.
                if (Input.GetMouseButton(0))
                {
                    OnTileClicked?.Invoke(new Vector2Int(x, z));
                }
            }
            else
            {
                cursorVisual.SetActive(false);
            }
        }
    }
}
