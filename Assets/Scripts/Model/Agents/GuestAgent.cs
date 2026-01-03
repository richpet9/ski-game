using SkiGame.Model.Core;
using SkiGame.Model.Guest;
using SkiGame.Model.Services;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.Model.Agents
{
    public class GuestAgent : ITickable
    {
        public GuestData Data { get; }
        public bool QueuedForDestruction { get; private set; }

        private const float WANDER_RADIUS = 12f;
        private const float WANDER_WAIT_TIME = 1f;
        private const float LODGE_WAIT_TIME = 3f;
        private const byte SKIING_ENERGY_COST = 3;
        private const byte WALKING_ENERGY_COST = 1;
        private const byte ENERGY_LEAVE_THRESHOLD = 67;
        private const byte LIFT_TICKET_PRICE = 15;

        private readonly Map _map;
        private readonly TickManager _tickManager;
        private readonly INavigationService _navService;
        private float _timer = 0f;

        public GuestAgent(GuestData data)
        {
            Data = data;
            _map = GameContext.Map;
            _navService = GameContext.Get<INavigationService>();
            _tickManager = GameContext.Get<TickManager>();
            _tickManager.Register(this);
            SetNewDestination();
        }

        public void Dispose()
        {
            GameContext.Map.Guests.RemoveGuest();
            _tickManager.Unregister(this);
        }

        public void Tick(float deltaTime)
        {
            if (QueuedForDestruction)
            {
                return;
            }

            if (Data.Energy <= ENERGY_LEAVE_THRESHOLD && Data.State != GuestState.Leaving)
            {
                TryLeave();
                return;
            }

            switch (Data.State)
            {
                case GuestState.InsideLodge:
                    HandleInsideLodge(deltaTime);
                    break;

                case GuestState.Waiting:
                    HandleWaiting(deltaTime);
                    break;

                case GuestState.Skiing:
                case GuestState.WalkingToLodge:
                case GuestState.RidingLift:
                case GuestState.Leaving:
                case GuestState.Wandering:
                case GuestState.WalkingToLift:
                default:
                    break;
            }
        }

        public void TickLong(float deltaTime)
        {
            ApplyEnergyCosts();
        }

        public void TickRare(float deltaTime) { }

        private void ApplyEnergyCosts()
        {
            if (Data.Energy <= 0)
            {
                Data.Energy = 0;
                return;
            }

            switch (Data.State)
            {
                case GuestState.Skiing:
                    Data.Energy -= SKIING_ENERGY_COST;
                    break;

                case GuestState.Wandering:
                case GuestState.WalkingToLift:
                    Data.Energy -= WALKING_ENERGY_COST;
                    break;

                case GuestState.Waiting:
                case GuestState.WalkingToLodge:
                case GuestState.InsideLodge:
                case GuestState.RidingLift:
                case GuestState.Leaving:
                default:
                    break;
            }
        }

        public void BeginLiftTraversal()
        {
            if (Data.State == GuestState.WalkingToLift)
            {
                Data.State = GuestState.RidingLift;
            }
        }

        private void HandleInsideLodge(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= LODGE_WAIT_TIME)
            {
                ExitLodge();
            }
        }

        private void HandleWaiting(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= WANDER_WAIT_TIME)
            {
                SetNewDestination();
            }
        }

        public void NotifyArrival()
        {
            switch (Data.State)
            {
                case GuestState.WalkingToLodge:
                    EnterLodge();
                    break;

                case GuestState.RidingLift:
                    // This is called when the agent reaches the end of the lift path.
                    // The state was set to RidingLift by the GuestView's TraverseLift coroutine.
                    SetSkiingDestination();
                    Data.State = GuestState.Skiing;
                    break;

                case GuestState.Skiing:
                    SetNewDestination();
                    break;

                case GuestState.Leaving:
                    QueuedForDestruction = true;
                    break;

                case GuestState.Wandering:
                    Data.State = GuestState.Waiting;
                    _timer = 0f;
                    break;

                case GuestState.Waiting:
                case GuestState.InsideLodge:
                case GuestState.WalkingToLift:
                default:
                    break;
            }
        }

        private void SetNewDestination()
        {
            _timer = 0f;

            // For now, let's prioritize skiing if possible.
            // This assumes StructureManager has a public List<Lift> Lifts.
            // A more complex decision-making process can be added later.
            if (_map.Structures.Lifts.Count > 0)
            {
                var targetLift = _map.Structures.Lifts[
                    Random.Range(0, _map.Structures.Lifts.Count)
                ];
                // The destination is the END of the lift. The pathfinder will route
                // the agent to the start and across the NavMeshLink.
                var targetGrid = targetLift.EndGrid;
                float y = _map.GetTile(targetGrid).Height;
                Data.TargetPosition = MapUtil.GridToWorld(targetGrid, y);
                Data.State = GuestState.WalkingToLift;
            }
            else if (_map.Structures.Lodges.Count > 0)
            {
                Vector2Int targetGrid = _map.Structures.Lodges[
                    Random.Range(0, _map.Structures.Lodges.Count)
                ];
                float y = _map.GetTile(targetGrid).Height;
                Data.TargetPosition = MapUtil.GridToWorld(targetGrid, y);
                Data.State = GuestState.WalkingToLodge;
            }
            else
            {
                SetRandomDestination();
                Data.State = GuestState.Wandering;
            }
        }

        private void SetSkiingDestination()
        {
            // After getting off a lift, ski towards a parking lot to simulate skiing to the base.
            // This assumes StructureManager has a public List<Vector2Int> ParkingLots.
            if (_map.Structures.Lodges.Count > 0)
            {
                Vector2Int gridPos = _map.Structures.Lodges[
                    Random.Range(0, _map.Structures.Lodges.Count)
                ];
                float y = _map.GetTile(gridPos).Height;
                Data.TargetPosition = MapUtil.GridToWorld(gridPos, y);
            }
            else
            {
                SetRandomDestination();
            }
        }

        private void SetRandomDestination()
        {
            Vector3 randomPoint = Data.Position + (Random.insideUnitSphere * WANDER_RADIUS);
            if (_navService.SamplePosition(randomPoint, out Vector3 hitPoint, WANDER_RADIUS))
            {
                Data.TargetPosition = hitPoint;
            }
        }

        private void EnterLodge()
        {
            Data.State = GuestState.InsideLodge;
            Data.TargetPosition = null;
            Data.IsVisible = false;
            _timer = 0f;
            _map.Economy.AddMoney(LIFT_TICKET_PRICE);
        }

        private void ExitLodge()
        {
            Data.IsVisible = true;
            _timer = 0f;
            TryLeave();
        }

        private void TryLeave()
        {
            Data.State = GuestState.Leaving;
            if (Data.HomePosition.HasValue)
            {
                Data.TargetPosition = Data.HomePosition.Value;
            }
            else
            {
                Debug.Log("Guest cannot leave!");
                Data.State = GuestState.Wandering;
                SetRandomDestination();
            }
        }
    }
}
