using UnityEngine;

public class PlayerIdleState : BaseState
{
    public PlayerIdleState(PlayerCharacter owner, Animator animator, ConfigStatsPlayer configStats) : base(
        owner, animator, configStats) { }

    
    public override void OnEnter()
    {
        m_animator.CrossFade(IdleHash, m_crossFadeDuration);
    }

}