using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyShooterHitState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    
    private EnemyShooter m_owner;
    private Rigidbody m_rb;
    
    public EnemyShooterHitState(EnemyShooter owner, Rigidbody rb)
    {
        m_owner = owner;
        m_rb = rb;
    }

    public override void OnEnter()
    {
        base.OnEnter();

        IsFinished = false;
        m_owner.Agent.enabled = false;
        
        m_owner.ConsumeHitRequest();

        m_owner.KnockBack.HitAsync(
            m_owner.PendingKnockbackDirection,
            m_owner.Stats.HitDuration,
            m_owner.PendingKnockbackForce,
            m_owner.Stats.HitKnockbackCurve).Forget();
    }

    public override void OnExit()
    {
        base.OnExit();
        m_owner.KnockBack.Cancel();

        if (m_owner.GroundCheck.IsGrounded())
        {
            m_owner.Agent.Warp(m_owner.Rigidbody.position);
        }
        m_owner.Agent.enabled = m_owner.GroundCheck.IsGrounded();
    }
    
    public override void Update()
    {
        base.Update();
        if (!m_owner.KnockBack.IsActive)
        {
            IsFinished = true;
        }
    }
    
}