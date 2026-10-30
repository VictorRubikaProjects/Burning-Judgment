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
    private float m_touchPressedTime;
    private bool m_touchUnresolved;
    private bool m_touchedThisWindow;

    public int ChainCount { get; private set; }
    public bool IsWindowOpen { get; private set; }
    public bool HasPendingChain { get; private set; }

    public float GetForceMultiplier => 1f + (ChainCount * m_stats.ChainBonusPercent);

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
        CancelTimer();
        CloseWindow();
        m_touchedThisWindow = false;
    }

    public void OpenWindow()
    {
        m_touchedThisWindow = false;

        bool pressedJustBefore = m_touchUnresolved
            && Time.unscaledTime - m_touchPressedTime <= m_stats.ChainThreshold;

        if (pressedJustBefore)
        {
            HasPendingChain = true;
            m_touchedThisWindow = true;
        }

        RunWindowAsync().Forget();
    }

    public override void Reset()
    {
        base.Reset();

        CancelTimer();
        CloseWindow();

        ChainCount = 0;
        HasPendingChain = false;
        m_touchedThisWindow = false;
    }

    private async UniTaskVoid RunWindowAsync()
    {
        CancelTimer();
        CloseWindow();

        m_ctsWindow = new CancellationTokenSource();
        CancellationToken token = m_ctsWindow.Token;

        IsWindowOpen = true;
        OnChainWindowOpen?.Invoke();

        bool cancelled = await UniTask
            .Delay(TimeSpan.FromSeconds(m_stats.ChainThreshold), WindowDelayType, cancellationToken: token)
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
        m_touchPressedTime = Time.unscaledTime;
        m_touchUnresolved = true;

        if (!IsWindowOpen) return;

        HasPendingChain = true;
        m_touchedThisWindow = true;
    }

    private void HandleHoldConfirmed()
    {
        m_touchUnresolved = false;

        if (!HasPendingChain) return;

        HasPendingChain = false;
        ChainCount++;

        OnChainInputSuccess?.Invoke();
    }

    private void HandleTouchReleased()
    {
        m_touchUnresolved = false;

        if (!HasPendingChain) return;

        Reset();
    }
}