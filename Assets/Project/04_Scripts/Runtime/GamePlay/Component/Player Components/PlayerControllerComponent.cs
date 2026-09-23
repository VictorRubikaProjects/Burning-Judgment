using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerControllerComponent : ActorComponent
{
    private Rigidbody m_rb;
    
    private Quaternion m_targetRotation;

    private PlayerCharacter m_pc;
    private readonly SO_PlayerStats m_stats;
    
    public PlayerControllerComponent(Actor owner,Rigidbody rb, SO_PlayerStats playerStats) : base(owner)
    {
        m_rb = rb;
        m_pc = (PlayerCharacter)owner;
        m_stats = playerStats;
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
        AnimationCurve curve,
        LayerMask groundLayer,
        float groundCheckHeight,
        float groundCheckDistance)
    {
        Vector3 start = m_rb.position;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            float curveValue = curve.Evaluate(t);
            Vector3 targetPosition = start + direction * (dashDistance * curveValue);

            if (!IsGrounded(targetPosition, groundLayer, groundCheckHeight, groundCheckDistance))
                return false;

            m_rb.MovePosition(targetPosition);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            if (token.IsCancellationRequested) return false;

            elapsed += Time.fixedDeltaTime;
        }

        Vector3 finalPosition = start + direction * dashDistance;

        if (!IsGrounded(finalPosition, groundLayer, groundCheckHeight, groundCheckDistance))
            return false;

        m_rb.MovePosition(finalPosition);
        return true;
    }
    
    public async UniTask<bool> HitAsync(
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

            m_rb.MovePosition(targetPosition);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            if (token.IsCancellationRequested) return false;

            elapsed += Time.fixedDeltaTime;
        }

        Vector3 finalPosition = start + direction * dashDistance;

        m_rb.MovePosition(finalPosition);
        return true;
    }

    private bool IsGrounded(Vector3 position, LayerMask groundLayer, float height, float distance)
    {
        Vector3 origin = position + Vector3.up * height;
        return Physics.Raycast(origin, Vector3.down, distance, groundLayer);
    }
    
    public override void OnDrawGizmosSelected()
    {
        if (m_stats == null) return;

        Vector3 origin = m_rb.position + Vector3.up * m_stats.GroundCheckHeight;
        bool grounded = Physics.Raycast(origin, Vector3.down, m_stats.GroundCheckDistance, m_stats.GroundLayer);

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * m_stats.GroundCheckDistance);
        Gizmos.DrawWireSphere(origin, 0.1f);
    }
}
