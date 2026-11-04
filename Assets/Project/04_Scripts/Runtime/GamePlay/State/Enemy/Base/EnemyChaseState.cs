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
    }

    public override void Update()
    {
        base.Update();
        Owner.LookAtPlayer();

        float distance = Vector3.Distance(Owner.TransformCache.position, Owner.Player.TransformCache.position);
        if (distance < Owner.ComfortDistance) IsFinished = true;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        Owner.Steering.MoveTowards(Owner.Player.TransformCache.position, Owner.StatsBase.Speed);
    }

    public override void OnExit()
    {
        base.OnExit();
        Owner.Steering.Stop();
    }
}