using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DashState : BaseState
{
    private readonly Rigidbody m_rb;
    private readonly float m_dashDistance;
    private readonly float m_dashDuration;

    private CancellationTokenSource m_ctsDash;

    public bool IsFinished { get; private set; }

    public DashState(PlayerCharacter owner, Animator animator, Rigidbody rb, ConfigStatsPlayer configStats)
        : base(owner, animator,configStats)
    {
        m_rb = rb;
        m_dashDistance = configStats.dashDistance;
        m_dashDuration = configStats.dashDuration;
    }

    public override void OnEnter()
    {
        IsFinished = false;
        m_animator.CrossFade(DashHash, m_crossFadeDuration);

        m_owner.ConsumeDashRequest();

        m_ctsDash = new CancellationTokenSource();
        Dash(m_owner.MoveDir, m_ctsDash.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsDash?.Cancel();
        m_ctsDash?.Dispose();
    }

    private async UniTask Dash(Vector3 direction, CancellationToken token)
    {
        Vector3 start = m_rb.position;
        Vector3 target = start + direction * m_dashDistance;
        float elapsed = 0f;

        while (elapsed < m_dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = elapsed / m_dashDuration;
            m_rb.MovePosition(Vector3.Lerp(start, target, t));
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token).SuppressCancellationThrow();
            if (token.IsCancellationRequested) return;
        }

        m_rb.MovePosition(target);
        IsFinished = true;
    }
}