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
        Owner.ConsumeHitRequest();

        Owner.HitGate.TakeDamage();
        
        Owner.KnockBack.HitAsync(
            Owner.PendingKnockbackDirection,
            Owner.StatsBase.HitDuration,
            Owner.PendingKnockbackForce,
            Owner.StatsBase.HitKnockbackCurve,
            Owner.StatsBase.ObstacleLayer).Forget();
        
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