using UnityEngine;

namespace SkiGame.Model.Guest
{
    public class GuestData
    {
        public Vector3 Position;
        public Vector3? HomePosition;
        public Vector3? TargetPosition;
        public GuestState State;
        public ushort Money;
        public bool IsVisible = true;
    }
}
