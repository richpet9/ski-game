using System;

namespace SkiGame.Model.Agents
{
    public class GuestManager
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
