using UnityEngine;

public class EnemyDasher : EnemyWithCooldown<SO_ConfigEnemyDash>
{
    [SerializeField] private SpriteRenderer spriteDash;

    private EnemyChaseState m_chaseState;
    private EnemyFleeState m_fleeState;
    private EnemyHitState m_hitState;
    private EnemyDasherIdleState m_idleState;
    private EnemyFallState m_fallState;
    private EnemyDashAttackState m_dashState;
    private EnemyRepositionState m_repositionState;

    public DashComponent Dash { get; private set; }
    public DashIndicatorComponent DashIndicator { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Dash = new DashComponent(this, Rigidbody, GroundCheck);
        DashIndicator = new DashIndicatorComponent(this, spriteDash, 2f);
    }

    protected override void Start()
    {
        base.Start();
        AddActorComponent(Dash);
    }

    protected override IState SetupStates()
    {
        m_chaseState = new EnemyChaseState(this);
        m_fleeState = new EnemyFleeState(this);
        m_idleState = new EnemyDasherIdleState(this);
        m_hitState = new EnemyHitState(this);
        m_fallState = new EnemyFallState(this);
        m_dashState = new EnemyDashAttackState(this);
        m_repositionState = new EnemyRepositionState(this);
        
        return m_idleState;
    }

    protected override void SetupTransitions()
    {
        base.SetupTransitions();

        Any(m_fallState, new FuncPredicate(() => !GroundCheck.IsGrounded()));

        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));
        At(m_fleeState, m_idleState, new FuncPredicate(() => m_fleeState.IsFinished));
        At(m_chaseState, m_idleState, new FuncPredicate(() => m_chaseState.IsFinished));
        At(m_dashState, m_idleState, new FuncPredicate(() => m_dashState.IsFinished));
        At(m_idleState, m_repositionState, new FuncPredicate(() => RepositionRequested));
        At(m_repositionState, m_idleState, new FuncPredicate(() => m_repositionState.IsFinished));

        Any(m_hitState, new FuncPredicate(() => HitRequested && GroundCheck.IsGrounded()));
        Any(m_dashState, new FuncPredicate(() => m_attackRequested && GroundCheck.IsGrounded()));
        Any(m_fleeState, new FuncPredicate(() => FleeRequested && GroundCheck.IsGrounded()));
        Any(m_chaseState, new FuncPredicate(() => ChaseRequested && GroundCheck.IsGrounded()));
    }

    public void ConsumeDashRequest() => ConsumeAttackRequest(Stats.DashCooldown);
}