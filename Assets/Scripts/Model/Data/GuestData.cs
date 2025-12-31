using UnityEngine;

namespace SkiGame.Model.Guest
{
    public struct GuestData
    {
        public Vector3 Position;
        public Vector3? HomePosition;
        public GuestState State;
        public ushort Money;
    }
}
