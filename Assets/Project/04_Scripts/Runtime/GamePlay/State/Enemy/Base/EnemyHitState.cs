using Cysharp.Threading.Tasks;

public class EnemyHitState : EnemyBaseState
{
    public bool IsFinished { get; private set; }

    public EnemyHitState(EnemyBase owner) : base(owner) { }

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
}