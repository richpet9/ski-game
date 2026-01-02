using System.Collections.Generic;

namespace SkiGame.Model.Core
{
    public sealed class TickManager
    {
        private const byte TICK_LONG_FREQUENCY = 127;
        private const byte TICK_RARE_FREQUENCY = 255;

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

                if (_ticks % TICK_LONG_FREQUENCY == 0)
                {
                    tickable.TickLong(deltaTime);
                }

                if (_ticks % TICK_RARE_FREQUENCY == 0)
                {
                    tickable.TickRare(deltaTime);
                }
            }

            _ticks = (byte)((_ticks + 1) % 256);
        }
    }
}
