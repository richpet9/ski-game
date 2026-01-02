// View/Controller/SimulationDriver.cs
using SkiGame.Model.Core;
using UnityEngine;

namespace SkiGame.View.Controller
{
    public class SimulationDriver : MonoBehaviour
    {
        // 20 Ticks per second = 50ms per tick.
        private const float TICK_RATE = 0.05f;
        private float _accumulator;
        private TickManager _tickManager;

        public float GameSpeed = 1f; // Modify this to 2f, 5f to speed up game!

        private void Start()
        {
            _tickManager = GameContext.Get<TickManager>();
        }

        private void Update()
        {
            // Add frame time to the "bucket".
            _accumulator += Time.deltaTime * GameSpeed;

            // Consume time from the bucket in fixed chunks.
            while (_accumulator >= TICK_RATE)
            {
                _tickManager.Tick(TICK_RATE);
                _accumulator -= TICK_RATE;
            }
        }
    }
}
