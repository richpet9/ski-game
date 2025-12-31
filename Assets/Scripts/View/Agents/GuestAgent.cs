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
            Leaving = 3,
        }

        private const float WANDER_RADIUS = 20f;
        private const float WANDER_WAIT_TIME = 2f;
        private const float LODGE_WAIT_TIME = 3f;
        private const float SQRT_2 = 1.142f;

        private NavMeshAgent _agent;
        private MeshRenderer[] _renderers;
        private Vector3 _homePosition;
        private State _state = State.WalkingToLodge;
        private float _timer = 0f;

        private void Awake()
        {
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = 3.5f;
            _agent.angularSpeed = 200f;
            _agent.acceleration = 10f;
        }

        public void SetHome(Vector3 homePos)
        {
            _homePosition = homePos;
        }

        public void Start()
        {
            SetNewDestination();
        }

        private void Update()
        {
            if (_state == State.WalkingToLodge)
            {
                if (!_agent.pathPending && _agent.remainingDistance < SQRT_2)
                {
                    EnterLodge();
                }
            }
            else if (_state == State.InsideLodge)
            {
                _timer += Time.deltaTime;
                if (_timer >= LODGE_WAIT_TIME)
                {
                    ExitLodge();
                }
            }
            else if (_state == State.Leaving)
            {
                if (!_agent.pathPending && _agent.remainingDistance < SQRT_2)
                {
                    Destroy(gameObject);
                }
            }
            else if (_state == State.Wandering)
            {
                if (!_agent.hasPath && !_agent.pathPending)
                {
                    _timer += Time.deltaTime;
                    if (_timer >= WANDER_WAIT_TIME)
                    {
                        SetNewDestination();
                    }
                }
            }
        }

        private void SetNewDestination()
        {
            if (GameContext.Structures.Lodges.Count > 0)
            {
                Vector2Int targetGrid = GameContext.Structures.Lodges[
                    Random.Range(0, GameContext.Structures.Lodges.Count)
                ];

                // TODO: We should ideally cache the world height in GameContext or
                // MapData to avoid doing "GetTile" checks here, but this works for now.
                // This can be implemented when we check that we only place structures
                // on flat ground.
                float y = 0;
                if (GameContext.Map != null)
                {
                    y = GameContext.Map.GetTile(targetGrid).Height;
                }

                Vector3 targetPos = new Vector3(targetGrid.x + 0.5f, y, targetGrid.y + 0.5f);

                _agent.SetDestination(targetPos);
                _state = State.WalkingToLodge;
            }
            else
            {
                SetRandomDestination();
                _state = State.Wandering;
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

        private void EnterLodge()
        {
            Hide();
            _state = State.InsideLodge;
            _timer = 0f;
        }

        private void ExitLodge()
        {
            Show();
            _state = State.Leaving;
            _timer = 0f;

            if (_homePosition != null)
            {
                _agent.SetDestination(_homePosition);
            }
            else
            {
                _agent.ResetPath();
            }
        }

        // TODO: Maybe visuals should be handled in a GuestView class.
        private void Hide()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = false;
            }
        }

        // TODO: Maybe visuals should be handled in a GuestView class.
        private void Show()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].enabled = true;
            }
        }
    }
}
