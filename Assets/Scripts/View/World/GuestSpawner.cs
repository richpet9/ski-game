using System.Collections;
using SkiGame.Model.Agents;
using SkiGame.Model.Core;
using SkiGame.Model.Guest;
using SkiGame.View.Agents;
using UnityEngine;

namespace SkiGame.View.World
{
    public class GuestSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField]
        private GameObject _guestPrefab;

        [Header("Config")]
        [SerializeField]
        private float _spawnInterval = 5f;

        private const ushort SPAWN_MONEY = 255;
        private const byte SPAWN_ENERGY = 255;

        private void Start()
        {
            StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_spawnInterval);
                SpawnGuest();
            }
        }

        private void SpawnGuest()
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

            GameObject guestObj = Instantiate(_guestPrefab, spawnPos, Quaternion.identity);
            if (guestObj.TryGetComponent(out GuestView guest))
            {
                guest.Initialize(
                    new GuestData
                    {
                        Position = spawnPos,
                        HomePosition = transform.position,
                        State = GuestState.Wandering,
                        Color = Random.ColorHSV(),
                        Money = SPAWN_MONEY,
                        Energy = SPAWN_ENERGY,
                    }
                );
                GameContext.Map.Guests.AddGuest();
            }
            else
            {
                Destroy(guestObj);
            }
        }
    }
}
