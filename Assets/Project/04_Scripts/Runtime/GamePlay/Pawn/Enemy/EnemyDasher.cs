using UnityEngine;

public class EnemyDasher : Enemy<SO_ConfigEnemyDash>
{
    private EnemyChaseState m_chaseState;
    private EnemyFleeState m_fleeState;
    private EnemyHitState m_hitState;
    private EnemyDasherIdleState m_idleState;
    private EnemyFallState m_fallState;
    private EnemyDashAttackState m_dashState;

    private bool m_dashRequest;
    private float m_nextDashTime;

    public DashComponent Dash { get; private set; }

    protected override void Update()
    {
        base.Update();
        Debug.Log($"State : {m_stateMachine.GetCurrentState().ToString()}");
    }

    protected override IState SetupStates()
    {
        Dash = new DashComponent(this, Rigidbody, GroundCheck);

        m_chaseState = new EnemyChaseState(this);
        m_fleeState = new EnemyFleeState(this);
        m_idleState = new EnemyDasherIdleState(this);
        m_hitState = new EnemyHitState(this);
        m_fallState = new EnemyFallState(this);
        m_dashState = new EnemyDashAttackState(this);
        return m_idleState;
    }

    protected override void SetupTransitions()
    {
        Any(m_fallState, new FuncPredicate(() => !GroundCheck.IsGrounded()));

        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));
        At(m_fleeState, m_idleState, new FuncPredicate(() => m_fleeState.IsFinished));
        At(m_chaseState, m_idleState, new FuncPredicate(() => m_chaseState.IsFinished));
        At(m_dashState, m_idleState, new FuncPredicate(() => m_dashState.IsFinished));

        Any(m_hitState, new FuncPredicate(() => HitRequested && GroundCheck.IsGrounded()));
        Any(m_dashState, new FuncPredicate(() => m_dashRequest && GroundCheck.IsGrounded()));
        Any(m_fleeState, new FuncPredicate(() => FleeRequested && GroundCheck.IsGrounded()));
        Any(m_chaseState, new FuncPredicate(() => ChaseRequested && GroundCheck.IsGrounded()));
    }

    public void EvaluateDashRequest() => m_dashRequest = CanDash();

    public void ClearDashRequest() => m_dashRequest = false;

    public void ConsumeDashRequest()
    {
        m_dashRequest = false;
        m_nextDashTime = Time.time + Stats.DashCooldown;
    }

    private bool CanDash()
    {
        if (Time.time < m_nextDashTime) return false;
        if (!GroundCheck.IsGrounded()) return false;

        float sqrDistance = (Player.TransformCache.position - TransformCache.position).sqrMagnitude;
        float min = Stats.DistanceMin;
        float max = Stats.DistanceMax;

        return sqrDistance >= min * min && sqrDistance <= max * max;
    }
}