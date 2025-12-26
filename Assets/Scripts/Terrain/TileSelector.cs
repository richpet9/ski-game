using SkiGame.Main;
using UnityEngine;

namespace SkiGame.Terrain
{
    public class TileSelector : MonoBehaviour
    {
        [SerializeField]
        private Camera cam;

        [SerializeField]
        private LayerMask terrainLayer;

        [SerializeField]
        private GameObject cursorVisual;

        private const float RAY_HEIGHT = 2000f;

        private void Update()
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, RAY_HEIGHT, terrainLayer))
            {
                // Snap to integer grid.
                int x = Mathf.RoundToInt(hit.point.x);
                int z = Mathf.RoundToInt(hit.point.z);

                // Get the precise height of the mesh at this integer coordinate.
                // (Optional: You could also read this from your MountainGen data if accessible).
                Vector3 snappedPos = new(x, hit.point.y, z);

                cursorVisual.transform.position = snappedPos;
                cursorVisual.SetActive(true);

                // Debug interaction.
                if (Input.GetMouseButton(0))
                {
                    TileData data = GameContext.Map.GetTile(x, z);
                    Debug.Log(
                        $"Painting Tile at: {x}, {z}; Type: {data.Type} Height: {data.Height}"
                    );
                }
            }
            else
            {
                cursorVisual.SetActive(false);
            }
        }
    }
}
