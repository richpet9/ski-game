using System.Collections;
using SkiGame.Model.Agents;
using SkiGame.Model.Guest;
using SkiGame.View.Agents;
using SkiGame.View.Controller;
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
                        Money = 0,
                    }
                );
            }
        }
    }
}
