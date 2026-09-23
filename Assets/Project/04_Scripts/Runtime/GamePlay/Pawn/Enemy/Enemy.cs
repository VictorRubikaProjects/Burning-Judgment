using UnityEngine;
using UnityEngine.AI;

public class Enemy : Actor
{
    #region Varaibles

    [SerializeField] private NavMeshAgent agent;
    public Transform TransformCache { get; private set; }
    
    public NavMeshAgent Agent => agent;
    
    protected StateMachine m_stateMachine;

    [field: SerializeField] public PlayerCharacter Player;

    #endregion

    protected override void Awake()
    {
        base.Awake();
        
        SetupStateMachine();
        
        TransformCache = transform;
    }

    protected override void Update()
    {
        base.Update();
        m_stateMachine.Update();
    }
    
    protected override void FixedUpdate()
    {
        m_stateMachine.FixedUpdate();
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

    protected virtual IState SetupStates()
    {
        return null;
    }

    protected virtual void SetupTransitions()
    {
        
    }

    #endregion
}