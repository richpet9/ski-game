using UnityEngine;

namespace SkiGame.View.UI
{
    [RequireComponent(typeof(LineRenderer))]
    public sealed class BoxCursorView : MonoBehaviour
    {
        [SerializeField]
        private float _size = 1f;

        private void Start()
        {
            LineRenderer line = GetComponent<LineRenderer>();

            // Ensure standard settings for a simple box.
            line.useWorldSpace = false;
            line.loop = true;
            line.positionCount = 4;

            // Define the 4 corners of the square on the XZ plane (Local Space).
            float s = _size * 0.5f;

            line.SetPosition(0, new Vector3(-s, 0, -s)); // Bottom Left.
            line.SetPosition(1, new Vector3(-s, 0, s)); // Top Left.
            line.SetPosition(2, new Vector3(s, 0, s)); // Top Right.
            line.SetPosition(3, new Vector3(s, 0, -s)); // Bottom Right.
        }
    }
}
