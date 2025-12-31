using SkiGame.Model.Core;
using UnityEngine;
using UnityEngine.AI; // Standard namespace for Agents

namespace SkiGame.View.Agents
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GuestAgent : MonoBehaviour
    {
        private enum State : byte
        {
            Wandering = 0,
            InsideLodge = 1,
            WalkingToLodge = 2,
        }

        private const float WANDER_RADIUS = 20f;
        private const float WANDER_WAIT_TIME = 2f;
        private const float LODGE_WAIT_TIME = 3f;

        private NavMeshAgent _agent;
        private MeshRenderer[] _renderers;
        private State _state = State.WalkingToLodge;
        private float _timer = 0f;
        private bool _shown = true;

        private void Awake()
        {
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = 3.5f;
            _agent.angularSpeed = 200f;
            _agent.acceleration = 10f;
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Wandering:
                    Wander();
                    break;
                case State.InsideLodge:
                    InsideLodge();
                    break;
                case State.WalkingToLodge:
                    WalkToLodge();
                    break;
            }
        }

        private void Wander()
        {
            if (!_agent.hasPath && !_agent.pathPending)
            {
                _timer += Time.deltaTime;
                if (_timer > WANDER_WAIT_TIME)
                {
                    SetRandomDestination();
                    _timer = 0;
                }
            }
        }

        private void InsideLodge()
        {
            if (_shown)
            {
                Hide();
            }

            _timer += Time.deltaTime;
            if (_timer > LODGE_WAIT_TIME)
            {
                Show();
                SetRandomDestination();
                _state = State.Wandering;
                _timer = 0;
            }
        }

        private void WalkToLodge()
        {
            if (!_agent.hasPath && !_agent.pathPending && GameContext.Structures.Lodges.Count > 0)
            {
                // TODO: Change from first lodge to nearest (or other hueristic).
                Vector2Int lodgePos = GameContext.Structures.Lodges[0];
                float tileHeight = GameContext.Map.GetTile(lodgePos).Height;
                _agent.SetDestination(new Vector3(lodgePos.x + .5f, tileHeight, lodgePos.y + .5f));
            }
            else if (_agent.remainingDistance <= 1.42f)
            {
                _agent.ResetPath();
                _timer = 0;
                _state = State.InsideLodge;
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

        // TODO: Maybe visuals should be handled in a GuestView class.
        private void Hide()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = false;
            }
            _shown = false;
        }

        // TODO: Maybe visuals should be handled in a GuestView class.
        private void Show()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = true;
            }
            _shown = true;
        }
    }
}
