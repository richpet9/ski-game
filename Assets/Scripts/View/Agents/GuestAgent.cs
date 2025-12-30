using UnityEngine;
using UnityEngine.AI; // Standard namespace for Agents

namespace SkiGame.View.Agents
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GuestAgent : MonoBehaviour
    {
        private const float WANDER_RADIUS = 20f;
        private const float WAIT_TIME = 2f;

        private NavMeshAgent _agent;
        private float _timer;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = 3.5f;
            _agent.angularSpeed = 200f;
            _agent.acceleration = 10f;
        }

        private void Update()
        {
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
            {
                _timer += Time.deltaTime;
                if (_timer > WAIT_TIME)
                {
                    SetRandomDestination();
                    _timer = 0;
                }
            }
        }

        private void SetRandomDestination()
        {
            // Pick a random point inside a sphere.
            Vector3 randomDirection = Random.insideUnitSphere * WANDER_RADIUS;
            randomDirection += transform.position;

            // Find the nearest valid point on the NavMesh.
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, WANDER_RADIUS, 1))
            {
                _agent.SetDestination(hit.position);
            }
        }
    }
}
