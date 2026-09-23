public class EnemyHitState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    public EnemyHitState(Enemy owner) : base(owner)
    {
    }
}