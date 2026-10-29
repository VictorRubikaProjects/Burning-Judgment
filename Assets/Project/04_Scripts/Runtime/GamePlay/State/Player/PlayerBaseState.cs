using UnityEngine;

public abstract class PlayerBaseState : IState
{
    protected readonly PlayerCharacter m_owner;
    protected readonly Animator m_animator;
    protected readonly SO_PlayerStats m_stats;

    protected static readonly int IdleHash = Animator.StringToHash("Idle");
    protected static readonly int RollHash = Animator.StringToHash("Roll");
    protected static readonly int DashHash = Animator.StringToHash("Dash");
    protected static readonly int FallHash = Animator.StringToHash("Fall");
    protected static readonly int HitHash = Animator.StringToHash("Hit");
    protected static readonly int DeathHash = Animator.StringToHash("Death");

    private readonly StateAnimationData m_animationData;

    protected PlayerBaseState(
        PlayerCharacter owner,
        Animator animator,
        SO_PlayerStats playerStats,
        StateAnimationData animationData)
    {
        m_owner = owner;
        m_animator = animator;
        m_stats = playerStats;
        m_animationData = animationData;
    }

    protected void PlayAnimation(int hash)
    {
        m_animator.speed = m_animationData.Speed;
        m_animator.CrossFade(hash, m_animationData.CrossFadeDuration);
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void Update() { }
    public virtual void FixedUpdate() { }
}