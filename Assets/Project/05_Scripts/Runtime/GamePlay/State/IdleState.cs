using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(PlayerCharacter owner, Animator animator) : base(owner, animator) { }

    public override void OnEnter()
    {
        m_animator.CrossFade(IdleHash, m_crossFadeDuration);
    }
}