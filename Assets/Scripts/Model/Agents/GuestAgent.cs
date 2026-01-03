using SkiGame.Model.Core;
using SkiGame.Model.Guest;
using SkiGame.Model.Services;
using SkiGame.Model.Structures;
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
        private const float SKI_SPEED = 12f;
        private const float WALK_SPEED = 3.5f;
        private const float GRAVITY = 5f;
        private const float LIFT_SPEED = 10f;
        private const float MINIMUM_TARGET_DIST = 0.5f;

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
                TryLeave();
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
                    HandleSkiing(deltaTime);
                    break;

                case GuestState.WalkingToLodge:
                case GuestState.WalkingToLift:
                case GuestState.Wandering:
                case GuestState.Leaving:
                    HandleWalking(deltaTime);
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

        public void BeginLiftTraversal()
        {
            if (Data.State == GuestState.WalkingToLift)
            {
                Data.State = GuestState.RidingLift;
            }
        }

        private void HandleSkiing(float deltaTime)
        {
            Vector2 flow = _navService.GetFlow(Data.Position);
            Vector3 moveDir = new Vector3(flow.x, 0, flow.y);

            if (moveDir.sqrMagnitude < 0.01f)
            {
                NotifyArrival();
            }
            else
            {
                Data.Position += moveDir * SKI_SPEED * deltaTime;
                Data.Rotation = Quaternion.LookRotation(moveDir);
            }
        }

        private void HandleWalking(float deltaTime)
        {
            if (!Data.TargetPosition.HasValue)
                return;

            // Get next corner from NavMesh (without being attached to it).
            Vector3 nextStep = _navService.GetNextPathPosition(
                Data.Position,
                Data.TargetPosition.Value
            );

            MoveTowards(nextStep, WALK_SPEED, deltaTime);

            if (Vector3.Distance(Data.Position, Data.TargetPosition.Value) < MINIMUM_TARGET_DIST)
            {
                NotifyArrival();
            }
        }

        private void HandleLift(float deltaTime)
        {
            if (!Data.TargetPosition.HasValue)
                return;

            // Fly straight to target (Lift Top)
            MoveTowards(Data.TargetPosition.Value, LIFT_SPEED, deltaTime);

            if (Vector3.Distance(Data.Position, Data.TargetPosition.Value) < MINIMUM_TARGET_DIST)
            {
                NotifyArrival();
            }
        }

        private void MoveTowards(Vector3 target, float speed, float dt)
        {
            Vector3 dir = (target - Data.Position).normalized;
            Data.Position += dt * speed * dir;

            // Rotate (Keep Y flat).
            Vector3 lookDir = new Vector3(dir.x, 0, dir.z);
            if (lookDir.sqrMagnitude > 0.001f)
            {
                Data.Rotation = Quaternion.LookRotation(lookDir);
            }
        }

        private void ApplyGravity(float deltaTime)
        {
            float terrainHeight = _map.GetTile(
                Mathf.RoundToInt(Data.Position.x),
                Mathf.RoundToInt(Data.Position.z)
            ).Height;

            // Anti-Jiggle: Only fall if significantly above ground.
            if (Data.Position.y > terrainHeight + 0.05f)
            {
                Data.Position += deltaTime * GRAVITY * Vector3.down;
            }
            else
            {
                // Snap to ground.
                Data.Position = new Vector3(Data.Position.x, terrainHeight, Data.Position.z);
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
                    throw new System.Exception(
                        $"Guest in Waiting state should not call NotifyArrival."
                    );

                case GuestState.InsideLodge:
                    throw new System.Exception(
                        $"Guest in InsideLodge state should not call NotifyArrival."
                    );

                default:
                    break;
            }
        }

        private float GetTerrainHeight(Vector3 pos)
        {
            Vector2Int gridPos = MapUtil.WorldToGrid(pos);
            if (_map.InBounds(gridPos))
            {
                return _map.GetTile(gridPos).Height;
            }
            return 0;
        }

        private void SetNewDestination()
        {
            _timer = 0f;

            // 1. Priority: Lifts.
            if (_map.Structures.Lifts.Count > 0)
            {
                StructureManager.Lift targetLift = _map.Structures.Lifts[
                    Random.Range(0, _map.Structures.Lifts.Count)
                ];

                // Target the BASE of the lift.
                Vector2Int baseGrid = targetLift.StartGrid;
                float y = _map.GetTile(baseGrid).Height;

                Data.TargetPosition = MapUtil.GridToWorld(baseGrid, y);
                Data.State = GuestState.WalkingToLift;
            }
            // 2. Priority: Food/Lodge.
            else if (_map.Structures.Structures[StructureType.Lodge].Count > 0)
            {
                Vector2Int targetGrid = _map.Structures.Structures[StructureType.Lodge][
                    Random.Range(0, _map.Structures.Structures[StructureType.Lodge].Count)
                ];
                float y = _map.GetTile(targetGrid).Height;
                Data.TargetPosition = MapUtil.GridToWorld(targetGrid, y);
                Data.State = GuestState.WalkingToLodge;
            }
            // 3. Fallback: Wander.
            else
            {
                SetRandomDestination();
                Data.State = GuestState.Wandering;
            }
        }

        private void RideLift()
        {
            // Logic: Find the lift we are standing near and target its EndGrid.
            Vector2Int currentGrid = new Vector2Int(
                Mathf.RoundToInt(Data.Position.x),
                Mathf.RoundToInt(Data.Position.z)
            );

            foreach (StructureManager.Lift lift in _map.Structures.Lifts)
            {
                // Check proximity to start base.
                if (Vector2Int.Distance(lift.StartGrid, currentGrid) < 4)
                {
                    Data.State = GuestState.RidingLift;

                    float endY = _map.GetTile(lift.EndGrid).Height;
                    Data.TargetPosition = MapUtil.GridToWorld(lift.EndGrid, endY);

                    _map.Economy.AddMoney(LIFT_TICKET_PRICE); // Optional: Charge for lift
                    return;
                }
            }

            // Failed to find lift? Just ski.
            Data.State = GuestState.Skiing;
        }

        private void SetSkiingDestination()
        {
            // When skiing, we don't really have a single target since we follow Flow
            // Fields. However, if we need to "aim" for the bottom, we can target a
            // parking lot. For now, this is mostly symbolic as Flow Field overrides
            // direction.
            if (_map.Structures.Structures[StructureType.ParkingLot].Count > 0)
            {
                Vector2Int gridPos = _map.Structures.Structures[StructureType.ParkingLot][0];
                float y = _map.GetTile(gridPos).Height;
                Data.TargetPosition = MapUtil.GridToWorld(gridPos, y);
            }
        }

        private void SetRandomDestination()
        {
            Vector2 randomCircle = Random.insideUnitCircle * WANDER_RADIUS;
            Vector3 randomPoint = Data.Position + new Vector3(randomCircle.x, 0, randomCircle.y);
            if (_navService.SamplePosition(randomPoint, out Vector3 hitPoint, WANDER_RADIUS))
            {
                Data.TargetPosition = hitPoint;
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
