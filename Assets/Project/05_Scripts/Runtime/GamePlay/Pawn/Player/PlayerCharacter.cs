using UnityEngine;

public class PlayerCharacter : Actor
{
    #region Variable

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    
    [Header("Aspect")]
    [SerializeField] private Material playerMaterial;
    [SerializeField] private MeshRenderer playerRenderer;
    
    [Header("Config")]
    [SerializeField] private ConfigStatsPlayer stats;

    public InputComponent Input { get; private set; }
    public PlayerControllerComponent Controller { get; private set; }
    public PlayerAspectComponent Aspect { get; private set; }
    public HealthComponent Health { get; private set; }

    public Vector3 MoveDir => m_moveDir;
    public Transform TransformCache => m_transformCache;
    
    private StateMachine m_stateMachine;

    private Vector3 m_moveDir;
    private Vector3 m_attackDirection;
    
    private bool m_wantsDash;
    private bool m_wantsAttack;
    private bool m_isDead;
    
    private IdleState m_idleState;
    private DeathState m_deathState;
    private DashState m_dashState;
    private AttackState m_attackState;
    
    private Transform m_transformCache;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

        m_transformCache = transform;
        
        SetupStateMachine();

        Input = new InputComponent(owner: this,stats);
        Controller = new PlayerControllerComponent(owner:this,rb,stats);
        Aspect = new PlayerAspectComponent(owner:this, playerMaterial, playerRenderer,stats);
        Health = new HealthComponent(this);
    }

    protected override void Start()
    {
        base.Start();
        
        AddActorComponent(Input);
        AddActorComponent(Controller);
        AddActorComponent(Aspect);
        AddActorComponent(Health);
        
        ServiceLocator.Get<CameraService>().AddTarget(transform,0.5f);
    }

    private void OnEnable()
    {
        Input.OnSwipe += SwipeHandler;
        Input.OnPressSwipeSuccess += AttackHandler;
        Input.OnAttackWindowEnter += Aspect.AttackReadyVisuals;
        Input.OnAttackWindowExit += Aspect.CancelAttackReadyVisuals;
    }

    private void OnDisable()
    {
        Input.OnSwipe -= SwipeHandler;
        Input.OnPressSwipeSuccess -= AttackHandler;
        Input.OnAttackWindowEnter -= Aspect.AttackReadyVisuals;
        Input.OnAttackWindowExit -= Aspect.CancelAttackReadyVisuals;
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
        m_deathState = new DeathState(this, animator,stats);
        m_attackState = new AttackState(this, animator, stats);

        At(m_idleState, m_dashState, new FuncPredicate(() => m_wantsDash));
        At(m_dashState, m_idleState, new FuncPredicate(() => m_dashState.IsFinished));
        At(m_idleState, m_attackState, new FuncPredicate(() => m_wantsAttack));
        At(m_attackState, m_idleState, new FuncPredicate(() => m_attackState.IsFinished));
        At(m_attackState, m_dashState, new FuncPredicate(() => m_attackState.IsFinished && m_wantsDash));
        
        Any(m_deathState, new FuncPredicate(() => m_isDead));

        m_stateMachine.SetState(m_idleState);
    }

    #endregion
    
    private void SwipeHandler(Vector2 moveDir)
    {
        m_moveDir = new Vector3(moveDir.x, 0f, moveDir.y);
        
        RequestDash();
    }

    private void AttackHandler(Vector2 dir)
    {
        m_attackDirection = new Vector3(dir.x, 0f, dir.y);
        m_moveDir = new Vector3(dir.x, 0f, dir.y);
        
        m_attackState.SetAttackDirection(m_attackDirection);
        
        RequestAttack();
    }

    #region Helpers

    public void ConsumeDashRequest() => m_wantsDash = false;
    public void ConsumeAttackRequest() => m_wantsAttack = false;
    public void RequestDash()=> m_wantsDash = true;
    public void RequestAttack()=> m_wantsAttack = true;
    public void Kill() => m_isDead = true;
    

    #endregion
    
#if UNITY_EDITOR    
    
    #region Debug

    private void OnDrawGizmos()
    {
        DrawDashRadius();
        DrawAttackCast();
    }

    private void DrawDashRadius()
    {
        if (Application.isPlaying && m_dashState.IsFinished) return;
     
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.position + MoveDir * stats.OffsetDash,stats.DashRadius);
    }

    private void DrawAttackCast()
    {
        if (Application.isPlaying && m_attackState.IsFinished) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rb.position, stats.AttackCastRadius);
        Gizmos.DrawWireSphere(rb.position + m_attackDirection * stats.AttackCastDistance, stats.AttackCastRadius);
        Gizmos.DrawLine(rb.position, rb.position + m_attackDirection * stats.AttackCastDistance);
    }

    #endregion
    
#endif
    
}