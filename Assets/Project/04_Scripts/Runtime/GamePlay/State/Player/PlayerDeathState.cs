using UnityEngine;

public class PlayerDeathState : PlayerBaseState
{
    public PlayerDeathState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats, playerStats.DeathAnimation) { }

    public override void OnEnter() => PlayAnimation(DeathHash);
}