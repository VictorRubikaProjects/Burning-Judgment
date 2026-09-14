using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(PlayerCharacter owner, Animator animator,ConfigStatsPlayer configStats) : base(owner, animator,configStats) { }

    public override void OnEnter()
    {
        m_animator.CrossFade(IdleHash, m_crossFadeDuration);
    }
}