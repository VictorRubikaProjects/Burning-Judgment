using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    public bool IsFinished { get; private set; }
    
    private readonly AudioService m_audioService;
    private readonly FxService m_fxService;
    
    private readonly Collider[] m_strikeHits = new Collider[30];

    public EnemyHitState(EnemyBase owner) : base(owner)
    {
        m_audioService = ServiceLocator.Get<AudioService>();
        m_fxService = ServiceLocator.Get<FxService>();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        IsFinished = false;

        bool dealsDamage = Owner.PendingHitDealsDamage;
        Owner.ConsumeHitRequest();

        if (dealsDamage) Owner.HitGate.TakeDamage();
        
        StartKnockback(Owner.PendingKnockbackDirection, Owner.PendingKnockbackForce);
        
        m_fxService.PlayEjectionEffect(Owner.TransformCache, Owner.StatsBase.HitDuration);
        m_fxService.PlayHitEffect(Owner.TransformCache,Owner.StatsBase.HitDuration);
        m_audioService.PlaySfx(m_audioService.Bank.EnemyHitEvent,Owner.TransformCache.position);
    }

    public override void OnExit()
    {
        base.OnExit();
        Owner.KnockBack.Cancel();
    }

    public override void Update()
    {
        base.Update();
        if (!Owner.KnockBack.IsActive) IsFinished = true;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Owner.KnockBack.IsActive) ExecuteStrikeHit();
    }

    public void Relaunch(Vector3 direction, float distance)
    {
        IsFinished = false;
        StartKnockback(direction, distance);
    }

    private void StartKnockback(Vector3 direction, float distance)
    {
        if (Owner.Animator)
        {
           Owner.Animator.Play(HitHash);
            Owner.Animator.CrossFade(HitHash, 0.1f); 
        }
        
        Owner.KnockBack.HitAsync(
            direction,
            Owner.StatsBase.HitDuration,
            distance,
            Owner.StatsBase.HitKnockbackCurve,
            Owner.StatsBase.ObstacleLayer).Forget();
    }

    private void ExecuteStrikeHit()
    {
        int count = Physics.OverlapSphereNonAlloc(
            Owner.TransformCache.position,
            Owner.StatsBase.HitRadius,
            m_strikeHits,
            Owner.StatsBase.EnemyLayer,
            QueryTriggerInteraction.Ignore);
        
        for (int i = 0; i < count; i++)
        {
            EnemyBase enemyBase = m_strikeHits[i].GetComponentInParent<EnemyBase>();
            if (!enemyBase) continue;

            EventBus<ActorPushedEvent>.Raise(new ActorPushedEvent(
                enemyBase,
                -Owner.TransformCache.forward,
                Owner.StatsBase.PushForce,
                Owner.StatsBase.Destabilization,
                Owner.TransformCache.position));
            
            return;
        }
    }
}