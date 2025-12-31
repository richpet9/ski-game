using UnityEngine;

namespace SkiGame.View.Controller
{
    public class DebugController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField]
        private SelectorController _selector;

        [Header("Prefabs")]
        [SerializeField]
        private GameObject _guestPrefab;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnGuest(_selector.GridPosition);
            }
        }

        private void SpawnGuest(Vector2Int gridPos)
        {
            Vector3 worldPos = new Vector3(gridPos.x + 0.5f, 1f, gridPos.y + 0.5f);
            GameObject guest = Instantiate(_guestPrefab, worldPos, Quaternion.identity);
            guest.transform.parent = transform;
        }
    }
}
