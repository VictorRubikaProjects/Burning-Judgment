using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerChainComponent : ActorComponent
{
    private const DelayType WindowDelayType = DelayType.UnscaledDeltaTime;

    private readonly SO_PlayerStats m_stats;
    private readonly PlayerInputComponent m_input;

    private CancellationTokenSource m_ctsWindow;
    private bool m_touchedThisWindow;

    public int ChainCount { get; private set; }
    public bool IsWindowOpen { get; private set; }
    public bool HasPendingChain { get; private set; }
    public float ForceMultiplier => 1f + m_stats.ChainBonusPercent / 100f * ChainCount;

    public event Action OnChainInputSuccess;
    public event Action OnChainWindowOpen;
    public event Action OnChainWindowClosed;

    public PlayerChainComponent(Actor owner, SO_PlayerStats stats, PlayerInputComponent input) : base(owner)
    {
        m_stats = stats;
        m_input = input;

        m_input.OnTouchPressed += HandleTouchPressed;
        m_input.OnHoldConfirmed += HandleHoldConfirmed;
        m_input.OnTouchReleased += HandleTouchReleased;
    }

    public override void Dispose()
    {
        base.Dispose();

        CancelTimer();

        m_input.OnTouchPressed -= HandleTouchPressed;
        m_input.OnHoldConfirmed -= HandleHoldConfirmed;
        m_input.OnTouchReleased -= HandleTouchReleased;
    }

    public void OnDashStarted()
    {
        m_touchedThisWindow = false;

        float openDelay = Mathf.Max(0f, m_stats.DashDuration - m_stats.ChainThreshold);
        ScheduleWindow(openDelay, m_stats.ChainThreshold * 2f).Forget();
    }

    public void OnDashStrike() => ScheduleWindow(0f, m_stats.ChainThreshold * 2f).Forget();

    public override void Reset()
    {
        base.Reset();

        CancelTimer();
        CloseWindow();

        ChainCount = 0;
        HasPendingChain = false;
        m_touchedThisWindow = false;
    }

    private async UniTaskVoid ScheduleWindow(float openDelay, float duration)
    {
        CancelTimer();
        CloseWindow();

        m_ctsWindow = new CancellationTokenSource();
        CancellationToken token = m_ctsWindow.Token;

        bool cancelled = await UniTask
            .Delay(TimeSpan.FromSeconds(openDelay), WindowDelayType, cancellationToken: token)
            .SuppressCancellationThrow();

        if (cancelled) return;

        IsWindowOpen = true;
        OnChainWindowOpen?.Invoke();

        cancelled = await UniTask
            .Delay(TimeSpan.FromSeconds(duration), WindowDelayType, cancellationToken: token)
            .SuppressCancellationThrow();

        if (cancelled) return;

        EndWindow();
    }

    private void EndWindow()
    {
        CloseWindow();

        if (!m_touchedThisWindow && !HasPendingChain) ChainCount = 0;

        m_touchedThisWindow = false;
    }

    private void CloseWindow()
    {
        if (!IsWindowOpen) return;

        IsWindowOpen = false;
        OnChainWindowClosed?.Invoke();
    }

    private void CancelTimer()
    {
        m_ctsWindow?.Cancel();
        m_ctsWindow?.Dispose();
        m_ctsWindow = null;
    }

    private void HandleTouchPressed()
    {
        if (!IsWindowOpen) return;

        HasPendingChain = true;
        m_touchedThisWindow = true;
    }

    private void HandleHoldConfirmed()
    {
        if (!HasPendingChain) return;

        HasPendingChain = false;
        ChainCount++;

        OnChainInputSuccess?.Invoke();
    }

    private void HandleTouchReleased()
    {
        if (!HasPendingChain) return;

        Reset();
    }
}