using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public class EnemyHitState : EnemyBaseState
{
    public bool IsFinished { get; private set; }

    public EnemyHitState(EnemyBase owner) : base(owner) { }
    
    private readonly Collider[] m_strikeHits = new Collider[30];

    public override void OnEnter()
    {
        base.OnEnter();
        IsFinished = false;
        Owner.Agent.enabled = false;
        Owner.ConsumeHitRequest();

        Owner.KnockBack.HitAsync(
            Owner.PendingKnockbackDirection,
            Owner.StatsBase.HitDuration,
            Owner.PendingKnockbackForce,
            Owner.StatsBase.HitKnockbackCurve).Forget();
    }

    public override void OnExit()
    {
        base.OnExit();
        Owner.KnockBack.Cancel();

        bool grounded = Owner.GroundCheck.IsGrounded();
        if (grounded) Owner.Agent.Warp(Owner.Rigidbody.position);
        Owner.Agent.enabled = grounded;
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
                Owner.StatsBase.Force));
            
            return;
        }
    }
}