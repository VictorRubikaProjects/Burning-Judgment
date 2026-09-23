public class EnemyFleeState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    public EnemyFleeState(Enemy owner) : base(owner)
    {
    }
}