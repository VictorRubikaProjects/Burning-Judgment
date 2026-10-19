using System;
using Event_Bus;
using Helpers.Runtime.Math;
using UnityEngine;

public class PlayerCharacter : Actor
{
    #region Variable

    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform feetTransform;
    [SerializeField] private SpriteRenderer dashIndicatorSprite;
    
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
    public PlayerBounceComponent Bounce { get; private set; }
    public DashIndicatorComponent DashIndicator { get; private set; }

    public Vector3 MoveDir => m_moveDir;
    public float DashDistance => m_dashDistance;
    public Vector3 PendingKnockbackDirection => m_pendingKnockbackDirection;
    public float PendingKnockbackForce => m_pendingKnockbackForce;
    public bool HasHitRequest => m_hitRequested;
    public Transform FeetTransform => feetTransform;
    
    private StateMachine m_stateMachine;

    private Vector3 m_moveDir;
    private Vector3 m_attackDirection;
    private Vector3 m_pendingKnockbackDirection;
    private float m_dashDistance;
    
    private bool m_wantsDash;
    private bool m_wantsAttack;
    private bool m_isDead;
    private bool m_hitRequested;
    private bool m_canCancelRecover;
    private bool m_recoverCancelRequested;
    private bool m_isAttackReadyShown;
    
    private PlayerIdleState m_playerIdleState;
    private PlayerDeathState m_playerDeathState;
    private PlayerRollState m_playerRollState;
    private PlayerHitState m_playerHitState;
    private PlayerFallState m_playerFallState;
    private PlayerRecoverState m_playerRecoverState;
    
    private Transform m_transformCache;
    
    private float m_pendingKnockbackForce;
    
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

        SetupStateMachine();

        GroundCheck = new GroundCheckComponent(
            this,
            transform,
            ActiveStats.GroundLayer,
            ActiveStats.GroundCheckHeight,
            ActiveStats.GroundCheckDistance);

        KnockBack = new KnockbackComponent(this, rb);
        HitGate = new HitGateComponent(this, ActiveStats.InvincibilityDuration);
        Dash = new DashComponent(this, rb, GroundCheck);
        Input = new PlayerInputComponent(owner: this, ActiveStats);
        Controller = new PlayerControllerComponent(owner: this, rb);
        Aspect = new PlayerAspectComponent(owner: this, playerMaterial, playerRenderer, ActiveStats);
        Bounce = new PlayerBounceComponent(this, ActiveStats);
        DashIndicator = new DashIndicatorComponent(this, dashIndicatorSprite);
    }

    protected override void Start()
    {
        base.Start();
    
        AddActorComponent(GroundCheck);
        AddActorComponent(KnockBack);
        AddActorComponent(HitGate);
        AddActorComponent(Input);
        AddActorComponent(Controller);
        AddActorComponent(Aspect);
        AddActorComponent(Bounce);
        AddActorComponent(DashIndicator);
    
        ServiceLocator.Get<CameraService>().AddTarget(transform,1f);
    }

    private void OnEnable()
    {
        Input.OnJoystickRelease += JoyStickRelease;
        Input.OnChargeUpdate += OnChargeUpdate;
        Input.OnChargeEnd += OnChargeEnd;
        
        m_playerPushedBinding = new EventBinding<ActorPushedEvent>(OnActorPushed);
        EventBus<ActorPushedEvent>.Register(m_playerPushedBinding);
    }

    private void OnDisable()
    {
        Input.OnJoystickRelease -= JoyStickRelease;
        Input.OnChargeUpdate -= OnChargeUpdate;
        Input.OnChargeEnd -= OnChargeEnd;
        
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

    private void OnDestroy()
    {
        if (m_runtimeStats) Destroy(m_runtimeStats);
    }

    #endregion

    #region State Machine

    void At(IState from, IState to, IPredictate condition) => m_stateMachine.AddTransition(from, to, condition);
    void Any(IState to, IPredictate condition) => m_stateMachine.AddAnyTransition(to, condition);
    
    private void SetupStateMachine()
    {
        m_stateMachine = new StateMachine();

        m_playerIdleState = new PlayerIdleState(this, animator, ActiveStats);
        m_playerRollState = new PlayerRollState(this, animator, rb, ActiveStats);
        m_playerDeathState = new PlayerDeathState(this, animator, ActiveStats);
        m_playerHitState = new PlayerHitState(this, animator, ActiveStats);
        m_playerFallState = new PlayerFallState(this, animator, ActiveStats, rb);
        m_playerRecoverState = new PlayerRecoverState(this, animator, ActiveStats);

        At(
            m_playerIdleState,
            m_playerRollState,
            new FuncPredicate(() => m_wantsDash)
        );

        At(
            m_playerRollState,
            m_playerRecoverState,
            new FuncPredicate(() =>
                m_playerRollState.IsFinished &&
                m_playerRollState.HasHit
            )
        );

        At(
            m_playerRollState,
            m_playerIdleState,
            new FuncPredicate(() =>
                m_playerRollState.IsFinished &&
                !m_playerRollState.HasHit
            )
        );

        At(
            m_playerRecoverState,
            m_playerRollState,
            new FuncPredicate(() =>
                m_recoverCancelRequested &&
                m_wantsDash
            )
        );

        At(
            m_playerRecoverState,
            m_playerIdleState,
            new FuncPredicate(() =>
                m_playerRecoverState.IsFinished &&
                !m_recoverCancelRequested
            )
        );

        At(
            m_playerHitState,
            m_playerIdleState,
            new FuncPredicate(() => m_playerHitState.IsFinished)
        );

        Any(
            m_playerDeathState,
            new FuncPredicate(() => m_isDead)
        );

        Any(
            m_playerHitState,
            new FuncPredicate(() => HasHitRequest)
        );

        Any(
            m_playerFallState,
            new FuncPredicate(() => !GroundCheck.IsGrounded())
        );

        m_stateMachine.SetState(m_playerIdleState);
    }

    #endregion
    
    #region Input Handlers

    private void JoyStickRelease(Vector2 dir, float ratio)
    {
        if (!CanSwipe()) return;

        m_moveDir = new Vector3(dir.x, 0f, dir.y);
        m_dashDistance = ComputeDashDistance(ratio);

        RequestDash();
    }
    
    private void OnChargeUpdate(Vector2 dir, float ratio)
    {
        if (dir == Vector2.zero || !CanSwipe())
        {
            DashIndicator.Hide();
            return;
        }

        Vector3 direction = new Vector3(dir.x, 0f, dir.y);
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

    private void OnChargeEnd() => DashIndicator.Hide();

    #endregion

    #region Event Bus Handlers

    private void OnActorPushed(ActorPushedEvent e)
    {
        if (e.Target != this) return;

        if (m_stateMachine.GetCurrentState() is PlayerHitState) return;

        m_pendingKnockbackDirection = e.Direction;
        m_pendingKnockbackForce = KnockbackUtility.CalculateKnockbackDistance(e.Force, baseStats.Weight);
        RequestHit();
    }

    #endregion

    #region Helpers

    public void ConsumeDashRequest() => m_wantsDash = false;
    public void ConsumeAttackRequest() => m_wantsAttack = false;
    public void RequestDash()=> m_wantsDash = true;
    public void RequestAttack()=> m_wantsAttack = true;

    public override void Kill()
    {
        m_isDead = true;
        ServiceLocator.Get<GameService>().EndRun();
    }

    public void RequestHit() => m_hitRequested = true;
    
    public void ConsumeHitRequest() => m_hitRequested = false;
    
    public void RequestRecoverCancel() => m_recoverCancelRequested = true;
    public void SetCanCancelRecover(bool value) => m_canCancelRecover = value;
    
    public void ResetRecoverCancel()
    {
        m_canCancelRecover = false;
        m_recoverCancelRequested = false;
    }

    public void SetIdle() => m_stateMachine.SetState(m_playerIdleState);
    
    private bool CanAttack() => m_stateMachine.GetCurrentState() is PlayerIdleState || Bounce.CanBounce;
    private bool CanSwipe() => m_stateMachine.GetCurrentState() is PlayerIdleState;
    
    private float ComputeDashDistance(float ratio) => ActiveStats.DashDistance * Mathf.Clamp01(ratio);
    
    public void ResetDashDistance() => m_dashDistance = ActiveStats.DashDistance;
    
    #endregion
    
#if UNITY_EDITOR    
    
    #region Debug
    
    void OnGUI()
    {
        Rect rect = new Rect(Screen.width - 400, 10, 200, 40);
        
        string text = $"{m_stateMachine.GetCurrentState()}";
        
        GUIStyle style = new GUIStyle
        {
            fontSize = 40,
            fontStyle = FontStyle.Bold,
            normal =
            {
                textColor = Color.white
            }
        };

        GUI.Box(rect,text,style);
    }

    #endregion
    
#endif
    
}