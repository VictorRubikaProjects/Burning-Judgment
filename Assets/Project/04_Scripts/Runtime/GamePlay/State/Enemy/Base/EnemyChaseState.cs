using UnityEngine;

public class EnemyChaseState : EnemyBaseState
{
    public bool IsFinished { get; private set; }

    public EnemyChaseState(EnemyBase owner) : base(owner) { }

    public override void OnEnter()
    {
        base.OnEnter();
        IsFinished = false;
        Owner.ConsumeChaseRequest();
        Owner.Agent.stoppingDistance = Owner.StatsBase.DistanceMax;
        Owner.Agent.SetDestination(Owner.Player.TransformCache.position);
    }

    public override void Update()
    {
        base.Update();
        Owner.LookAtPlayer();
        Owner.Agent.SetDestination(Owner.Player.TransformCache.position);

        float distance = Vector3.Distance(Owner.TransformCache.position, Owner.Player.TransformCache.position);
        if (distance < Owner.StatsBase.DistanceMax) IsFinished = true;
    }
}