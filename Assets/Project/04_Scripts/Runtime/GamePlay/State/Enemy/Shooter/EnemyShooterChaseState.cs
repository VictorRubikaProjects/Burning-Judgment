public class EnemyShooterChaseState : EnemyChaseState
{
    private readonly EnemyShooter m_shooter;

    public EnemyShooterChaseState(EnemyShooter owner) : base(owner)
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