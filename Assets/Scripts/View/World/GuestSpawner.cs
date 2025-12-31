using System.Collections;
using SkiGame.View.Agents;
using UnityEngine;

namespace SkiGame.View.World
{
    public class GuestSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject _guestPrefab;

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
            if (guestObj.TryGetComponent(out GuestAgent agent))
            {
                agent.SetHome(transform.position);
                agent.Start();
            }
        }
    }
}
