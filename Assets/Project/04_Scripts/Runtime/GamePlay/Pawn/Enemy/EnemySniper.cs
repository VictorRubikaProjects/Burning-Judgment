using UnityEngine;

public class EnemySniper : Enemy<SO_ConfigEnemySniper>
{
    [field: SerializeField] public Transform ShootPoint { get; private set; }
    [field: SerializeField] public SniperAimVisual AimVisual { get; private set; }
    
    private EnemyChaseState m_chaseState;
    private EnemyFleeState m_fleeState;
    private EnemyHitState m_hitState;
    private EnemyFallState m_fallState;
    private EnemySniperIdleState m_idleState;
    private EnemySniperAttackState m_attackState;
    
    private bool m_shotRequest;
    private float m_nextShotTime;
    
    protected override IState SetupStates()
    {
        m_chaseState = new EnemyChaseState(this);
        m_fleeState = new EnemyFleeState(this);
        m_hitState = new EnemyHitState(this);
        m_fallState = new EnemyFallState(this);
        m_idleState = new EnemySniperIdleState(this);
        m_attackState = new EnemySniperAttackState(this);
        
        return m_idleState;
    }
    
    protected override void Start()
    {
        base.Start();
        AimVisual.Configure(Stats.ConeAngle, Stats.ShotRange, Stats.TargetLayer);
    }
    
    protected override void SetupTransitions()
    {
        Any(m_fallState, new FuncPredicate(() => !GroundCheck.IsGrounded()));

        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));
        At(m_fleeState, m_idleState, new FuncPredicate(() => m_fleeState.IsFinished));
        At(m_chaseState, m_idleState, new FuncPredicate(() => m_chaseState.IsFinished));
        At(m_attackState, m_idleState, new FuncPredicate(() => m_attackState.IsFinished));

        Any(m_hitState, new FuncPredicate(() => HitRequested && GroundCheck.IsGrounded()));
        Any(m_attackState, new FuncPredicate(() => m_shotRequest && GroundCheck.IsGrounded()));
        Any(m_fleeState, new FuncPredicate(() => FleeRequested && GroundCheck.IsGrounded()));
        Any(m_chaseState, new FuncPredicate(() => ChaseRequested && GroundCheck.IsGrounded()));
    }
    
    public void EvaluateShootRequest() => m_shotRequest = CanShoot();

    public void ClearShootRequest() => m_shotRequest = false;

    public void ConsumeShootRequest()
    {
        m_shotRequest = false;
        m_nextShotTime = Time.time + Stats.Cooldown;
    }

    private bool CanShoot()
    {
        if (Time.time < m_nextShotTime) return false;
        if (!GroundCheck.IsGrounded()) return false;

        float sqrDistance = (Player.TransformCache.position - TransformCache.position).sqrMagnitude;
        float min = Stats.DistanceMin;
        float max = Stats.DistanceMax;

        return sqrDistance >= min * min && sqrDistance <= max * max;
    }
}