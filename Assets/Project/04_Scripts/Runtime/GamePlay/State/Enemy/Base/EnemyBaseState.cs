public class EnemyBaseState : IState
{
    protected readonly EnemyBase Owner;

    public EnemyBaseState(EnemyBase owner)
    {
        Owner = owner;
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}