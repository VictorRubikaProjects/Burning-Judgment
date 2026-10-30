using Event_Bus;
using UnityEngine;

public class PlayerCharacter : Actor
{
    #region Variable

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform feetTransform;
    [SerializeField] private SpriteRenderer dashIndicatorSprite;
    [SerializeField] private RectTransform joystickRectT;
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

    public Vector3 MoveDir => m_moveDir;
    public Vector3 PendingKnockbackDirection => m_pendingKnockbackDirection;

    public float DashDistance => m_dashDistance;
    public float PendingKnockbackForce => m_pendingKnockbackForce;

    public Transform FeetTransform => feetTransform;

    private StateMachine m_stateMachine;

    private Vector3 m_moveDir;
    private Vector3 m_pendingKnockbackDirection;

    private float m_dashDistance;
    private float m_pendingKnockbackForce;

    private bool m_wantsDash;
    private bool m_wantsRoll;
    private bool m_isDead;
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

        AugmentService augmentService = ServiceLocator.Get<AugmentService>();

        AugmentDeltas deltas = default;

        if (augmentService == null)
        {
            Debug.LogError("Augment Service null in PlayerCharacter Awake");
        }
        else
        {
            deltas = augmentService.Total;
        }

        m_runtimeStats = baseStats.CreateRuntimeCopy(deltas);

        GroundCheck = new GroundCheckComponent(
            this,
            transform,
            ActiveStats.GroundLayer,
            ActiveStats.GroundCheckHeight,
            ActiveStats.GroundCheckDistance);

        TimeController = new TimeControllerComponent(this);
        KnockBack = new KnockbackComponent(this, rb);
        HitGate = new HitGateComponent(this, ActiveStats.InvincibilityDuration);
        Dash = new DashComponent(this, rb, GroundCheck);
        Input = new PlayerInputComponent(owner: this, ActiveStats, joystickRectT, TimeController);
        Controller = new PlayerControllerComponent(owner: this, rb, ActiveStats);
        Aspect = new PlayerAspectComponent(owner: this, playerMaterial, playerRenderer, ActiveStats);
        Chain = new PlayerChainComponent(this, ActiveStats, Input);
        DashIndicator = new DashIndicatorComponent(this, dashIndicatorSprite);

        SetupStateMachine();
    }

    protected override void Start()
    {
        base.Start();

        AddActorComponent(TimeController);
        AddActorComponent(GroundCheck);
        AddActorComponent(KnockBack);
        AddActorComponent(HitGate);
        AddActorComponent(Input);
        AddActorComponent(Controller);
        AddActorComponent(Aspect);
        AddActorComponent(Chain);
        AddActorComponent(DashIndicator);

        ServiceLocator.Get<CameraService>().AddTarget(transform, 1f);
    }

    private void OnEnable()
    {
        Input.OnJoystickRelease += HandleJoystickRelease;
        Input.OnChargeUpdate += HandleChargeUpdate;
        Input.OnChargeEnd += HandleChargeEnd;
        Input.OnSwipe += HandleSwipe;

        Chain.OnChainInputSuccess += HandleChainSuccess;

        m_playerPushedBinding = new EventBinding<ActorPushedEvent>(HandleActorPushed);
        EventBus<ActorPushedEvent>.Register(m_playerPushedBinding);
    }

    private void OnDisable()
    {
        Input.OnJoystickRelease -= HandleJoystickRelease;
        Input.OnChargeUpdate -= HandleChargeUpdate;
        Input.OnChargeEnd -= HandleChargeEnd;
        Input.OnSwipe -= HandleSwipe;

        Chain.OnChainInputSuccess -= HandleChainSuccess;

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

    #region State Machine

    private void At(IState from, IState to, IPredictate condition) => m_stateMachine.AddTransition(from, to, condition);
    private void Any(IState to, IPredictate condition) => m_stateMachine.AddAnyTransition(to, condition);

    private void SetupStateMachine()
    {
        m_stateMachine = new StateMachine();

        m_playerIdleState = new PlayerIdleState(this, animator, ActiveStats);
        m_playerDashState = new PlayerDashState(this, animator, ActiveStats);
        m_playerDeathState = new PlayerDeathState(this, animator, ActiveStats);
        m_playerHitState = new PlayerHitState(this, animator, ActiveStats);
        m_playerFallState = new PlayerFallState(this, animator, ActiveStats, rb);
        m_playerRollState = new PlayerRollState(this, animator, ActiveStats);

        At(m_playerIdleState, m_playerDashState, new FuncPredicate(() => m_wantsDash));
        At(m_playerIdleState, m_playerRollState, new FuncPredicate(() => m_wantsRoll));

        At(m_playerDashState, m_playerIdleState, new FuncPredicate(() => m_playerDashState.IsFinished));
        At(m_playerRollState, m_playerIdleState, new FuncPredicate(() => m_playerRollState.IsFinished));
        At(m_playerHitState, m_playerIdleState, new FuncPredicate(() => m_playerHitState.IsFinished));

        Any(m_playerDeathState, new FuncPredicate(() => m_isDead));
        Any(m_playerHitState, new FuncPredicate(() => m_hitRequested));
        Any(m_playerFallState, new FuncPredicate(() => !GroundCheck.IsGrounded()));

        m_stateMachine.SetState(m_playerIdleState);
    }

    #endregion

    #region Input Handlers

    private void HandleJoystickRelease(Vector2 dir, float ratio)
    {
        if (!CanAct()) return;

        m_moveDir = ToWorldDirection(dir);
        m_dashDistance = ComputeDashDistance(ratio);

        RequestDash();
    }

    private void HandleChargeUpdate(Vector2 dir, float ratio)
    {
        if (dir == Vector2.zero || !CanAct())
        {
            DashIndicator.Hide();
            return;
        }

        Vector3 direction = ToWorldDirection(dir);
        Vector3 origin = feetTransform.position;
        float distance = ComputeDashDistance(ratio);

        if (Physics.Raycast(
                origin + Vector3.up * 0.5f,
                direction,
                out RaycastHit hit,
                distance,
                ActiveStats.ObstacleLayer,
                QueryTriggerInteraction.Ignore))
        {
            distance = hit.distance;
        }

        DashIndicator.Show(origin, origin + direction * distance);
    }

    private void HandleChargeEnd() => DashIndicator.Hide();

    private void HandleSwipe(Vector2 dir, float ratio)
    {
        if (!CanAct() || dir == Vector2.zero) return;

        m_moveDir = ToWorldDirection(dir);
        RequestRoll();
    }

    private void HandleChainSuccess()
        => ServiceLocator.Get<FxService>().PlayChainSuccessEffect(TransformCache, 1.5f);

    #endregion

    #region Event Bus Handlers

    private void HandleActorPushed(ActorPushedEvent e)
    {
        if (e.Target != this) return;

        if (m_stateMachine.GetCurrentState() is PlayerHitState) return;

        m_pendingKnockbackDirection = e.Direction;
        m_pendingKnockbackForce = KnockbackUtility.CalculateKnockbackDistance(e.Force, ActiveStats.Weight);
        RequestHit();
        DamagePopUp.Show(e.Force, TransformCache.position);
    }

    #endregion

    #region Helpers

    public void RequestDash() => m_wantsDash = true;
    public void ConsumeDashRequest() => m_wantsDash = false;

    public void RequestRoll() => m_wantsRoll = true;
    public void ConsumeRollRequest() => m_wantsRoll = false;

    public void RequestHit() => m_hitRequested = true;
    public void ConsumeHitRequest() => m_hitRequested = false;

    public void SetIdle() => m_stateMachine.SetState(m_playerIdleState);

    public void ResetDashDistance() => m_dashDistance = ActiveStats.DashDistance;

    public override void Kill()
    {
        m_isDead = true;
        ServiceLocator.Get<GameService>().EndRun();
    }

    private bool CanAct() => m_stateMachine.GetCurrentState() is PlayerIdleState;

    private float ComputeDashDistance(float ratio) => ActiveStats.DashDistance * Mathf.Clamp01(ratio);

    private static Vector3 ToWorldDirection(Vector2 dir) => new(dir.x, 0f, dir.y);

    #endregion

#if UNITY_EDITOR

    #region Debug

    private void OnGUI()
    {
        Rect rect = new Rect(Screen.width - 400, 10, 200, 40);

        string text = $"{m_stateMachine.GetCurrentState()}";

        GUIStyle style = new GUIStyle
        {
            fontSize = 40,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        GUI.Box(rect, text, style);
    }

    #endregion

#endif
}