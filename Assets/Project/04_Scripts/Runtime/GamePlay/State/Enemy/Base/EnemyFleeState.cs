using UnityEngine;

public class EnemyFleeState : EnemyBaseState
{
    public bool IsFinished { get; private set; }

    private Vector3 m_fleeTargetPosition;

    public EnemyFleeState(EnemyBase owner) : base(owner) { }

    public override void OnEnter()
    {
        base.OnEnter();
        Owner.ConsumeFleeRequest();
        IsFinished = false;
        CalculateFleePosition();
    }

    public override void Update()
    {
        base.Update();
        Owner.LookAtPlayer();
        EvaluateFleeSuccess();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        Owner.Steering.MoveTowards(m_fleeTargetPosition, Owner.StatsBase.Speed);
    }

    public override void OnExit()
    {
        base.OnExit();
        Owner.Steering.Stop();
    }

    private void CalculateFleePosition()
    {
        Vector3 away = (Owner.TransformCache.position - Owner.Player.TransformCache.position).normalized;
        m_fleeTargetPosition = Owner.TransformCache.position + away * Owner.StatsBase.DistanceMax;
    }

    private void EvaluateFleeSuccess()
    {
        float distance = Vector3.Distance(Owner.TransformCache.position, Owner.Player.TransformCache.position);
        if (distance > Owner.ComfortDistance) IsFinished = true;
    }
}