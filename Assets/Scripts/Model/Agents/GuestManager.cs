using System;

namespace SkiGame.Model.Agents
{
    public sealed class GuestManager
    {
        public ushort GuestCount { get; private set; }
        public event Action<ushort> OnGuestCountChanged;

        public void AddGuest()
        {
            GuestCount++;
            OnGuestCountChanged(GuestCount);
        }

        public void RemoveGuest()
        {
            GuestCount--;
            OnGuestCountChanged(GuestCount);
        }
    }
}
