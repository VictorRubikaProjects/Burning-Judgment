using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyShooterHitState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    
    private EnemyShooter m_owner;
    private Rigidbody m_rb;
    private CancellationTokenSource m_cts;
    
    public EnemyShooterHitState(EnemyShooter owner, Rigidbody rb)
    {
        m_owner = owner;
        m_rb = rb;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log($"[{m_owner.name}] Enter Hit, dir={m_owner.PendingKnockbackDirection}, force={m_owner.PendingKnockbackForce}, rb.isKinematic={m_rb.isKinematic}");

        IsFinished = false;
        m_owner.Agent.enabled = false;
        m_cts = new CancellationTokenSource();
        m_owner.ConsumeHitRequest();

        HitAsync(
            m_owner.PendingKnockbackDirection,
            m_cts.Token,
            0.2f,
            m_owner.PendingKnockbackForce,
            m_owner.Stats.HitCurve).Forget();
    }

    public override void OnExit()
    {
        base.OnExit();
        m_cts?.Cancel();
        m_cts?.Dispose();
        
        m_owner.Agent.Warp(m_rb.position);
        m_owner.Agent.enabled = true;
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
        IsFinished = true;
        return true;
    }
}