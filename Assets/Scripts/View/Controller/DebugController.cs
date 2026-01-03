using SkiGame.Model.Core;
using SkiGame.Model.Guest;
using SkiGame.Model.Terrain;
using SkiGame.View.Agents;
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

        private const float SPAWN_HEIGHT = 1f;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SpawnGuest(_selector.GridPosition);
            }
        }

        private void SpawnGuest(Vector2Int gridPos)
        {
            Vector3 worldPos = MapUtil.GridToWorld(gridPos, SPAWN_HEIGHT);
            GameObject guest = Instantiate(_guestPrefab, worldPos, Quaternion.identity);
            guest
                .GetComponent<GuestView>()
                .Initialize(
                    new GuestData
                    {
                        Position = worldPos,
                        HomePosition = null,
                        State = GuestState.Wandering,
                        Money = 0,
                        Color = Color.white,
                        Energy = 255,
                    }
                );
            guest.transform.parent = transform;
            GameContext.Map.Guests.AddGuest();
        }
    }
}
