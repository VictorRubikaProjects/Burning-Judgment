using System;
using Event_Bus;
using UnityEngine;

public class EnemyShooter : Enemy
{
    [field:SerializeField] public Transform ShootPoint { get; private set; }
    
    private EnemyChaseState m_chaseState;
    private EnemyFleeState m_fleeState;
    private EnemyHitState m_hitState;
    private EnemyIdleState m_idleState;
    
    private bool m_hitRequested;
    private bool m_chaseRequested;
    private bool m_fleeRequested;

    private EventBinding<ActorPushedEvent> m_eventBindingPushActor;

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
        m_chaseState = new EnemyChaseState(this);
        m_fleeState = new EnemyFleeState(this);
        m_hitState = new EnemyHitState(this);
        m_idleState = new EnemyIdleState(this);
        
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

        //m_pendingKnockbackDirection = e.Direction;
        //m_pendingKnockbackForce = KnockbackUtility.CalculateKnockbackDistance(e.Force, stats.Weight, stats.KnockbackResistance);
        RequestHit();
    }
}