using UnityEngine;

public class PLayerTransitionBounceState : PlayerBaseState
{
    public bool IsFinished { get; private set; } = false;
    
    public PLayerTransitionBounceState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats) : base(owner, animator, playerStats)
    {
    }

    public override void OnEnter()
    {
        base.OnEnter();
        m_owner.Bounce.SuccessBounce();
        IsFinished = true;
    }

    public override void OnExit()
    {
        base.OnExit();
        IsFinished = false;
    }
}