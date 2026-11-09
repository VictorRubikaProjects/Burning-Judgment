public class EnemySniperIdleState : EnemyIdleState
{
    private readonly EnemySniper m_sniper;
    
    public EnemySniperIdleState(EnemyBase owner) : base(owner)
    {
        m_sniper = owner as EnemySniper;
    }

    public override void Update()
    {
        base.Update();
        m_sniper.EvaluateShootRequest();
    }

    public override void OnExit()
    {
        base.OnExit();
        m_sniper.ClearShootRequest();
    }
    
}