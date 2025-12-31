using System;

namespace SkiGame.Model.Agents
{
    public class GuestManager
    {
        public static ushort GuestCount { get; private set; }
        public static event Action<ushort> OnGuestCountChanged;

        public static void AddGuest()
        {
            GuestCount++;
            OnGuestCountChanged(GuestCount);
        }

        public static void RemoveGuest()
        {
            GuestCount--;
            OnGuestCountChanged(GuestCount);
        }
    }
}
