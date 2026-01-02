namespace SkiGame.Model.Core
{
    public interface ITickable
    {
        public void Tick(float deltaTime);
        public void TickLong(float deltaTime);
        public void TickRare(float deltaTime);
    }
}
