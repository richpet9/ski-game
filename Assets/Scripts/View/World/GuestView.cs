using SkiGame.Model.Agents;
using SkiGame.Model.Guest;
using UnityEngine;

namespace SkiGame.View.Agents
{
    public class GuestView : MonoBehaviour
    {
        [SerializeField]
        private float _smoothSpeed = 10f;

        private GuestAgent _agent;
        private MeshRenderer[] _renderers;

        public void Initialize(GuestData data)
        {
            _agent = new GuestAgent(data);
            _renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer r in _renderers)
            {
                r.material.color = data.Color;
            }

            transform.position = data.Position;
        }

        private void Update()
        {
            if (_agent == null)
            {
                return;
            }

            if (_agent.QueuedForDestruction)
            {
                _agent.Dispose();
                Destroy(gameObject);
                return;
            }

            // Visibility Toggle.
            bool visible = _agent.Data.IsVisible;
            if (_renderers.Length > 0 && _renderers[0].enabled != visible)
            {
                foreach (MeshRenderer r in _renderers)
                {
                    r.enabled = visible;
                }
            }

            if (!visible)
            {
                return;
            }

            transform.SetPositionAndRotation(
                Vector3.Lerp(
                    transform.position,
                    _agent.Data.Position,
                    Time.deltaTime * _smoothSpeed
                ),
                Quaternion.Slerp(
                    transform.rotation,
                    _agent.Data.Rotation,
                    Time.deltaTime * _smoothSpeed
                )
            );
        }
    }
}
