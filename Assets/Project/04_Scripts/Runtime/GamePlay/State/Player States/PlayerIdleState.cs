using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats) : base(
        owner, animator, playerStats) { }

    
    public override void OnEnter()
    {
        m_animator.CrossFade(IdleHash, m_crossFadeDuration);
    }

}