public class EnemyDasherIdleState : EnemyIdleState
{
    private readonly EnemyDasher m_dasher;

    public EnemyDasherIdleState(EnemyDasher owner) : base(owner)
    {
        m_dasher = owner;
    }

    public override void Update()
    {
        base.Update();
        m_dasher.EvaluateAttackRequest();
    }

    public override void OnExit()
    {
        base.OnExit();
        m_dasher.ClearAttackRequest();
    }
}