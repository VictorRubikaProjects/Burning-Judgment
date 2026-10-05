public class EnemyShooterIdleState : EnemyIdleState
{
    private readonly EnemyShooter m_shooter;

    public EnemyShooterIdleState(EnemyShooter owner) : base(owner)
    {
        m_shooter = owner;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        m_shooter.Shoot.Enable(true);
    }

    public override void OnExit()
    {
        base.OnExit();
        m_shooter.Shoot.Enable(false);
    }
}