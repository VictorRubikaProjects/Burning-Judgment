using System;
using Codice.CM.Common;
using UnityEngine;

public class PlayerCharacter : Actor
{
    #region Variable

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    
    [Header("Config")]
    [SerializeField] private ConfigStatsPlayer stats;

    public InputComponent Input { get; private set; }
    public PlayerControllerComponent ControllerComponent { get; private set; }

    public Vector3 MoveDir => m_moveDir;
    public Transform TransformCache => m_transformCache;
    
    private StateMachine m_stateMachine;

    private Vector3 m_moveDir;
    
    private bool m_wantsDash;
    private bool m_isHit;
    private bool m_isDead;
    
    private IdleState m_idleState;
    private DeathState m_deathState;
    private DashState m_dashState;
    private HitState m_hitState;
    
    private Transform m_transformCache;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

        m_transformCache = transform;
        
        SetupStateMachine();

        Input = new InputComponent(owner: this);
        ControllerComponent = new PlayerControllerComponent(owner:this,rb);
    }

    protected override void Start()
    {
        base.Start();
        
        AddActorComponent(Input);
        AddActorComponent(ControllerComponent);
        
        ServiceLocator.Get<CameraService>().AddTarget(transform,0.5f);
    }

    private void OnEnable()
    {
        Input.OnSwipe += SwipeHandler;
    }

    private void OnDisable()
    {
        Input.OnSwipe -= SwipeHandler;
    }

    protected override void Update()
    {
        base.Update();
        
        m_stateMachine.Update();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        
        m_stateMachine.FixedUpdate();
    }

    #endregion

    #region State Machine

    void At(IState from, IState to, IPredictate condition) => m_stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredictate condition) => m_stateMachine.AddAnyTransition(to, condition);
    
    private void SetupStateMachine()
    {
        m_stateMachine = new StateMachine();

        m_idleState = new IdleState(this, animator,stats);
        m_dashState = new DashState(this, animator, rb,stats);
        m_hitState = new HitState(this, animator,stats);
        m_deathState = new DeathState(this, animator,stats);

        At(m_idleState, m_dashState, new FuncPredicate(() => m_wantsDash));
        At(m_dashState, m_idleState, new FuncPredicate(() => m_dashState.IsFinished));
        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));

        Any(m_hitState, new FuncPredicate(() => m_isHit));
        Any(m_deathState, new FuncPredicate(() => m_isDead));

        m_stateMachine.SetState(m_idleState);
    }

    #endregion
    
    private void SwipeHandler(Vector2 moveDir)
    {
        m_moveDir = new Vector3(moveDir.x, 0f, moveDir.y);
        m_wantsDash = true;
    }

    #region Helpers

    public void ConsumeDashRequest() => m_wantsDash = false;
    public void ConsumeHitRequest() => m_isHit = false;

    public void RequestHit() => m_isHit = true;
    public void Kill() => m_isDead = true;

    #endregion
    
#if UNITY_EDITOR    
    
    #region Debug

    private void OnDrawGizmosSelected()
    {
        DrawDashRadius();
    }

    private void DrawDashRadius()
    {
        if (m_dashState.IsFinished) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.position + MoveDir * stats.offsetDash,stats.dashRadius);
    }

    #endregion
    
#endif
}