namespace SkiGame.Model.Guest
{
    public enum GuestState : byte
    {
        Waiting = 0,
        Wandering = 1,
        WalkingToLift = 2,
        WalkingToLodge = 3,
        Skiing = 4,
        RidingLift = 5,
        InsideLodge = 6,
        Leaving = 7,
    }
}
