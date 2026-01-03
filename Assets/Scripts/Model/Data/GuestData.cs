using UnityEngine;

namespace SkiGame.Model.Guest
{
    public class GuestData
    {
        public bool IsVisible = true;
        public Vector3 Position;
        public Vector3? HomePosition;
        public Vector3? TargetPosition;
        public GuestState State;
        public Color Color;
        public ushort Money;
        public byte Energy;
        public Quaternion Rotation;
    }
}
