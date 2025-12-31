using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LineRenderer))]
public class GuestDebugger : MonoBehaviour
{
    private NavMeshAgent _agent;
    private LineRenderer _line;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _line = GetComponent<LineRenderer>();
        _line.startWidth = 0.5f;
        _line.endWidth = 0.5f;
        _line.material = new Material(Shader.Find("Sprites/Default"));
        _line.startColor = Color.red;
        _line.endColor = Color.red;
    }

    private void Update()
    {
        if (_agent.hasPath)
        {
            _line.positionCount = _agent.path.corners.Length;
            _line.SetPositions(_agent.path.corners);

            // Draw the link connection if traversing
            if (_agent.isOnOffMeshLink)
            {
                Debug.DrawLine(
                    transform.position,
                    _agent.currentOffMeshLinkData.endPos,
                    Color.green
                );
            }
        }
    }

    // Call this via a button or context menu to verify cost
    [ContextMenu("Log Path Details")]
    public void LogPathDetails()
    {
        if (_agent.pathStatus == NavMeshPathStatus.PathComplete)
        {
            Debug.Log(
                $"Path Found! Corners: {_agent.path.corners.Length} Status: {_agent.pathStatus}"
            );
        }
        else
        {
            Debug.LogWarning($"Path Invalid/Partial. Status: {_agent.pathStatus}");
        }
    }
}
