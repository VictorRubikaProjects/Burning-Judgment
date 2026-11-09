using UnityEngine;

public class SniperAimVisual : MonoBehaviour
{
    [SerializeField] private LineRenderer m_line;
    [SerializeField] private Color m_trackingColor = Color.yellow;
    [SerializeField] private Color m_lockedColor = Color.red;
    [SerializeField] private float m_blinkSpeed = 10f;

    private float m_range;
    private LayerMask m_blockMask;
    private bool m_isLocked;

    private void Awake()
    {
        m_line.useWorldSpace = true;
        m_line.positionCount = 2;
        Hide();
    }

    public void Configure(float range, LayerMask blockMask)
    {
        m_range = range;
        m_blockMask = blockMask;
    }

    public void Show()
    {
        SetLocked(false);
        m_line.enabled = true;
    }

    public void Hide() => m_line.enabled = false;

    public void SetLocked(bool locked)
    {
        m_isLocked = locked;
        if (!locked)
        {
            ApplyColor(m_trackingColor);
        }
    }

    public void UpdateAim(Vector3 origin, Vector3 forward)
    {
        if (!m_line.enabled) return;

        if (m_isLocked)
        {
            float t = Mathf.PingPong(Time.time * m_blinkSpeed, 1f);
            ApplyColor(Color.Lerp(m_lockedColor, Color.white, t));
        }

        float length = m_range;
        if (Physics.Raycast(origin, forward, out RaycastHit hit, m_range, m_blockMask, QueryTriggerInteraction.Ignore))
        {
            length = hit.distance;
        }

        m_line.SetPosition(0, origin);
        m_line.SetPosition(1, origin + forward.normalized * length);
    }

    private void ApplyColor(Color color)
    {
        m_line.startColor = color;
        m_line.endColor = color;
    }
}