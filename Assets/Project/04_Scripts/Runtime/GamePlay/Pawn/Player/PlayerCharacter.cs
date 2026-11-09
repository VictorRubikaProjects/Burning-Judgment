using Event_Bus;
using UnityEngine;

public class PlayerCharacter : Actor
{
    public readonly struct DashContext
    {
        public readonly Vector3 Direction;
        public readonly float Distance;

        public DashContext(in Vector3 direction, float distance)
        {
            Direction = direction;
            Distance = distance;
        }
    }

    #region Fields & Properties

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform feetTransform;
    [SerializeField] private SpriteRenderer dashIndicatorSprite;
    [SerializeField] private RectTransform joystickRectT;
    [SerializeField] private CanvasGroup icyPanel;
    [field: SerializeField] public DamagePopUp DamagePopUp { get; protected set; }

    [Header("Aspect")]
    [SerializeField] private Material playerMaterial;
    [SerializeField] private SkinnedMeshRenderer playerRenderer;

    [Header("Config")]
    [SerializeField] private SO_PlayerStats baseStats;

    public PlayerControllerComponent Controller { get; private set; }
    public PlayerInputComponent Input { get; private set; }
    public PlayerAspectComponent Aspect { get; private set; }
    public HitGateComponent HitGate { get; private set; }
    public KnockbackComponent KnockBack { get; private set; }
    public GroundCheckComponent GroundCheck { get; private set; }
    public DashComponent Dash { get; private set; }
    public PlayerChainComponent Chain { get; private set; }
    public DashIndicatorComponent DashIndicator { get; private set; }
    public TimeControllerComponent TimeController { get; private set; }

    public Vector3 MoveDir => m_currentDashContext.Direction;
    public float DashDistance => m_currentDashContext.Distance;

    public Vector3 PendingKnockbackDirection => m_pendingKnockbackDirection;
    public float PendingKnockbackForce => m_pendingKnockbackForce;
    public Transform FeetTransform => feetTransform;
    public string CurrentStateName => m_stateMachine?.GetCurrentState()?.ToString() ?? "None";

    private StateMachine m_stateMachine;
    private DashContext m_currentDashContext;

    private Vector3 m_pendingKnockbackDirection;
    private float m_pendingKnockbackForce;

    private bool m_wantsDash;
    private bool m_wantsRoll;
    private bool m_hitRequested;

    private PlayerIdleState m_playerIdleState;
    private PlayerDeathState m_playerDeathState;
    private PlayerDashState m_playerDashState;
    private PlayerHitState m_playerHitState;
    private PlayerFallState m_playerFallState;
    private PlayerRollState m_playerRollState;

    private EventBinding<ActorPushedEvent> m_playerPushedBinding;
    private SO_PlayerStats m_runtimeStats;

    private SO_PlayerStats ActiveStats => m_runtimeStats ? m_runtimeStats : baseStats;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();
        InitializeStats();
        InitializeComponents();
        SetupStateMachine();
    }

    protected override void Start()
    {
        base.Start();
        RegisterActorComponents();
        ServiceLocator.Get<CameraService>().AddTarget(transform, 1f);
    }

    private void OnEnable()
    {
        Input.OnJoystickRelease += HandleJoystickRelease;
        Input.OnChargeUpdate += HandleChargeUpdate;
        Input.OnChargeEnd += HandleChargeEnd;
        Input.OnChargeStarted += HandleChargeStarted;
        Input.OnSwipe += HandleSwipe;

        m_playerPushedBinding = new EventBinding<ActorPushedEvent>(HandleActorPushed);
        EventBus<ActorPushedEvent>.Register(m_playerPushedBinding);
    }

    private void OnDisable()
    {
        Input.OnJoystickRelease -= HandleJoystickRelease;
        Input.OnChargeUpdate -= HandleChargeUpdate;
        Input.OnChargeEnd -= HandleChargeEnd;
        Input.OnChargeStarted -= HandleChargeStarted;
        Input.OnSwipe -= HandleSwipe;

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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_runtimeStats) Destroy(m_runtimeStats);
    }

    #endregion

    #region Initialization Helpers

    private void InitializeStats()
    {
        AugmentService augmentService = ServiceLocator.Get<AugmentService>();
        AugmentDeltas deltas = augmentService != null ? augmentService.Total : default;

        if (augmentService == null)
        {
            Debug.LogError("Augment Service null in PlayerCharacter Awake");
        }

        m_runtimeStats = baseStats.CreateRuntimeCopy(deltas);
        Stability = m_runtimeStats.Stability;
    }

    private void InitializeComponents()
    {
        GroundCheck = new GroundCheckComponent(this, transform, ActiveStats.GroundLayer, ActiveStats.GroundCheckHeight, ActiveStats.GroundCheckDistance);
        TimeController = new TimeControllerComponent(this);
        KnockBack = new KnockbackComponent(this, rb);
        HitGate = new HitGateComponent(this, ActiveStats.InvincibilityDuration);
        Dash = new DashComponent(this, rb, GroundCheck);
        Input = new PlayerInputComponent(this, ActiveStats, joystickRectT, TimeController);
        Controller = new PlayerControllerComponent(this, rb, ActiveStats);
        Aspect = new PlayerAspectComponent(this, playerMaterial, playerRenderer, ActiveStats, icyPanel);
        Chain = new PlayerChainComponent(this, ActiveStats);
        DashIndicator = new DashIndicatorComponent(this, dashIndicatorSprite);
    }

    private void RegisterActorComponents()
    {
        AddActorComponent(TimeController);
        AddActorComponent(GroundCheck);
        AddActorComponent(KnockBack);
        AddActorComponent(HitGate);
        AddActorComponent(Input);
        AddActorComponent(Controller);
        AddActorComponent(Aspect);
        AddActorComponent(Chain);
        AddActorComponent(DashIndicator);
    }

    private void SetupStateMachine()
    {
        m_stateMachine = new StateMachine();

        m_playerIdleState = new PlayerIdleState(this, animator, ActiveStats);
        m_playerDashState = new PlayerDashState(this, animator, ActiveStats);
        m_playerDeathState = new PlayerDeathState(this, animator, ActiveStats);
        m_playerHitState = new PlayerHitState(this, animator, ActiveStats);
        m_playerFallState = new PlayerFallState(this, animator, ActiveStats, rb);
        m_playerRollState = new PlayerRollState(this, animator, ActiveStats);

        m_stateMachine.AddTransition(m_playerIdleState, m_playerDashState, new FuncPredicate(() => m_wantsDash));
        m_stateMachine.AddTransition(m_playerIdleState, m_playerRollState, new FuncPredicate(() => m_wantsRoll));
        m_stateMachine.AddTransition(m_playerDashState, m_playerIdleState, new FuncPredicate(() => m_playerDashState.IsFinished));
        m_stateMachine.AddTransition(m_playerRollState, m_playerIdleState, new FuncPredicate(() => m_playerRollState.IsFinished));
        m_stateMachine.AddTransition(m_playerHitState, m_playerIdleState, new FuncPredicate(() => m_playerHitState.IsFinished));

        m_stateMachine.AddAnyTransition(m_playerDeathState, new FuncPredicate(() => m_isDead));
        m_stateMachine.AddAnyTransition(m_playerHitState, new FuncPredicate(() => m_hitRequested));
        m_stateMachine.AddAnyTransition(m_playerFallState, new FuncPredicate(() => !GroundCheck.IsGrounded()));

        m_stateMachine.SetState(m_playerIdleState);
    }

    #endregion

    #region Input Handlers

    private void HandleJoystickRelease(Vector2 dir, float ratio)
    {
        if (!CanAct()) return;

        Vector3 moveDir = ToWorldDirection(in dir);
        float distance = ComputeDashDistance(ratio);
        m_currentDashContext = new DashContext(in moveDir, distance);

        RequestDash();
    }

    private void HandleChargeUpdate(Vector2 dir, float ratio)
    {
        if (dir == Vector2.zero || !CanAct())
        {
            DashIndicator.Hide();
            return;
        }

        Vector3 direction = ToWorldDirection(in dir);
        Vector3 origin = feetTransform.position;
        float distance = ComputeDashDistance(ratio);

        if (TryRaycastObstacle(in origin, in direction, ref distance))
        {
            
        }

        Vector3 targetPos = origin + direction * distance;
        DashIndicator.Show(origin, targetPos);
    }

    private void HandleChargeStarted()
    {
        if (Chain.ChainCount <= 0) return;

        TimeController.LerpTimeDilationAsync(ActiveStats.SlowMotionTargetDilation, ActiveStats.SlowMotionEnterDuration).Forget();
        Aspect.StartSlowMotionVisualsAsync();
    }

    private void HandleChargeEnd()
    {
        DashIndicator.Hide();
        TimeController.LerpTimeDilationAsync(1f, ActiveStats.SlowMotionExitDuration).Forget();
        Aspect.CancelSlowMotionVisualsAsync();
    }

    private void HandleSwipe(Vector2 dir, float ratio)
    {
        if (!CanAct() || dir == Vector2.zero) return;

        Vector3 moveDir = ToWorldDirection(in dir);
        m_currentDashContext = new DashContext(in moveDir, ActiveStats.DashDistance);
        RequestRoll();
    }

    private void HandleChainSuccess()
    {
        ServiceLocator.Get<FxService>().PlayChainSuccessEffect(TransformCache, 1.5f);
    }

    #endregion

    #region Event Bus Handlers

    private void HandleActorPushed(ActorPushedEvent e)
    {
        if (e.Target != this) return;
        if (m_stateMachine.GetCurrentState() is PlayerHitState || !HitGate.CanTakeDamage()) return;

        m_pendingKnockbackDirection = e.Direction;
        m_pendingKnockbackForce = KnockbackUtility.CalculateKnockbackDistance(e.Force, ActiveStats.Weight, ActiveStats.DefaultScaling, Stability);
        Stability = KnockbackUtility.CalculateNewStability(e.Destabilization, e.Force, ActiveStats.Weight, Stability);

        RequestHit();
        DamagePopUp.Show(m_pendingKnockbackForce, TransformCache.position);
    }

    #endregion

    #region Public State Control

    public void RequestDash() => m_wantsDash = true;
    public void ConsumeDashRequest() => m_wantsDash = false;

    public void RequestRoll() => m_wantsRoll = true;
    public void ConsumeRollRequest() => m_wantsRoll = false;

    public void RequestHit() => m_hitRequested = true;
    public void ConsumeHitRequest() => m_hitRequested = false;

    public void SetIdle() => m_stateMachine.SetState(m_playerIdleState);
    public void ResetDashDistance() => m_currentDashContext = new DashContext(m_currentDashContext.Direction, ActiveStats.DashDistance);

    public override void Kill()
    {
        if (m_isDead) return;
        m_isDead = true;
        ServiceLocator.Get<GameService>().EndRun();
    }

    #endregion

    #region Pure Utilities

    private bool CanAct() => m_stateMachine.GetCurrentState() is PlayerIdleState;

    private float ComputeDashDistance(float ratio) => ActiveStats.DashDistance * Mathf.Clamp01(ratio);

    private static Vector3 ToWorldDirection(in Vector2 dir) => new(dir.x, 0f, dir.y);

    private bool TryRaycastObstacle(in Vector3 origin, in Vector3 direction, ref float distance)
    {
        if (Physics.Raycast(origin + Vector3.up * 0.5f, direction, out RaycastHit hit, distance, ActiveStats.ObstacleLayer, QueryTriggerInteraction.Ignore))
        {
            distance = hit.distance;
            return true;
        }
        return false;
    }

    #endregion
}