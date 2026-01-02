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
        private const float WANDER_WAIT_TIME = 2f;
        private const float LODGE_WAIT_TIME = 3f;

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
            _tickManager.Unregister(this);
        }

        public void Tick(float deltaTime)
        {
            if (QueuedForDestruction)
            {
                return;
            }

            switch (Data.State)
            {
                case GuestState.WalkingToLodge:
                    HandleWalkingToLodge();
                    break;
                case GuestState.InsideLodge:
                    HandleInsideLodge(deltaTime);
                    break;
                case GuestState.Leaving:
                    HandleLeaving();
                    break;
                case GuestState.Wandering:
                    HandleWandering(deltaTime);
                    break;
            }
        }

        public void TickLong(float deltaTime) { }

        public void TickRare(float deltaTime) { }

        private void HandleWalkingToLodge()
        {
            if (Data.RemainingDistance <= 1.5f)
            {
                EnterLodge();
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

        private void HandleLeaving()
        {
            if (Data.RemainingDistance <= 0.5f)
            {
                QueuedForDestruction = true;
            }
        }

        private void HandleWandering(float deltaTime)
        {
            Debug.Log("Remaining distance: " + Data.RemainingDistance);
            if (Data.RemainingDistance <= 0.5f)
            {
                Debug.Log("Incr. timer");
                _timer += deltaTime;
                if (_timer >= WANDER_WAIT_TIME)
                {
                    Debug.Log("Set new destination.");
                    SetNewDestination();
                }
            }
        }

        private void SetNewDestination()
        {
            _timer = 0f;

            if (_map.Structures.Lodges.Count > 0)
            {
                var targetGrid = _map.Structures.Lodges[
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
            _map.Economy.AddMoney(15);
        }

        private void ExitLodge()
        {
            Data.State = GuestState.Leaving;
            Data.IsVisible = true;
            _timer = 0f;

            if (Data.HomePosition.HasValue)
            {
                Data.TargetPosition = Data.HomePosition.Value;
            }
            else
            {
                Data.State = GuestState.Wandering;
                SetRandomDestination();
            }
        }
    }
}
