using UnityEngine;

public class EnemyBaseState : IState
{
    protected readonly EnemyBase Owner;
    
    protected static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
    protected static readonly int FallHash = Animator.StringToHash("Fall");
    protected static readonly int HitHash = Animator.StringToHash("Hit");
    protected static readonly int ChargeAttackHash = Animator.StringToHash("Charge Attack");
    protected static readonly int AttackHash = Animator.StringToHash("Attack");

    public EnemyBaseState(EnemyBase owner)
    {
        Owner = owner;
    }
    
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}