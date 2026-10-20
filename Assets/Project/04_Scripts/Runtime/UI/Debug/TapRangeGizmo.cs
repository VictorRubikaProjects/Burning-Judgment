using UnityEngine;

public class TapRangeGizmo : MonoBehaviour
{
    [SerializeField] private SO_PlayerStats m_stats;
    [SerializeField] private Color m_color = Color.cyan;

    private void OnDrawGizmos()
    {
        if (m_stats == null) return;

        Gizmos.color = m_color;
        Gizmos.DrawWireSphere(transform.position, m_stats.MaxTapDistance);
    }
}