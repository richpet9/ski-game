using System.Numerics;

namespace SkiGame.Model.Guest
{
    public enum GuestState : byte
    {
        Wandering = 0,
        InsideLodge = 1,
        WalkingToLodge = 2,
        Leaving = 3,
    }
}
