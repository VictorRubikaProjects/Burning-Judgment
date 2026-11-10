using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public class PlayerChainComponent : ActorComponent
{
    private const DelayType ComboDelayType = DelayType.UnscaledDeltaTime;

    private readonly SO_PlayerStats m_stats;
    private CancellationTokenSource m_ctsCombo;

    public int ChainCount { get; private set; }
    public bool IsActive => m_ctsCombo != null;

    public float GetForceMultiplier => 1f + (ChainCount * m_stats.ChainBonusPercent);

    public PlayerChainComponent(Actor owner, SO_PlayerStats stats) : base(owner)
    {
        m_stats = stats;
    }

    public override void Dispose()
    {
        base.Dispose();
        CancelTimer();
    }

    public void IncrementChain()
    {
        ChainCount++;
        Debug.Log($"<color=green>COMBO {ChainCount} </color>");
        RestartComboTimer().Forget();
        EventBus<PlayerComboEvent>.Raise(new PlayerComboEvent(ChainCount));
    }

    public override void Reset()
    {
        base.Reset();
        CancelTimer();
        ChainCount = 0;
        Debug.Log($"<color=red>COMBO {ChainCount} </color>");
        EventBus<PlayerComboEvent>.Raise(new PlayerComboEvent(ChainCount));
    }

    private async UniTaskVoid RestartComboTimer()
    {
        CancelTimer();

        m_ctsCombo = new CancellationTokenSource();
        
        CancellationToken token = m_ctsCombo.Token;

        bool cancelled = await UniTask
            .Delay(TimeSpan.FromSeconds(m_stats.ChainDurationBeforeClear), ComboDelayType, cancellationToken: token)
            .SuppressCancellationThrow();

        if (!cancelled) Reset();
    }

    private void CancelTimer()
    {
        m_ctsCombo?.Cancel();
        m_ctsCombo?.Dispose();
        m_ctsCombo = null;
    }
}