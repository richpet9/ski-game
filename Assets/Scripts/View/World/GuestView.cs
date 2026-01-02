using System.Collections;
using System.Collections.Generic;
using SkiGame.Model.Agents;
using SkiGame.Model.Core;
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
        private bool _isTraversingLift = false;
        private bool _hasNotifiedArrival;

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
            _navAgent.autoTraverseOffMeshLink = false;
        }

        private void Update()
        {
            if (_agent == null || TryDestroy())
            {
                return;
            }

            _agent.Data.Position = transform.position;

            if (_isTraversingLift)
            {
                return;
            }

            if (_navAgent.isOnOffMeshLink && !_isTraversingLift)
            {
                StartCoroutine(TraverseLift());
                return;
            }

            if (
                _agent.Data.TargetPosition.HasValue
                && !_navAgent.pathPending
                && (!_navAgent.hasPath || _navAgent.velocity.sqrMagnitude == 0f)
                && Vector3.Distance(_agent.Data.Position, _agent.Data.TargetPosition.Value) < 0.1f
            )
            {
                if (!_hasNotifiedArrival)
                {
                    _agent.NotifyArrival();
                    _hasNotifiedArrival = true;
                }
            }
            else
            {
                _hasNotifiedArrival = false;
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

            SyncVisible();
        }

        private bool TryDestroy()
        {
            if (_agent.QueuedForDestruction)
            {
                _agent.Dispose();
                Destroy(gameObject);
                return true;
            }
            return false;
        }

        private IEnumerator TraverseLift()
        {
            _isTraversingLift = true;

            // Capture the destination before disabling the agent.
            OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
            Vector3 startPos = transform.position;
            Vector3 endPos = data.endPos + Vector3.up * _navAgent.baseOffset;

            // DISABLE the agent. This frees up the "Start Node" immediately,
            // allowing the next agent to enter the lift behind us.
            _navAgent.enabled = false;

            float duration = Vector3.Distance(startPos, endPos) / _navAgent.speed;
            float time = 0;

            while (time < duration)
            {
                transform.position = Vector3.Lerp(startPos, endPos, time / duration);
                time += Time.deltaTime;
                yield return null;
            }

            transform.position = endPos;

            // Re-enable and warp to ensure the internal state matches the visual state.
            _navAgent.enabled = true;
            _navAgent.Warp(endPos);

            // Since we disabled the agent, we don't need CompleteOffMeshLink().
            // We just resume normal behavior next frame.
            _lastTargetPos = null;
            _isTraversingLift = false;
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
