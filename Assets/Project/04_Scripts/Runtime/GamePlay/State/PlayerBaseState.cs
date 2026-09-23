using UnityEngine;

public abstract class PlayerBaseState : IState
{
    protected readonly PlayerCharacter m_owner;
    protected readonly Animator m_animator;
    
    protected static readonly int IdleHash = Animator.StringToHash("Idle");
    protected static readonly int DashHash = Animator.StringToHash("Dash");
    protected static readonly int DeathHash = Animator.StringToHash("Death");
    protected static readonly int HitHash = Animator.StringToHash("Hit");

    protected readonly float m_crossFadeDuration;
    
    protected readonly SO_PlayerStats m_stats; 

    protected PlayerBaseState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats)
    {
        m_owner = owner;
        m_animator = animator;
        m_stats = playerStats;
        m_crossFadeDuration = playerStats.CrossFadeDuration;
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
