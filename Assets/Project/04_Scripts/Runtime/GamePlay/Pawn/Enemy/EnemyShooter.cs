using UnityEngine;

public class EnemyShooter : Enemy<SO_ConfigEnemyShoot>
{
    [field:SerializeField] public Transform ShootPoint { get; private set; }

    private EnemyChaseState m_chaseState;
    private EnemyFleeState m_fleeState;
    private EnemyHitState m_hitState;
    private EnemyIdleState m_idleState;
    private EnemyFallState m_fallState;

    public EnemyShootComponent Shoot { get; private set; }

    protected override void Awake()
    {
        Shoot = new EnemyShootComponent(
            this,
            Stats.ProjectilePrefabs,
            Stats.FrequencyShoot,
            ShootPoint);

        base.Awake();
    }

    protected override void Start()
    {
        AddActorComponent(Shoot);
        base.Start();
    }

    protected override IState SetupStates()
    {
        m_chaseState = new EnemyShooterChaseState(this);
        m_fleeState = new EnemyShooterFleeState(this);
        m_hitState = new EnemyHitState(this);
        m_idleState = new EnemyShooterIdleState(this);
        m_fallState = new EnemyFallState(this);
        return m_idleState;
    }

    protected override void SetupTransitions()
    {
        Any(m_fallState, new FuncPredicate(() => !GroundCheck.IsGrounded()));

        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));
        At(m_fleeState, m_idleState, new FuncPredicate(() => m_fleeState.IsFinished));
        At(m_chaseState, m_idleState, new FuncPredicate(() => m_chaseState.IsFinished));

        Any(m_fleeState, new FuncPredicate(() => FleeRequested && GroundCheck.IsGrounded()));
        Any(m_chaseState, new FuncPredicate(() => ChaseRequested && GroundCheck.IsGrounded()));
        Any(m_hitState, new FuncPredicate(() => HitRequested && GroundCheck.IsGrounded()));
    }
}