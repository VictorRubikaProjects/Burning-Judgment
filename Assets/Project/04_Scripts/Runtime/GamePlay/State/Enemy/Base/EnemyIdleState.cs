using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(EnemyBase owner) : base(owner) { }

    public override void OnEnter()
    {
        base.OnEnter();
        Owner.Agent.ResetPath();
    }

    public override void Update()
    {
        base.Update();
        Owner.LookAtPlayer();
        EvaluateSituation();
    }

    private void EvaluateSituation()
    {
        float distance = Vector3.Distance(Owner.TransformCache.position, Owner.Player.TransformCache.position);

        if (distance > Owner.StatsBase.DistanceMax) Owner.RequestChase();
        else if (distance < Owner.StatsBase.DistanceMin) Owner.RequestFlee();
    }
}