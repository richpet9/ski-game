using SkiGame.Model.Agents;
using SkiGame.Model.Guest;
using UnityEngine;
using UnityEngine.AI;

namespace SkiGame.View.Agents
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GuestView : MonoBehaviour
    {
        private GuestAgent _agent;
        private NavMeshAgent _navAgent;
        private MeshRenderer[] _renderers;

        private bool _isVisible = true;

        public void Initialize(GuestData data)
        {
            _agent = new GuestAgent(data);
        }

        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _renderers = GetComponentsInChildren<MeshRenderer>();

            _navAgent.speed = 3.5f;
            _navAgent.angularSpeed = 200f;
            _navAgent.acceleration = 10f;
        }

        private void Update()
        {
            if (_agent == null)
            {
                Debug.LogError("Agent is null. Destroying GameObject.");
                Destroy(gameObject);
                return;
            }

            if (_agent.QueuedForDestruction)
            {
                Destroy(gameObject);
                return;
            }

            if (_agent.Data.TargetPosition.HasValue)
            {
                _navAgent.SetDestination(_agent.Data.TargetPosition.Value);
            }

            _agent.Tick(Time.deltaTime, transform.position, _navAgent.remainingDistance);

            SyncVisible();
        }

        private void SyncVisible()
        {
            bool shouldBeVisible = _agent.Data.IsVisible;
            if (_isVisible != shouldBeVisible)
            {
                foreach (MeshRenderer renderer in _renderers)
                {
                    renderer.enabled = shouldBeVisible;
                }
                _isVisible = shouldBeVisible;
            }
        }
    }
}
