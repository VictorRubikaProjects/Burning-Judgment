using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerControllerComponent : ActorComponent
{
    private Rigidbody m_rb;
    
    private Quaternion m_targetRotation;

    private PlayerCharacter m_pc;
    private readonly SO_PlayerStats m_stats;
    private readonly GroundCheckComponent m_groundCheck;
    
    public PlayerControllerComponent(Actor owner,Rigidbody rb, SO_PlayerStats playerStats, GroundCheckComponent groundCheck) : base(owner)
    {
        m_rb = rb;
        m_pc = (PlayerCharacter)owner;
        m_stats = playerStats;
        m_groundCheck = groundCheck;
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
    
    public async UniTask<bool> DashAsync(
        Vector3 direction,
        CancellationToken token,
        float dashDuration,
        float dashDistance,
        AnimationCurve curve)
    {
        Vector3 start = m_rb.position;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            float curveValue = curve.Evaluate(t);
            Vector3 targetPosition = start + direction * (dashDistance * curveValue);

            if (!m_groundCheck.IsGrounded(targetPosition))
                return false;

            m_rb.MovePosition(targetPosition);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            if (token.IsCancellationRequested) return false;

            elapsed += Time.fixedDeltaTime;
        }

        Vector3 finalPosition = start + direction * dashDistance;

        if (!m_groundCheck.IsGrounded(finalPosition))
            return false;

        m_rb.MovePosition(finalPosition);
        return true;
    }
}