using UnityEngine;

public class DeathState : BaseState
{
    public DeathState(PlayerCharacter owner, Animator animator, ConfigStatsPlayer configStats) : base(owner, animator,configStats) { }

    public override void OnEnter()
    {
        m_animator.CrossFade(DeathHash, m_crossFadeDuration);
    }
}