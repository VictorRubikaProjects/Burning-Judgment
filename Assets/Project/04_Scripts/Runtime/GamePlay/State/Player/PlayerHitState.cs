using Cysharp.Threading.Tasks;
using Project._05_Scripts.Runtime.System.ServiceLocator.Services;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    private bool m_isFinished;

    public bool IsFinished => m_isFinished;

    public PlayerHitState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats, playerStats.HitAnimation)
    { }

    public override void OnEnter()
    {
        base.OnEnter();

        m_isFinished = false;

        m_owner.ConsumeHitRequest();
        m_owner.Aspect.HurtVisuals();

        PlayAnimation(HitHash);

        m_owner.HitGate.TakeDamage();

        m_owner.KnockBack.HitAsync(
            m_owner.PendingKnockbackDirection,
            m_stats.HitDuration,
            m_owner.PendingKnockbackForce,
            m_stats.HitKnockbackCurve,
            m_stats.ObstacleLayer).Forget();
        
        ServiceLocator.Get<FxService>().PlayEjectionEffect(m_owner.TransformCache,m_stats.HitDuration);
    }

    public override void Update()
    {
        base.Update();

        if (!m_owner.KnockBack.IsActive) m_isFinished = true;
    }

    public override void OnExit()
    {
        base.OnExit();
        m_owner.KnockBack.Cancel();
    }
}