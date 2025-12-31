using SkiGame.Model.Core;
using SkiGame.Model.Guest;
using UnityEngine;
using UnityEngine.AI;

namespace SkiGame.Model.Agents
{
    public class GuestAgent
    {
        public GuestData Data { get; }
        public bool QueuedForDestruction { get; private set; }

        private const float WANDER_RADIUS = 128f;
        private const float WANDER_WAIT_TIME = 2f;
        private const float LODGE_WAIT_TIME = 3f;

        private float _timer;
        private float _remainingDistance;

        public GuestAgent(GuestData data)
        {
            Data = data;
            SetNewDestination();
        }

        public void Tick(float deltaTime, Vector3 currentPosition, float remainingDistance)
        {
            Data.Position = currentPosition;
            _remainingDistance = remainingDistance;

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

        private void HandleWalkingToLodge()
        {
            if (_remainingDistance <= 1.5f)
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
            if (_remainingDistance <= 0.5f)
            {
                QueuedForDestruction = true;
            }
        }

        private void HandleWandering(float deltaTime)
        {
            if (_remainingDistance <= 0.5f)
            {
                _timer += deltaTime;
                if (_timer >= WANDER_WAIT_TIME)
                {
                    SetNewDestination();
                }
            }
        }

        private void SetNewDestination()
        {
            _timer = 0f;

            if (GameContext.Structures.Lodges.Count > 0)
            {
                var targetGrid = GameContext.Structures.Lodges[
                    Random.Range(0, GameContext.Structures.Lodges.Count)
                ];
                float y = GameContext.Map.GetTile(targetGrid).Height;
                // TODO: Use world coord converter.
                Data.TargetPosition = new Vector3(targetGrid.x + 0.5f, y, targetGrid.y + 0.5f);
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
            if (
                // TODO: This should not be referenced in this assmebly, try to find a
                // a way to store potential wander points in the model.
                NavMesh.SamplePosition(
                    randomPoint,
                    out NavMeshHit hit,
                    WANDER_RADIUS,
                    NavMesh.AllAreas
                )
            )
            {
                Data.TargetPosition = hit.position;
            }
        }

        private void EnterLodge()
        {
            Data.State = GuestState.InsideLodge;
            Data.TargetPosition = null;
            Data.IsVisible = false;
            _timer = 0f;
            GameContext.Economy.AddMoney(15);
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
