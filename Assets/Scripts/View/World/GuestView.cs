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
        private bool _isTraversingLink = false;

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
            if (_agent == null || _agent.QueuedForDestruction)
            {
                if (_agent?.QueuedForDestruction == true)
                {
                    Destroy(gameObject);
                }
                return;
            }

            if (_isTraversingLink)
            {
                return;
            }

            if (_navAgent.isOnOffMeshLink && !_isTraversingLink)
            {
                StartCoroutine(TraverseLift());
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

            SyncVisible();

            _agent.Tick(Time.deltaTime, transform.position, distance);
        }

        private IEnumerator TraverseLift()
        {
            _isTraversingLink = true;

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
            _isTraversingLink = false;
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
