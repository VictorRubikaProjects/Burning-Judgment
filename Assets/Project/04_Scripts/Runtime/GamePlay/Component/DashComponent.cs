using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DashComponent : ActorComponent
{
    private readonly Rigidbody m_rb;
    private readonly GroundCheckComponent m_groundCheck;

    public bool IsDashing { get; private set; }

    public DashComponent(Actor owner, Rigidbody rb, GroundCheckComponent groundCheck) : base(owner)
    {
        m_rb = rb;
        m_groundCheck = groundCheck;
    }

    public async UniTask<bool> DashAsync(
        Vector3 direction,
        CancellationToken token,
        float dashDuration,
        float dashDistance,
        AnimationCurve curve)
    {
        if (IsDashing) return false;

        IsDashing = true;

        try
        {
            Vector3 start = m_rb.position;
            float elapsed = 0f;

            while (elapsed < dashDuration)
            {
                float t = elapsed / dashDuration;
                Vector3 targetPosition = start + direction * (dashDistance * curve.Evaluate(t));

                if (!m_groundCheck.IsGrounded(targetPosition))
                    return false;

                m_rb.MovePosition(targetPosition);

                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

                elapsed += Time.fixedDeltaTime;
            }

            Vector3 finalPosition = start + direction * dashDistance;

            if (!m_groundCheck.IsGrounded(finalPosition))
                return false;

            m_rb.MovePosition(finalPosition);
            return true;
        }
        finally
        {
            IsDashing = false;
        }
    }
}