using UnityEngine;

public class EnemyFleeState : EnemyBaseState
{
    public bool IsFinished { get; private set; }

    public EnemyFleeState(EnemyBase owner) : base(owner) { }

    public override void OnEnter()
    {
        base.OnEnter();
        Owner.ConsumeFleeRequest();
        IsFinished = false;
        Owner.Agent.stoppingDistance = 0f;
        CalculateFleePosition();
    }

    public override void Update()
    {
        base.Update();
        Owner.LookAtPlayer();
        CalculateFleePosition();
        EvaluateFleeSuccess();
    }

    private void CalculateFleePosition()
    {
        Vector3 away = (Owner.TransformCache.position - Owner.Player.TransformCache.position).normalized;
        Owner.Agent.SetDestination(Owner.TransformCache.position + away * Owner.StatsBase.DistanceMax);
    }

    private void EvaluateFleeSuccess()
    {
        float distance = Vector3.Distance(Owner.TransformCache.position, Owner.Player.TransformCache.position);
        if (distance > Owner.StatsBase.DistanceMin) IsFinished = true;
    }
}