using UnityEngine;

public class GroundCheckComponent : ActorComponent
{
    private readonly Transform m_transform;
    private readonly LayerMask m_groundLayer;
    private readonly float m_checkHeight;
    private readonly float m_checkDistance;

    public GroundCheckComponent(Actor owner, Transform transform, LayerMask groundLayer, float checkHeight, float checkDistance) : base(owner)
    {
        m_transform = transform;
        m_groundLayer = groundLayer;
        m_checkHeight = checkHeight;
        m_checkDistance = checkDistance;
    }

    public bool IsGrounded() => IsGrounded(m_transform.position);

    public bool IsGrounded(Vector3 position)
    {
        Vector3 origin = position + Vector3.up * m_checkHeight;
        return Physics.Raycast(origin, Vector3.down, m_checkDistance, m_groundLayer);
    }

    public override void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        base.OnDrawGizmosSelected();
        
        Vector3 origin = m_transform.position + Vector3.up * m_checkHeight;
        bool grounded = IsGrounded();

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * m_checkDistance);
        Gizmos.DrawWireSphere(origin, 0.1f);
    }
}