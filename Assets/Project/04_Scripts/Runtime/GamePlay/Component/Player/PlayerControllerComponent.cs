using Event_Bus;
using Project._05_Scripts.Runtime.System.ServiceLocator.Services;
using UnityEngine;

public class PlayerControllerComponent : ActorComponent
{
    private readonly Rigidbody m_rb;
    private readonly PlayerCharacter m_pc;
    private readonly SO_PlayerStats m_stats;
    
    private Quaternion m_targetRotation;
    
    private readonly Collider[] m_strikeHits = new Collider[8];

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
    
    public bool TryStrike()
    {
        Vector3 center = m_pc.TransformCache.position;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            m_stats.AttackStrikeRadius,
            m_strikeHits,
            m_stats.EnemyLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (!m_strikeHits[i].TryGetComponent(out EnemyBase enemy)) continue;

            EventBus<ActorPushedEvent>.Raise(
                new ActorPushedEvent(
                    enemy,
                    m_pc.MoveDir,
                    m_stats.PushForce));
            
            return true;
        }

        return false;
    }
}