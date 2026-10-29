using UnityEngine;

public class PlayerFallState : PlayerBaseState
{
    private Rigidbody m_rb;
    
    public PlayerFallState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats, Rigidbody rigidbody) : base(owner, animator, playerStats)
    {
        m_rb = rigidbody;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        
        m_rb.isKinematic = false;
        m_rb.useGravity = true;
    }
}