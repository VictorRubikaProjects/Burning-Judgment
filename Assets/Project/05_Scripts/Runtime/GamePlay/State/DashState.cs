using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DashState : BaseState
{
    private readonly Rigidbody m_rb;
    private readonly float m_dashDistance;
    private readonly float m_dashDuration;
    private readonly AnimationCurve m_dashCurve;

    private CancellationTokenSource m_ctsDash;

    public bool IsFinished { get; private set; }

    public DashState(PlayerCharacter owner, Animator animator, Rigidbody rb, ConfigStatsPlayer configStats)
        : base(owner, animator, configStats)
    {
        m_rb = rb;
        m_dashDistance = configStats.dashDistance;
        m_dashDuration = configStats.dashDuration;
        m_dashCurve = configStats.dashCurve;
    }

    public override void OnEnter()
    {
        IsFinished = false;
        
        m_animator.CrossFade(DashHash, m_crossFadeDuration);

        m_owner.ConsumeDashRequest();

        m_ctsDash = new CancellationTokenSource();
        
        DashAsync(m_owner.MoveDir, m_ctsDash.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsDash?.Cancel();
        m_ctsDash?.Dispose();
    }

    private async UniTaskVoid DashAsync(Vector3 direction, CancellationToken token)
    {
        Vector3 start = m_rb.position;
        float elapsed = 0f;

        while (elapsed < m_dashDuration)
        {
            float t = elapsed / m_dashDuration;
            float curveValue = m_dashCurve.Evaluate(t);
            Vector3 targetPosition = start + direction * (m_dashDistance * curveValue);

            m_rb.MovePosition(targetPosition);

            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);
            
            if (token.IsCancellationRequested) return;

            elapsed += Time.fixedDeltaTime;
        }

        Vector3 finalPosition = start + direction * m_dashDistance;
        
        m_rb.MovePosition(finalPosition);

        IsFinished = true;
    }
}