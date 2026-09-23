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
    
    private bool m_hitRequested;
    private bool m_chaseRequested;
    private bool m_fleeRequested;

    private EventBinding<ActorPushedEvent> m_eventBindingPushActor;

    public EnemyShootComponent Shoot { get;private set; }
    
    public Vector3 PendingKnockbackDirection {get; private set;}
    public float PendingKnockbackForce {get; private set;}

    protected override void Awake()
    {
        Shoot = new EnemyShootComponent(this);
        Agent.speed = Stats.Speed;
        Agent.updateRotation = false;
        base.Awake();
    }

    protected override void Start()
    {
        AddActorComponent(Shoot);
        
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
        
        return m_idleState;
    }

    protected override void SetupTransitions()
    {
        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));
        At(m_fleeState, m_idleState, new FuncPredicate(() => m_fleeState.IsFinished));
        At(m_chaseState, m_idleState, new FuncPredicate(() => m_chaseState.IsFinished));
        
        Any(m_fleeState,new FuncPredicate(() => m_fleeRequested));
        
        Any(m_chaseState,new FuncPredicate(() => m_chaseRequested));
        
        Any(m_hitState, new FuncPredicate(() => m_hitRequested));
    }
    
    
    public void ConsumeHitRequest() => m_hitRequested = false;
    public void ConsumeFleeRequest() => m_fleeRequested = false;
    public void ConsumeChaseRequest() => m_chaseRequested = false;
    public void RequestHit() { Debug.Log($"[{name}] RequestHit"); m_hitRequested = true; }
    public void RequestFlee() { Debug.Log($"[{name}] RequestFlee"); m_fleeRequested = true; }
    public void RequestChase() { Debug.Log($"[{name}] RequestChase"); m_chaseRequested = true; }
    
    
    private void OnActorPushed(ActorPushedEvent e)
    {
        if (e.Target != this) return;
        Debug.Log($"[{name}] OnActorPushed, currentState={m_stateMachine.GetCurrentState()?.GetType().Name}");

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