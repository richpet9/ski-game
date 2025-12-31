using System.Collections;
using System.Collections.Generic;
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

        // Cache variables for prevent spamming operations.
        private Vector3? _lastTargetPos;
        private bool _isVisible = true;

        public void Initialize(GuestData data)
        {
            _agent = new GuestAgent(data);

            foreach (MeshRenderer renderer in _renderers)
            {
                renderer.material.color = data.Color;
            }
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
            if (_agent == null || _agent.QueuedForDestruction)
            {
                if (_agent?.QueuedForDestruction == true)
                {
                    Destroy(gameObject);
                }
                return;
            }

            if (_agent.Data.TargetPosition.HasValue)
            {
                Vector3 newTarget = _agent.Data.TargetPosition.Value;
                if (
                    !_lastTargetPos.HasValue
                    || Vector3.Distance(_lastTargetPos.Value, newTarget) > 0.1f
                )
                {
                    _navAgent.SetDestination(newTarget);
                    _lastTargetPos = newTarget;
                }
            }

            float distance = _navAgent.pathPending
                ? float.PositiveInfinity
                : _navAgent.remainingDistance;

            _agent.Tick(Time.deltaTime, transform.position, distance);

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
