using UnityEngine;

public class PlayerDeathState : PlayerBaseState
{
    public PlayerDeathState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats) : base(owner, animator,playerStats) { }

    public override void OnEnter()
    {
        m_animator.CrossFade(DeathHash, m_crossFadeDuration);
    }
}