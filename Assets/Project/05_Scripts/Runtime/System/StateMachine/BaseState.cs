using UnityEngine;

public abstract class BaseState : IState
{
    protected readonly PlayerCharacter m_owner;
    protected readonly Animator m_animator;
    
    protected static readonly int IdleHash = Animator.StringToHash("Idle");
    protected static readonly int DashHash = Animator.StringToHash("Dash");
    protected static readonly int DeathHash = Animator.StringToHash("Death");
    protected static readonly int HitHash = Animator.StringToHash("Hit");

    protected readonly float m_crossFadeDuration;
    
    protected readonly ConfigStatsPlayer m_configStats; 

    protected BaseState(PlayerCharacter owner, Animator animator, ConfigStatsPlayer configStats)
    {
        m_owner = owner;
        m_animator = animator;
        m_configStats = configStats;
        m_crossFadeDuration = configStats.CrossFadeDuration;
    }
    
    public virtual void OnEnter()
    {
        
    }

    public virtual void OnExit()
    {
        
    }

    public virtual void Update()
    {
        
    }

    public virtual void FixedUpdate()
    {
        
    }
}
