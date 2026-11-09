using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerHitState : PlayerBaseState
{
    private bool m_isFinished;

    public bool IsFinished => m_isFinished;
    
    private readonly AudioService m_audioService;
    private readonly FxService m_fxService;

    public PlayerHitState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats, playerStats.HitAnimation)
    {
        m_audioService = ServiceLocator.Get<AudioService>();
        m_fxService = ServiceLocator.Get<FxService>();
    }

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
        
        m_owner.Chain.Reset();
        
        m_audioService.PlaySfx(m_audioService.Bank.PlayerHitEvent,m_owner.TransformCache.position);
        m_fxService.PlayEjectionEffect(m_owner.TransformCache,m_stats.HitDuration);
        m_fxService.PlayHitEffect(m_owner.TransformCache,m_stats.HitDuration);
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