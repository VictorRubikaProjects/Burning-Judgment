using UnityEngine;

public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats, playerStats.IdleAnimation) { }

    public override void OnEnter() => PlayAnimation(IdleHash);
}