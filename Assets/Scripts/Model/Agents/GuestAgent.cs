using SkiGame.Model.AI;
using SkiGame.Model.Core;
using SkiGame.Model.Guest;
using SkiGame.Model.Services;
using SkiGame.Model.Structures;
using SkiGame.Model.Terrain;
using UnityEngine;

namespace SkiGame.Model.Agents
{
    public sealed class GuestAgent : ITickable
    {
        public GuestData Data { get; }
        public bool QueuedForDestruction { get; private set; }

        private const float WANDER_WAIT_TIME = 1f;
        private const float LODGE_WAIT_TIME = 3f;
        private const byte SKIING_ENERGY_COST = 3;
        private const byte WALKING_ENERGY_COST = 1;
        private const byte ENERGY_LEAVE_THRESHOLD = 67;
        private const byte LIFT_TICKET_PRICE = 15;
        private const float SKI_SPEED = 5f;
        private const float WALK_SPEED = 3.5f;
        private const float GRAVITY = 5f;
        private const float LIFT_SPEED = 10f;
        private const float ARRIVAL_THRESHOLD = 1.0f;

        private readonly Map _map;
        private readonly TickManager _tickManager;
        private readonly IntegrationFieldService _navService;

        private float _timer = 0f;

        public GuestAgent(GuestData data)
        {
            Data = data;
            _map = GameContext.Map;
            _navService = GameContext.Get<IntegrationFieldService>();
            _tickManager = GameContext.Get<TickManager>();

            _tickManager.Register(this);
            UpdateState();
        }

        public void Dispose()
        {
            _map.Guests.RemoveGuest();
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
                Data.State = GuestState.Leaving;
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
                    HandleMovement(deltaTime, SKI_SPEED);
                    break;

                case GuestState.WalkingToLodge:
                case GuestState.WalkingToLift:
                case GuestState.Wandering:
                case GuestState.Leaving:
                    HandleMovement(deltaTime, WALK_SPEED);
                    break;

                case GuestState.RidingLift:
                    HandleLift(deltaTime);
                    break;

                default:
                    break;
            }

            if (Data.IsVisible && Data.State != GuestState.RidingLift)
            {
                ApplyGravity(deltaTime);
            }
        }

        public void TickLong(float deltaTime)
        {
            ApplyEnergyCosts();
        }

        public void TickRare(float deltaTime) { }

        private void HandleMovement(float deltaTime, float speed)
        {
            NavigationGoal goal = GetGoalFromState();

            // If the current state has no goal, we don't move.
            if (goal == NavigationGoal.None)
            {
                return;
            }

            Vector2 direction = _navService.GetDirection(Data.Position, goal);
            Vector3 moveDir = new Vector3(direction.x, 0, direction.y);

            if (moveDir.sqrMagnitude < 0.01f)
            {
                NotifyArrival();
            }
            else
            {
                Data.Position += deltaTime * speed * moveDir;
                Data.Rotation = Quaternion.LookRotation(moveDir);
            }
        }

        private NavigationGoal GetGoalFromState()
        {
            return Data.State switch
            {
                GuestState.WalkingToLift => NavigationGoal.LiftEntrance,
                GuestState.WalkingToLodge => NavigationGoal.Lodge,
                GuestState.Skiing => NavigationGoal.ParkingLot,
                GuestState.Leaving => NavigationGoal.ParkingLot,
                GuestState.Wandering => NavigationGoal.Wander,
                _ => NavigationGoal.None,
            };
        }

        private void HandleLift(float deltaTime)
        {
            if (!Data.TargetPosition.HasValue)
            {
                return;
            }

            Vector3 target = Data.TargetPosition.Value;
            Vector3 dir = (target - Data.Position).normalized;
            Data.Position += deltaTime * LIFT_SPEED * dir;

            if (Vector3.Distance(Data.Position, target) < ARRIVAL_THRESHOLD)
            {
                NotifyArrival();
            }
        }

        private void ApplyGravity(float deltaTime)
        {
            Vector2Int gridPos = MapUtil.WorldToGrid(Data.Position);
            float terrainHeight = _map.GetTile(gridPos).Height;

            if (Data.Position.y > terrainHeight + 0.05f)
            {
                Data.Position += deltaTime * GRAVITY * Vector3.down;
            }
            else
            {
                Data.Position = new Vector3(Data.Position.x, terrainHeight, Data.Position.z);
            }
        }

        private void HandleWaiting(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= WANDER_WAIT_TIME)
            {
                UpdateState();
            }
        }

        private void HandleInsideLodge(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= LODGE_WAIT_TIME)
            {
                Data.IsVisible = true;
                _timer = 0f;
                UpdateState();
            }
        }

        private void NotifyArrival()
        {
            switch (Data.State)
            {
                case GuestState.WalkingToLodge:
                    EnterLodge();
                    break;

                case GuestState.WalkingToLift:
                    RideLift();
                    break;

                case GuestState.RidingLift:
                    Data.State = GuestState.Skiing;
                    break;

                case GuestState.Skiing:
                    UpdateState();
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
                default:
                    break;
            }
        }

        private void UpdateState()
        {
            _timer = 0f;

            if (_map.Structures.Lifts.Count > 0)
            {
                Data.State = GuestState.WalkingToLift;
            }
            else if (_map.Structures.Structures[StructureType.Lodge].Count > 0)
            {
                Data.State = GuestState.WalkingToLodge;
            }
            else
            {
                Data.State = GuestState.Wandering;
            }
        }

        private void RideLift()
        {
            Vector2Int currentGrid = MapUtil.WorldToGrid(Data.Position);

            foreach (StructureManager.Lift lift in _map.Structures.Lifts)
            {
                if (Vector2Int.Distance(lift.StartGrid, currentGrid) < 4)
                {
                    Data.State = GuestState.RidingLift;
                    float endHeight = _map.GetTile(lift.EndGrid).Height;
                    Data.TargetPosition = MapUtil.GridToWorld(lift.EndGrid, endHeight);
                    _map.Economy.AddMoney(LIFT_TICKET_PRICE);
                    return;
                }
            }

            Data.State = GuestState.Skiing;
        }

        private void EnterLodge()
        {
            Data.State = GuestState.InsideLodge;
            Data.IsVisible = false;
            _timer = 0f;
            _map.Economy.AddMoney(LIFT_TICKET_PRICE);
        }

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
                case GuestState.RidingLift:
                case GuestState.InsideLodge:
                case GuestState.Leaving:
                default:
                    break;
            }
        }
    }
}
