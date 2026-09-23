public class EnemyShooterFleeState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    public EnemyShooterFleeState(Enemy owner) : base(owner)
    {
    }
}