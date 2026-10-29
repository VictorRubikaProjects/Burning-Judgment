using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerChainComponent : ActorComponent
{
    public int ChainCount { get; private set; } = 0;
    public bool IsWindowChainOpen { get; private set; } = false;
    public bool HasPendingChain { get; private set; } = false;
    
    private bool m_hasChainSucceeded = false;
    
    private readonly SO_PlayerStats m_stats;
    private CancellationTokenSource m_ctsChain;
    private readonly PlayerCharacter m_playerCharacter;

    public event Action OnChainInputSuccess;
    public event Action OnChainWindowOpen;
    public event Action OnChainWindowClosed;
    
    public PlayerChainComponent(Actor owner, SO_PlayerStats stats) : base(owner)
    {
        m_stats = stats;
        m_playerCharacter = owner as PlayerCharacter;

        if (!m_playerCharacter)
        {
            Debug.LogError("<color=red>[PlayerChainComponent] Owner is not a PlayerCharacter</color>");
            return;
        }
        
        m_playerCharacter.Input.OnTouchPressed += ResolveTouchInput;
        m_playerCharacter.Input.OnHoldConfirmed += ResolveHoldConfirmedInput;
        m_playerCharacter.Input.OnSwipe += ResolveSwipeInput;
    }

    public override void Dispose()
    {
        base.Dispose();
        
        m_ctsChain?.Cancel();
        m_ctsChain?.Dispose();
        m_ctsChain = null;

        if (m_playerCharacter != null && m_playerCharacter.Input != null)
        {
            m_playerCharacter.Input.OnTouchPressed -= ResolveTouchInput;
            m_playerCharacter.Input.OnHoldConfirmed -= ResolveHoldConfirmedInput;
            m_playerCharacter.Input.OnSwipe -= ResolveSwipeInput;
        }
        
        Debug.Log("<color=grey>[PlayerChainComponent] Disposed and unsubscribed from inputs.</color>");
    }

    public async UniTaskVoid SetTimerChain() // Appel depuis le Dash State OnEnter
    {
        m_ctsChain?.Cancel();
        m_ctsChain?.Dispose();
        m_ctsChain = new CancellationTokenSource();
        
        m_hasChainSucceeded = false;
        
        try
        {
            float initialDelay = Mathf.Max(0f, m_stats.DashDuration - m_stats.ChainThreshold);
            
            Debug.Log($"<color=cyan>[PlayerChainComponent] Dash started. Waiting {initialDelay:F2}s before opening chain window...</color>");

            await UniTask.Delay(TimeSpan.FromSeconds(initialDelay),
                DelayType.UnscaledDeltaTime,
                cancellationToken: m_ctsChain.Token);
        
            IsWindowChainOpen = true;
            Debug.Log("<color=green>[PlayerChainComponent] >>> WINDOW CHAIN OPENED <<<</color>");
        
            OnChainWindowOpen?.Invoke();
            
            await UniTask.Delay(TimeSpan.FromSeconds(m_stats.ChainThreshold),
                DelayType.UnscaledDeltaTime,
                cancellationToken: m_ctsChain.Token);
        
            OnChainWindowClosed?.Invoke();
            
            IsWindowChainOpen = false;
            Debug.Log("<color=orange>[PlayerChainComponent] >>> WINDOW CHAIN CLOSED <<<</color>");

            if (!HasPendingChain && !m_hasChainSucceeded) 
            {
                Debug.Log("<color=yellow>[PlayerChainComponent] No pending chain or success found at window close. Resetting.</color>");
                Reset();
            }
        }
        catch (OperationCanceledException)
        {
            IsWindowChainOpen = false;
            
            Debug.Log("<color=magenta>[PlayerChainComponent] SetTimerChain task was cancelled.</color>");
        }
    }

    private void ResolveTouchInput()
    {
        if (!IsWindowChainOpen) return; 
        
        HasPendingChain = true;
        Debug.Log("<color=green>[PlayerChainComponent] Touch Input detected! HasPendingChain = true</color>");
    }

    private void ResolveHoldConfirmedInput()
    {
        if (!HasPendingChain) return;
        
        ChainCount += 1;
        HasPendingChain = false;
        m_hasChainSucceeded = true; 
        
        OnChainInputSuccess?.Invoke();
        
        Debug.Log($"<color=lime>[PlayerChainComponent] Hold Confirmed! ChainCount = {ChainCount} (Pending cleared, Success marked)</color>");
    }

    private void ResolveSwipeInput(Vector2 direction, float ratio)
    {
        if (!HasPendingChain) return;
        
        Debug.Log("<color=red>[PlayerChainComponent] Swipe detected while having a pending chain -> Resetting!</color>");
        
        Reset();
    }

    public override void Reset()
    {
        base.Reset();
        
        ChainCount = 0;
        IsWindowChainOpen = false;
        HasPendingChain = false;
        m_hasChainSucceeded = false;
        
        m_ctsChain?.Cancel();
        m_ctsChain?.Dispose();
        m_ctsChain = null;
        
        Debug.Log("<color=grey>[PlayerChainComponent] Reset() executed. All states cleared.</color>");
    }
}