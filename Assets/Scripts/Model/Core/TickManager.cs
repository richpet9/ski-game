using System.Collections.Generic;

namespace SkiGame.Model.Core
{
    public sealed class TickManager
    {
        private const byte TICKS_FOR_LONG_TICK = 5;
        private const byte TICKS_FOR_RARE_TICK = 20;

        private readonly List<ITickable> _tickables = new List<ITickable>();
        private byte _ticks = 0;

        public void Register(ITickable tickable) => _tickables.Add(tickable);

        public void Unregister(ITickable tickable) => _tickables.Remove(tickable);

        public void Tick(float deltaTime)
        {
            for (int i = _tickables.Count - 1; i >= 0; i--)
            {
                ITickable tickable = _tickables[i];

                tickable.Tick(deltaTime);

                if (_ticks % TICKS_FOR_LONG_TICK == 0)
                {
                    tickable.TickLong(deltaTime * TICKS_FOR_LONG_TICK);
                }

                if (_ticks % TICKS_FOR_RARE_TICK == 0)
                {
                    tickable.TickRare(deltaTime * TICKS_FOR_RARE_TICK);
                }
            }

            _ticks++;
            if (_ticks >= TICKS_FOR_RARE_TICK)
            {
                _ticks = 0;
            }
        }
    }
}
