using UnityEngine;

public class PlayerControllerComponent : ActorComponent
{
    private readonly Rigidbody m_rb;
    private readonly PlayerCharacter m_pc;

    private Quaternion m_targetRotation;

    public PlayerControllerComponent(Actor owner, Rigidbody rb) : base(owner)
    {
        m_rb = rb;
        m_pc = (PlayerCharacter)owner;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleRotation();
    }

    public override void Update()
    {
        base.Update();
        CalculateRotation();
    }

    private void CalculateRotation()
    {
        if (m_pc.MoveDir == Vector3.zero) return;
        m_targetRotation = Quaternion.LookRotation(m_pc.MoveDir);
    }

    private void HandleRotation()
    {
        if (m_targetRotation == Quaternion.identity) return;
        m_targetRotation.Normalize();
        m_rb.MoveRotation(m_targetRotation);
    }
}