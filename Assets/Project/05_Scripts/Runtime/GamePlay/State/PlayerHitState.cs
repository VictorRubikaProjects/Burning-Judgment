using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerHitState : BaseState
{
    private CancellationTokenSource m_cts;
    private bool m_isFinished;

    public bool IsFinished => m_isFinished;

    public PlayerHitState(PlayerCharacter owner, Animator animator, ConfigStatsPlayer configStats) : base(owner, animator, configStats) { }

    public override void OnEnter()
    {
        base.OnEnter();

        m_isFinished = false;

        m_owner.ConsumeHitRequest();
        m_owner.Aspect.HurtVisuals();

        m_animator.CrossFade(HitHash, m_crossFadeDuration);

        m_cts = new CancellationTokenSource();
        RunHitAsync(m_cts.Token).Forget();
    }

    public override void OnExit()
    {
        base.OnExit();
        m_cts?.Cancel();
        m_cts?.Dispose();
    }

    private async UniTaskVoid RunHitAsync(CancellationToken token)
    {
        await m_owner.Controller.DashAsync(
            m_owner.PendingKnockbackDirection,
            token,
            m_configStats.HitDuration,
            m_owner.PendingKnockbackForce,
            m_configStats.HitKnockbackCurve,
            m_configStats.GroundLayer,
            m_configStats.GroundCheckHeight,
            m_configStats.GroundCheckDistance);

        if (token.IsCancellationRequested) return;

        m_isFinished = true;
    }
}