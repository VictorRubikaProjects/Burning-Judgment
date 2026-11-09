public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(EnemyBase owner) : base(owner) { }

    public override void OnEnter()
    {
        base.OnEnter();
        Owner.Steering.Stop();
    }

    public override void Update()
    {
        base.Update();
        Owner.LookAtPlayer();
        EvaluateSituation();
    }

    private void EvaluateSituation()
    {
        switch (Owner.EvaluatePositionNeed())
        {
            case EnemyBase.PositionNeed.Approach: Owner.RequestChase(); break;
            case EnemyBase.PositionNeed.Retreat: Owner.RequestFlee(); break;
            case EnemyBase.PositionNeed.Reposition: Owner.RequestReposition(); break;
        }
    }
}