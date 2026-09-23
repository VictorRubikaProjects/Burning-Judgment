using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class KnockbackComponent : ActorComponent
{
    private readonly Rigidbody m_rb;
    private CancellationTokenSource m_cts;

    public bool IsActive { get; private set; }

    public KnockbackComponent(Actor owner, Rigidbody rb) : base(owner)
    {
        m_rb = rb;
    }

    public override void Dispose()
    {
        base.Dispose();
        Cancel();
    }

    public void Cancel()
    {
        if (m_cts == null) return;

        m_cts.Cancel();
        m_cts.Dispose();
        m_cts = null;
        IsActive = false;
    }

    public async UniTask<bool> HitAsync(
        Vector3 direction,
        float dashDuration,
        float dashDistance,
        AnimationCurve curve)
    {
        Cancel();

        m_cts = new CancellationTokenSource();
        var token = m_cts.Token;

        IsActive = true;
        Vector3 start = m_rb.position;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            float curveValue = curve.Evaluate(t);
            Vector3 targetPosition = start + direction * (dashDistance * curveValue);

            m_rb.MovePosition(targetPosition);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            if (token.IsCancellationRequested)
            {
                IsActive = false;
                return false;
            }

            elapsed += Time.fixedDeltaTime;
        }

        m_rb.MovePosition(start + direction * dashDistance);
        IsActive = false;
        return true;
    }
}