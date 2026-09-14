using UnityEngine;

public class DeathState : BaseState
{
    public DeathState(PlayerCharacter owner, Animator animator) : base(owner, animator) { }

    public override void OnEnter()
    {
        m_animator.CrossFade(DeathHash, m_crossFadeDuration);
    }
}