using Event_Bus;
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
    [SerializeField] private SO_PlayerStats stats;

    public InputComponent Input { get; private set; }
    public PlayerControllerComponent Controller { get; private set; }
    public PlayerAspectComponent Aspect { get; private set; }
    public HitGateComponent HitGate { get; private set; }

    public Vector3 MoveDir => m_moveDir;
    public Transform TransformCache => m_transformCache;
    public Vector3 PendingKnockbackDirection => m_pendingKnockbackDirection;
    public float PendingKnockbackForce => m_pendingKnockbackForce;
    public bool HasHitRequest => m_hitRequested;
    
    private StateMachine m_stateMachine;

    private Vector3 m_moveDir;
    private Vector3 m_attackDirection;
    private Vector3 m_pendingKnockbackDirection;
    
    private bool m_wantsDash;
    private bool m_wantsAttack;
    private bool m_isDead;
    private bool m_hitRequested;
    
    private PlayerIdleState m_playerIdleState;
    private PlayerDeathState m_playerDeathState;
    private PlayerDashState m_playerDashState;
    private PlayerAttackState m_playerAttackState;
    private PlayerHitState m_playerHitState;
    
    private Transform m_transformCache;
    
    private float m_pendingKnockbackForce;
    
    private EventBinding<ActorPushedEvent> m_playerPushedBinding;

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
        HitGate = new HitGateComponent(this);
    }

    protected override void Start()
    {
        base.Start();
        
        AddActorComponent(Input);
        AddActorComponent(Controller);
        AddActorComponent(Aspect);
        AddActorComponent(HitGate);
        
        ServiceLocator.Get<CameraService>().AddTarget(transform,0.5f);
    }

    private void OnEnable()
    {
        Input.OnSwipe += SwipeHandler;
        Input.OnPressSwipeSuccess += AttackHandler;
        Input.OnAttackWindowEnter += Aspect.AttackReadyVisuals;
        Input.OnAttackWindowExit += Aspect.CancelAttackReadyVisuals;

        m_playerPushedBinding = new EventBinding<ActorPushedEvent>(OnActorPushed);
        EventBus<ActorPushedEvent>.Register(m_playerPushedBinding);
    }

    private void OnDisable()
    {
        Input.OnSwipe -= SwipeHandler;
        Input.OnPressSwipeSuccess -= AttackHandler;
        Input.OnAttackWindowEnter -= Aspect.AttackReadyVisuals;
        Input.OnAttackWindowExit -= Aspect.CancelAttackReadyVisuals;

        EventBus<ActorPushedEvent>.Unregister(m_playerPushedBinding);
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

        m_playerIdleState = new PlayerIdleState(this, animator,stats);
        m_playerDashState = new PlayerDashState(this, animator, rb,stats);
        m_playerDeathState = new PlayerDeathState(this, animator,stats);
        m_playerAttackState = new PlayerAttackState(this, animator, stats);
        m_playerHitState = new PlayerHitState(this, animator, stats);

        At(m_playerIdleState, m_playerDashState, new FuncPredicate(() => m_wantsDash));
        At(m_playerDashState, m_playerIdleState, new FuncPredicate(() => m_playerDashState.IsFinished));
        At(m_playerIdleState, m_playerAttackState, new FuncPredicate(() => m_wantsAttack));
        At(m_playerAttackState, m_playerIdleState, new FuncPredicate(() => m_playerAttackState.IsFinished));
        At(m_playerAttackState, m_playerDashState, new FuncPredicate(() => m_playerAttackState.IsFinished && m_wantsDash));
        At(m_playerHitState, m_playerIdleState, new FuncPredicate(() => m_playerHitState.IsFinished));
        
        Any(m_playerDeathState, new FuncPredicate(() => m_isDead));
        Any(m_playerHitState, new FuncPredicate(() => HasHitRequest));
        
        
        m_stateMachine.SetState(m_playerIdleState);
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
        
        m_playerAttackState.SetAttackDirection(m_attackDirection);
        
        RequestAttack();
    }
    
    private void OnActorPushed(ActorPushedEvent e)
    {
        if (e.Target != this) return;
        
        if (m_stateMachine.GetCurrentState() is PlayerHitState) return;

        m_pendingKnockbackDirection = e.Direction;
        m_pendingKnockbackForce = KnockbackUtility.CalculateKnockbackDistance(e.Force, stats.Weight, stats.KnockbackResistance);
        RequestHit();
    }

    #region Helpers

    public void ConsumeDashRequest() => m_wantsDash = false;
    public void ConsumeAttackRequest() => m_wantsAttack = false;
    public void RequestDash()=> m_wantsDash = true;
    public void RequestAttack()=> m_wantsAttack = true;
    public override void Kill() => m_isDead = true;

    public void RequestHit() => m_hitRequested = true;
    
    public void ConsumeHitRequest() => m_hitRequested = false;

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
        if (Application.isPlaying && m_playerDashState.IsFinished) return;
     
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.position + MoveDir * stats.OffsetDash,stats.DashRadius);
    }

    private void DrawAttackCast()
    {
        if (Application.isPlaying && m_playerAttackState.IsFinished) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rb.position, stats.AttackCastRadius);
        Gizmos.DrawWireSphere(rb.position + m_attackDirection * stats.AttackCastDistance, stats.AttackCastRadius);
        Gizmos.DrawLine(rb.position, rb.position + m_attackDirection * stats.AttackCastDistance);
    }

    #endregion
    
#endif
    
}