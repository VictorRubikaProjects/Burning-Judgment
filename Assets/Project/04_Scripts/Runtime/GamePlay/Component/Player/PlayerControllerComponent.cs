using Event_Bus;
using UnityEngine;

public class PlayerControllerComponent : ActorComponent
{
    private readonly Rigidbody m_rb;
    private readonly PlayerCharacter m_pc;
    private readonly SO_PlayerStats m_stats;
    
    private Quaternion m_targetRotation;
    
    private readonly Collider[] m_strikeHits = new Collider[8];
    
    public enum StrikeType
    {
        Roll, Dash
    }

    public PlayerControllerComponent(Actor owner, Rigidbody rb, SO_PlayerStats stats) : base(owner)
    {
        m_rb = rb;
        m_pc = (PlayerCharacter)owner;
        m_stats = stats;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleRotation();
    }

    public override void Update()
    {
        base.Update();
        CalculateRotation();
    }

    private void CalculateRotation()
    {
        if (m_pc.MoveDir == Vector3.zero) return;
        m_targetRotation = Quaternion.LookRotation(m_pc.MoveDir);
    }

    private void HandleRotation()
    {
        if (m_targetRotation == Quaternion.identity) return;
        m_targetRotation.Normalize();
        m_rb.MoveRotation(m_targetRotation);
    }
    
    public StrikeResult TryStrike(float radius, LayerMask layers,StrikeType strikeType, float forceMultiplier = 1)
    {
        Vector3 center = m_pc.TransformCache.position;
        Vector3 forward = m_pc.MoveDir;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            radius,
            m_strikeHits,
            layers,
            QueryTriggerInteraction.Ignore);
        
        float force = strikeType is  StrikeType.Dash ? m_stats.PushForce : m_stats.PushForce/2;
        
        StrikeContext context = new(forward, force * forceMultiplier,m_stats.Destabilization);

        for (int i = 0; i < count; i++)
        {
            Collider hit = m_strikeHits[i];

            if (!IsInFront(hit, center, forward)) continue;
            if (!hit.TryGetComponent(out IStrikeable target)) continue;

            StrikeResult result = target.OnStruck(context);

            if (result.Accepted) return result;
        }

        return StrikeResult.Ignored;
    }

    private static bool IsInFront(Collider collider, Vector3 origin, Vector3 forward)
    {
        Vector3 toClosest = collider.ClosestPoint(origin) - origin;
        toClosest.y = 0f;

        if (toClosest.sqrMagnitude < 0.0001f) return true;

        return Vector3.Dot(toClosest, forward) > 0f;
    }
}