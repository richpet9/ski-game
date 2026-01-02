namespace SkiGame.Model.Guest
{
    public enum GuestState : byte
    {
        Waiting = 0,
        Wandering = 1,
        InsideLodge = 2,
        WalkingToLodge = 3,
        Leaving = 4,
        WalkingToLift = 5,
        RidingLift = 6,
        Skiing = 7,
    }
}
