public class EnemyChaseState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    public EnemyChaseState(Enemy owner) : base(owner)
    {
    }
}