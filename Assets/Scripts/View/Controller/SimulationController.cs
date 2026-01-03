// View/Controller/SimulationDriver.cs
using SkiGame.Model.Core;
using UnityEngine;

namespace SkiGame.View.Controller
{
    public class SimulationController : MonoBehaviour
    {
        private const float TICK_RATE = 0.05f; // Every 50ms.

        private float _accumulator;
        private float _gameSpeed = 1f; // Modify this to 2f, 5f to speed up game!
        private TickManager _tickManager;

        private void Start()
        {
            _tickManager = GameContext.Get<TickManager>();
        }

        private void Update()
        {
            // Add frame time to the "bucket".
            _accumulator += Time.deltaTime * _gameSpeed;

            // Consume time from the bucket in fixed chunks.
            while (_accumulator >= TICK_RATE)
            {
                _tickManager.Tick(TICK_RATE);
                _accumulator -= TICK_RATE;
            }
        }
    }
}
