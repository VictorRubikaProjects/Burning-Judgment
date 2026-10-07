using UnityEngine;

public class SniperAimVisual : MonoBehaviour
{
    [SerializeField] private LineRenderer m_leftLine;
    [SerializeField] private LineRenderer m_rightLine;
    [SerializeField] private Color m_trackingColor = Color.yellow;
    [SerializeField] private Color m_lockedColor = Color.red;

    private float m_coneAngle;
    private float m_range;
    private LayerMask m_blockMask;

    private void Awake()
    {
        SetupLine(m_leftLine);
        SetupLine(m_rightLine);
        Hide();
    }

    public void Configure(float coneAngle, float range, LayerMask blockMask)
    {
        m_coneAngle = coneAngle;
        m_range = range;
        m_blockMask = blockMask;
    }

    public void Show()
    {
        SetLocked(false);
        m_leftLine.enabled = true;
        m_rightLine.enabled = true;
    }

    public void Hide()
    {
        m_leftLine.enabled = false;
        m_rightLine.enabled = false;
    }

    public void SetLocked(bool locked)
    {
        Color color = locked ? m_lockedColor : m_trackingColor;
        ApplyColor(m_leftLine, color);
        ApplyColor(m_rightLine, color);
    }

    public void UpdateAim(Vector3 origin, Vector3 forward, float progress)
    {
        float halfAngle = m_coneAngle * 0.5f * (1f - Mathf.Clamp01(progress));

        DrawLine(m_leftLine, origin, Quaternion.AngleAxis(-halfAngle, Vector3.up) * forward);
        DrawLine(m_rightLine, origin, Quaternion.AngleAxis(halfAngle, Vector3.up) * forward);
    }

    private void DrawLine(LineRenderer line, Vector3 origin, Vector3 direction)
    {
        float length = m_range;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, m_range, m_blockMask, QueryTriggerInteraction.Ignore))
            length = hit.distance;

        line.SetPosition(0, origin);
        line.SetPosition(1, origin + direction * length);
    }

    private static void SetupLine(LineRenderer line)
    {
        line.useWorldSpace = true;
        line.positionCount = 2;
    }

    private static void ApplyColor(LineRenderer line, Color color)
    {
        line.startColor = color;
        line.endColor = color;
    }
}