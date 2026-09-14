using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HitState : BaseState
{
    private readonly float m_hitDuration;
    private CancellationTokenSource m_ctsHit;

    public bool IsFinished { get; private set; }

    public HitState(PlayerCharacter owner, Animator animator,ConfigStatsPlayer configStats) : base(owner, animator,configStats)
    {
        m_hitDuration = configStats.hitDuration;
    }

    public override void OnEnter()
    {
        IsFinished = false;
        m_animator.CrossFade(HitHash, m_crossFadeDuration);

        m_owner.ConsumeHitRequest();

        m_ctsHit = new CancellationTokenSource();
        Recover(m_ctsHit.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsHit?.Cancel();
        m_ctsHit?.Dispose();
    }

    private async UniTask Recover(CancellationToken token)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(m_hitDuration), cancellationToken: token).SuppressCancellationThrow();
        if (token.IsCancellationRequested) return;

        IsFinished = true;
    }
}