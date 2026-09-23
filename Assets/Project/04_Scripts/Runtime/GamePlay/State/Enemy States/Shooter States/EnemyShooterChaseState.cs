public class EnemyShooterChaseState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    public EnemyShooterChaseState(Enemy owner) : base(owner)
    {
    }
}