using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    private bool m_isFinished;

    public bool IsFinished => m_isFinished;

    public PlayerHitState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats) : base(owner, animator, playerStats) { }

    public override void OnEnter()
    {
        base.OnEnter();

        m_isFinished = false;

        m_owner.ConsumeHitRequest();
        m_owner.Aspect.HurtVisuals();

        m_animator.CrossFade(HitHash, m_crossFadeDuration);

        m_owner.HitGate.TakeDamage();
        
        RunHitAsync().Forget();
    }

    public override void OnExit()
    {
        base.OnExit();
        m_owner.KnockBack.Cancel();
    }

    private async UniTaskVoid RunHitAsync()
    {
        await m_owner.KnockBack.HitAsync(
            m_owner.PendingKnockbackDirection,
            m_stats.HitDuration,
            m_owner.PendingKnockbackForce,
            m_stats.HitKnockbackCurve);

        if (!m_owner.KnockBack.IsActive)
        {
            m_isFinished = true;
        }
    }
}