using SkiGame.Model.Core;
using SkiGame.Model.Guest;
using UnityEngine;
using UnityEngine.AI; // Standard namespace for Agents

namespace SkiGame.View.Agents
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class GuestAgent : MonoBehaviour
    {
        private GuestData _data;

        private const float WANDER_RADIUS = 20f;
        private const float WANDER_WAIT_TIME = 2f;
        private const float LODGE_WAIT_TIME = 3f;

        private NavMeshAgent _agent;
        private MeshRenderer[] _renderers;
        private float _timer = 0f;

        public void Initialize(GuestData data)
        {
            _data = data;
        }

        private void Start()
        {
            _renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = 3.5f;
            _agent.angularSpeed = 200f;
            _agent.acceleration = 10f;

            SetNewDestination();
        }

        private void Update()
        {
            _data.Position = transform.position;

            if (_data.State == GuestState.WalkingToLodge)
            {
                if (!_agent.pathPending && _agent.remainingDistance < 1.2f)
                {
                    EnterLodge();
                }
            }
            else if (_data.State == GuestState.InsideLodge)
            {
                _timer += Time.deltaTime;
                if (_timer >= LODGE_WAIT_TIME)
                {
                    ExitLodge();
                }
            }
            else if (_data.State == GuestState.Leaving)
            {
                if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                {
                    Destroy(gameObject);
                }
            }
            else if (_data.State == GuestState.Wandering)
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
                float y = GameContext.Map.GetTile(targetGrid).Height;
                Vector3 targetPos = new Vector3(targetGrid.x + 0.5f, y, targetGrid.y + 0.5f);

                _agent.SetDestination(targetPos);
                _data.State = GuestState.WalkingToLodge;
            }
            else
            {
                SetRandomDestination();
                _data.State = GuestState.Wandering;
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
            _data.State = GuestState.InsideLodge;
            _timer = 0f;

            GameContext.Economy.AddMoney(15);
        }

        private void ExitLodge()
        {
            Show();
            _data.State = GuestState.Leaving;
            _timer = 0f;

            if (_data.HomePosition != null)
            {
                _agent.SetDestination((Vector3)_data.HomePosition);
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
