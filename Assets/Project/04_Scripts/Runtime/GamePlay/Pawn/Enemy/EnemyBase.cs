using Event_Bus;
using UnityEngine;
using UnityEngine.AI;


public abstract class EnemyBase : Actor , IStrikeable
{
    #region Variables

    [SerializeField] private NavMeshAgent agent;
    [field:SerializeField] public Rigidbody Rigidbody { get; protected set; }
    [field: SerializeField] public DamagePopUp  DamagePopUp { get; protected set; }

    public abstract SO_ConfigEnemy StatsBase { get; }

    public NavMeshAgent Agent => agent;

    protected StateMachine m_stateMachine;

    protected bool m_hitRequested;

    private bool m_chaseRequested;
    private bool m_fleeRequested;

    public bool HitRequested => m_hitRequested;
    public bool ChaseRequested => m_chaseRequested;
    public bool FleeRequested => m_fleeRequested;

    public Vector3 PendingKnockbackDirection { get; private set; }
    public float PendingKnockbackForce { get; private set; }

    public PlayerCharacter Player { get; private set; }

    public HitGateComponent HitGate { get; private set; }
    public KnockbackComponent KnockBack { get; private set; }
    public GroundCheckComponent GroundCheck { get; private set; }

    private EventBinding<ActorPushedEvent> m_eventBindingPushActor;

    #endregion

    protected override void Awake()
    {
        base.Awake();

        ServiceLocator.Get<CameraService>().AddTarget(TransformCache, 0.5f);

        GroundCheck = new GroundCheckComponent(
            this,
            transform,
            StatsBase.GroundLayer,
            StatsBase.GroundCheckHeight,
            StatsBase.GroundCheckDistance);

        KnockBack = new KnockbackComponent(this, Rigidbody);

        HitGate = new HitGateComponent(this, StatsBase.InvincibilityDuration);

        Agent.speed = StatsBase.Speed;
        Agent.updateRotation = false;

        SetupStateMachine();
    }

    protected override void Start()
    {
        base.Start();
        AddActorComponent(GroundCheck);
        AddActorComponent(KnockBack);
        AddActorComponent(HitGate);
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

    private void OnEnable()
    {
        m_eventBindingPushActor = new EventBinding<ActorPushedEvent>(OnActorPushed);
        EventBus<ActorPushedEvent>.Register(m_eventBindingPushActor);
    }

    private void OnDisable()
    {
        EventBus<ActorPushedEvent>.Unregister(m_eventBindingPushActor);
    }

    public override void Kill()
    {
        base.Kill();
        ServiceLocator.Get<CameraService>().RemoveTarget(TransformCache);
        EventBus<EnemyDiedEvent>.Raise(new EnemyDiedEvent(this));
        Destroy(gameObject);
    }

    #region StateMachine

    protected void At(IState from, IState to, IPredictate condition) => m_stateMachine.AddTransition(from, to, condition);
    protected void Any(IState to, IPredictate condition) => m_stateMachine.AddAnyTransition(to, condition);

    private void SetupStateMachine()
    {
        m_stateMachine = new StateMachine();

        IState state = SetupStates();

        SetupTransitions();

        m_stateMachine.SetState(state);
    }

    protected abstract IState SetupStates();

    protected virtual void SetupTransitions() { }

    public void SetupPlayerRef(PlayerCharacter player) => Player = player;

    #endregion

    protected void OnActorPushed(ActorPushedEvent e)
    {
        if (e.Target != this) return;

        if (m_stateMachine.GetCurrentState() is EnemyHitState) return;

        ApplyPush(e.Direction, e.Force);
    }

    public void LookAtPlayer()
    {
        Vector3 direction = Player.TransformCache.position - TransformCache.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        TransformCache.rotation = Quaternion.RotateTowards(
            TransformCache.rotation,
            targetRotation,
            StatsBase.RotationSpeed * Time.deltaTime);
    }

    public void RequestHit() => m_hitRequested = true;
    public void ConsumeHitRequest() => m_hitRequested = false;

    public void RequestChase() => m_chaseRequested = true;
    public void ConsumeChaseRequest() => m_chaseRequested = false;

    public void RequestFlee() => m_fleeRequested = true;
    public void ConsumeFleeRequest() => m_fleeRequested = false;
    public Transform TargetTransform => TransformCache;
    public bool CanBeStruck => m_stateMachine.GetCurrentState() is not (EnemyHitState or EnemyFallState);
    
    public bool IsPlayerInSight()
    {
        if (!Player) return false;

        Vector3 offset = Vector3.up * StatsBase.SightHeight;
        Vector3 origin = TransformCache.position + offset;
        Vector3 target = Player.TransformCache.position + offset;

        return !Physics.Linecast(origin, target, StatsBase.ObstacleLayer, QueryTriggerInteraction.Ignore);
    }

    public virtual StrikeResult OnStruck(in StrikeContext context)
    {
        if (!CanBeStruck) return StrikeResult.Ignored;

        ApplyPush(context.Direction, context.Force);
        return new StrikeResult(accepted: true, cancelsDash: true, allowsChain: true);
    }

    private void ApplyPush(Vector3 direction, float force)
    {
        PendingKnockbackDirection = direction;
        PendingKnockbackForce = KnockbackUtility.CalculateKnockbackDistance(force, StatsBase.Weight);
        RequestHit();
        DamagePopUp.Show(force, TransformCache.position);
    }
}