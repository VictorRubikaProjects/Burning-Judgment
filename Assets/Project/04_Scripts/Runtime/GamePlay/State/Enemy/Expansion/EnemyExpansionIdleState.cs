public class EnemyExpansionIdleState : EnemyIdleState
{
    private readonly EnemyExpansion m_expansion;
    
    public EnemyExpansionIdleState(EnemyExpansion owner) : base(owner)
    {
        m_expansion = owner;
    }
    
    public override void Update()
    {
        base.Update();
        m_expansion.EvaluateAttackRequest();
    }

    public override void OnExit()
    {
        base.OnExit();
        m_expansion.ClearAttackRequest();
    }
}