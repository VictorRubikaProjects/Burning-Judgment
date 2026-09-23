public class EnemyShooterHitState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    public EnemyShooterHitState(Enemy owner) : base(owner)
    {
    }
}