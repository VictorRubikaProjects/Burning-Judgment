using UnityEngine;

public class EnemyExpansion : Enemy<SO_ConfigEnemyExpansion>
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

    private bool m_attackRequested;
    private float m_nextAttackTime;

    protected override void Awake()
    {
        base.Awake();
        MaterialChargeAttackInstance = new Material(materialChargeAttack);
        rendererEnemy.material = MaterialChargeAttackInstance;
    }

    private void OnDestroy()
    {
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

        return m_idleState;
    }

    protected override void SetupTransitions()
    {
        Any(m_fallState, new FuncPredicate(() => !GroundCheck.IsGrounded()));

        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));
        At(m_fleeState, m_idleState, new FuncPredicate(() => m_fleeState.IsFinished));
        At(m_chaseState, m_idleState, new FuncPredicate(() => m_chaseState.IsFinished));
        At(m_attackState, m_idleState, new FuncPredicate(() => m_attackState.IsFinished));

        Any(m_hitState, new FuncPredicate(() => HitRequested && GroundCheck.IsGrounded()));
        Any(m_attackState, new FuncPredicate(() => m_attackRequested && GroundCheck.IsGrounded()));
        Any(m_fleeState, new FuncPredicate(() => FleeRequested && GroundCheck.IsGrounded()));
        Any(m_chaseState, new FuncPredicate(() => ChaseRequested && GroundCheck.IsGrounded()));
    }

    public void EvaluateAttackRequest() => m_attackRequested = CanAttack();

    public void ClearAttackRequest() => m_attackRequested = false;

    public void ConsumeAttackRequest()
    {
        m_attackRequested = false;
        m_nextAttackTime = Time.time + Stats.AttackCooldown;
    }

    private bool CanAttack()
    {
        if (Time.time < m_nextAttackTime) return false;
        if (!GroundCheck.IsGrounded()) return false;

        float sqrDistance = (Player.TransformCache.position - TransformCache.position).sqrMagnitude;
        float min = Stats.DistanceMin;
        float max = Stats.DistanceMax;

        return sqrDistance >= min * min && sqrDistance <= max * max;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Stats) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Stats.AttackRadius);
    }
#endif
}