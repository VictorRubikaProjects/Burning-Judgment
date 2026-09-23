using System;
using Event_Bus;
using UnityEngine;

public class EnemyShooter : Enemy
{
    [field:SerializeField] public Transform ShootPoint { get; private set; }
    [field:SerializeField] public SO_ConfigEnemyShoot Stats { get; private set; }
    
    private EnemyShooterChaseState m_chaseState;
    private EnemyShooterFleeState m_fleeState;
    private EnemyShooterHitState m_hitState;
    private EnemyShooterIdleState m_idleState;
    
    private bool m_hitRequested;
    private bool m_chaseRequested;
    private bool m_fleeRequested;

    private EventBinding<ActorPushedEvent> m_eventBindingPushActor;

    private EnemyShootComponent Shoot;
    
    public Vector3 PendingKnockbackDirection {get; private set;}
    public float PendingKnockbackForce {get; private set;}

    protected override void Awake()
    {
        base.Awake();
        Shoot = new EnemyShootComponent(this);
    }

    protected override void Start()
    {
        base.Start();
        AddActorComponent(Shoot);
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
        m_hitState = new EnemyShooterHitState(this);
        m_idleState = new EnemyShooterIdleState(this);
        
        return m_idleState;
    }

    protected override void SetupTransitions()
    {
        At(m_hitState, m_idleState, new FuncPredicate(() => m_hitState.IsFinished));
        
        Any(m_fleeState,new FuncPredicate(() => m_fleeRequested));
        
        Any(m_chaseState,new FuncPredicate(() => m_chaseRequested));
        
        Any(m_hitState, new FuncPredicate(() => m_hitRequested));
    }
    
    
    public void RequestHit() => m_hitRequested = true;
    
    public void ConsumeHitRequest() => m_hitRequested = false;
    
    public void RequestFlee() => m_fleeRequested = true;
    
    public void ConsumeFleeRequest() => m_fleeRequested = false;
    
    public void RequestChase() => m_chaseRequested = true;
    
    public void ConsumeChaseRequest() => m_chaseRequested = false;
    
    private void OnActorPushed(ActorPushedEvent e)
    {
        if (e.Target != this) return;
        
        if (m_stateMachine.GetCurrentState() is PlayerHitState) return;

        PendingKnockbackDirection = e.Direction;
        
        PendingKnockbackForce = KnockbackUtility.CalculateKnockbackDistance(e.Force, Stats.Weight, Stats.KnockbackResistance);
        
        RequestHit();
    }
}