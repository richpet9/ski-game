using System.Collections;
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
        private const float ACCELERATION = 10f;
        private const float ANGULAR_SPEED = 200f;
        private const float WALK_SPEED = 3.5f;
        private const float SKI_SPEED = 12f;
        private const float LIFT_SPEED = 10f;
        private const float MINIMUM_TARGET_DIST = 0.5f;
        private const float MINIMUM_PATH_DIST = 0.1f;

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

            _navAgent.speed = WALK_SPEED;
            _navAgent.angularSpeed = ANGULAR_SPEED;
            _navAgent.acceleration = ACCELERATION;
            _navAgent.autoTraverseOffMeshLink = false;
        }

        private void Update()
        {
            if (_agent == null || TryDestroy())
            {
                return;
            }

            _agent.Data.Position = transform.position;

            // Adjust speed based on state.
            // TODO: Move this to agent.
            if (_agent.Data.State == GuestState.Skiing)
            {
                if (_navAgent.speed != SKI_SPEED)
                {
                    _navAgent.speed = SKI_SPEED;
                }
            }
            else if (_navAgent.speed != WALK_SPEED)
            {
                _navAgent.speed = WALK_SPEED;
            }

            if (_isTraversingLift)
            {
                return;
            }

            if (_navAgent.isOnOffMeshLink && !_isTraversingLift)
            {
                // Only traverse if we are heading to a lift.
                // This prevents accidentally traversing other off-mesh links.
                if (_agent.Data.State == GuestState.WalkingToLift)
                {
                    StartCoroutine(TraverseLift());
                    return;
                }
            }

            // More robust arrival detection that correctly handles off-mesh links.
            if (_agent.Data.TargetPosition.HasValue && _navAgent.hasPath && !_navAgent.pathPending)
            {
                float dist = Vector3.Distance(_agent.Data.TargetPosition.Value, transform.position);
                if (
                    !_hasNotifiedArrival
                    && !_navAgent.isOnOffMeshLink
                    && dist < MINIMUM_TARGET_DIST
                )
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
                    || Vector3.Distance(_lastTargetPos.Value, newTarget) > MINIMUM_PATH_DIST
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
            _agent.BeginLiftTraversal();

            // Capture the destination before disabling the agent.
            OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
            Vector3 startPos = transform.position;
            Vector3 endPos = data.endPos + Vector3.up * _navAgent.baseOffset;

            // DISABLE the agent. This frees up the "Start Node" immediately,
            // allowing the next agent to enter the lift behind us.
            _navAgent.enabled = false;

            float duration = Vector3.Distance(startPos, endPos) / LIFT_SPEED;
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

            // Explicitly notify the agent that it has arrived at the end of the lift.
            _agent.NotifyArrival();
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
