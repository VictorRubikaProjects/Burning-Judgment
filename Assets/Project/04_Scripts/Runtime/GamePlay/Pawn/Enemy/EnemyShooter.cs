using System;
using Event_Bus;
using UnityEngine;

public class EnemyShooter : Enemy
{
    [field:SerializeField] public Transform ShootPoint { get; private set; }
    [field:SerializeField] public SO_ConfigEnemyShoot Stats { get; private set; }
    [field:SerializeField] public Rigidbody Rigidbody { get; private set; }
    
    private EnemyShooterChaseState m_chaseState;
    private EnemyShooterFleeState m_fleeState;
    private EnemyShooterHitState m_hitState;
    private EnemyShooterIdleState m_idleState;
    private EnemyShooterFallState m_fallState;
    
    private bool m_hitRequested;
    private bool m_chaseRequested;
    private bool m_fleeRequested;
    
    private EventBinding<ActorPushedEvent> m_eventBindingPushActor;
    
    public HitGateComponent HitGate { get; private set; }
    public KnockbackComponent KnockBack { get; private set; }
    public GroundCheckComponent GroundCheck { get; private set; }
    public EnemyShootComponent Shoot { get;private set; }
    
    public Vector3 PendingKnockbackDirection {get; private set;}
    public float PendingKnockbackForce {get; private set;}

    protected override void Awake()
    {
        Shoot = new EnemyShootComponent(this);
        
        GroundCheck = new GroundCheckComponent(
            this,
            transform,
            Stats.GroundLayer,
            Stats.GroundCheckHeight,
            Stats.GroundCheckDistance);
        
        KnockBack = new KnockbackComponent(this, Rigidbody);
        
        HitGate = new HitGateComponent(this, Stats.InvincibilityDuration);
        
        Agent.speed = Stats.Speed;
        Agent.updateRotation = false;
        
        base.Awake();
    }

    protected override void Start()
    {
        AddActorComponent(Shoot);
        AddActorComponent(GroundCheck);
        AddActorComponent(KnockBack);
        AddActorComponent(HitGate);
        
        base.Start();
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

    protected override IState SetupStates()
    {
        m_chaseState = new EnemyShooterChaseState(this);
        m_fleeState = new EnemyShooterFleeState(this);
        m_hitState = new EnemyShooterHitState(this,Rigidbody);
        m_idleState = new EnemyShooterIdleState(this);
        m_fallState = new EnemyShooterFallState(this);
    
        return m_idleState;
    }

    protected override void SetupTransitions()
    {
        Any(m_fallState, new FuncPredicate(() => !GroundCheck.IsGrounded()));

        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));
        At(m_fleeState, m_idleState, new FuncPredicate(() => m_fleeState.IsFinished));
        At(m_chaseState, m_idleState, new FuncPredicate(() => m_chaseState.IsFinished));
    
        Any(m_fleeState, new FuncPredicate(() => m_fleeRequested && GroundCheck.IsGrounded()));
        Any(m_chaseState, new FuncPredicate(() => m_chaseRequested && GroundCheck.IsGrounded()));
        Any(m_hitState, new FuncPredicate(() => m_hitRequested && GroundCheck.IsGrounded()));
    }
    
    
    public void ConsumeHitRequest() => m_hitRequested = false;
    public void ConsumeFleeRequest() => m_fleeRequested = false;
    public void ConsumeChaseRequest() => m_chaseRequested = false;
    public void RequestHit() => m_hitRequested = true;
    public void RequestFlee() => m_fleeRequested = true;
    public void RequestChase() => m_chaseRequested = true;


    private void OnActorPushed(ActorPushedEvent e)
    {
        if (e.Target != this) return;

        if (m_stateMachine.GetCurrentState() is EnemyShooterHitState) return;

        PendingKnockbackDirection = e.Direction;
        PendingKnockbackForce = KnockbackUtility.CalculateKnockbackDistance(e.Force, Stats.Weight, Stats.KnockbackResistance);
        RequestHit();
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
            Stats.RotationSpeed * Time.deltaTime); 
    }
}