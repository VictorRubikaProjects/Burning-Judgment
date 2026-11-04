using UnityEngine;

public class EnemyExpansion : EnemyWithCooldown<SO_ConfigEnemyExpansion>
{
    [SerializeField] private Material materialChargeAttack;
    [SerializeField] private Renderer rendererEnemy;

    public Material MaterialChargeAttackInstance { get; private set; }

    private EnemyChaseState m_chaseState;
    private EnemyFleeState m_fleeState;
    private EnemyHitState m_hitState;
    private EnemyFallState m_fallState;
    private EnemyExpansionIdleState m_idleState;
    private EnemyExpansionAttackState m_attackState;
    private EnemyRepositionState m_repositionState;

    protected override void Awake()
    {
        base.Awake();
        MaterialChargeAttackInstance = new Material(materialChargeAttack);
        rendererEnemy.material = MaterialChargeAttackInstance;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Destroy(MaterialChargeAttackInstance);
        MaterialChargeAttackInstance = null;
    }

    protected override IState SetupStates()
    {
        m_chaseState = new EnemyChaseState(this);
        m_fleeState = new EnemyFleeState(this);
        m_hitState = new EnemyHitState(this);
        m_fallState = new EnemyFallState(this);
        m_idleState = new EnemyExpansionIdleState(this);
        m_attackState = new EnemyExpansionAttackState(this);
        m_repositionState = new EnemyRepositionState(this);

        return m_idleState;
    }

    protected override void SetupTransitions()
    {
        Any(m_fallState, new FuncPredicate(() => !GroundCheck.IsGrounded()));

        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));
        At(m_fleeState, m_idleState, new FuncPredicate(() => m_fleeState.IsFinished));
        At(m_chaseState, m_idleState, new FuncPredicate(() => m_chaseState.IsFinished));
        At(m_attackState, m_idleState, new FuncPredicate(() => m_attackState.IsFinished));
        At(m_idleState, m_repositionState, new FuncPredicate(() => RepositionRequested));
        At(m_repositionState, m_idleState, new FuncPredicate(() => m_repositionState.IsFinished));

        Any(m_hitState, new FuncPredicate(() => HitRequested && GroundCheck.IsGrounded()));
        Any(m_attackState, new FuncPredicate(() => m_attackRequested && GroundCheck.IsGrounded()));
        Any(m_fleeState, new FuncPredicate(() => FleeRequested && GroundCheck.IsGrounded()));
        Any(m_chaseState, new FuncPredicate(() => ChaseRequested && GroundCheck.IsGrounded()));
    }

    public new void ConsumeAttackRequest() => ConsumeAttackRequest(Stats.AttackCooldown);

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Stats) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Stats.AttackRadius);
    }
#endif
}