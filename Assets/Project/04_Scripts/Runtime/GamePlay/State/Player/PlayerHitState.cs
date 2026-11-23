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

        bool dealsDamage = m_owner.PendingHitDealsDamage;
        m_owner.ConsumeHitRequest();
        
        if (dealsDamage) m_owner.HitGate.TakeDamage();
        
        StartKnockback(m_owner.PendingKnockbackDirection, m_owner.PendingKnockbackForce);
        
        m_owner.Chain.Reset();
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
    
    public void Relaunch(Vector3 direction, float distance)
    {
        m_isFinished = false;
        StartKnockback(direction, distance);
    }
    
    private void StartKnockback(Vector3 direction, float distance)
    {
        if (m_animator)
        {
            PlayAnimation(HitHash);
        }
        
        m_audioService.PlaySfx(m_audioService.Bank.PlayerHitEvent,m_owner.TransformCache.position);
        m_fxService.PlayEjectionEffect(m_owner.TransformCache,m_stats.HitDuration);
        m_fxService.PlayHitEffect(m_owner.TransformCache,m_stats.HitDuration);
        
        m_owner.Aspect.HurtVisuals();
        
        m_owner.KnockBack.HitAsync(
            direction,
            m_stats.HitDuration,
            distance,
            m_stats.HitKnockbackCurve,
            m_stats.ObstacleLayer).Forget();
    }
}