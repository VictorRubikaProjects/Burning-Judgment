using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public class PlayerChainComponent : ActorComponent
{
    private readonly SO_PlayerStats m_stats;
    
    private CancellationTokenSource m_ctsCombo;

    private float m_forceMultiplier = 1f;

    private bool m_isLosingForce;
    private bool m_canIncreaseChain;

    public float ForceMultiplier
    {
        get => m_forceMultiplier;
        private set
        {
            m_forceMultiplier = Mathf.Max(value, 1f);
            EventBus<PlayerComboEvent>.Raise(new PlayerComboEvent(m_forceMultiplier));
        }
    }

    public PlayerChainComponent(Actor owner, SO_PlayerStats stats) : base(owner)
    {
        m_stats = stats;
    }

    public override void Dispose()
    {
        base.Dispose();
        CancelTimer();
    }

    public override void Update()
    {
        base.Update();

        if (m_isLosingForce)
        {
            UpdateChainForce(-m_stats.ChainDecrementation * Time.deltaTime);
        }
    }

    public async UniTask StartChainForce()
    {
        CancelTimer();
        
        m_canIncreaseChain = false;
        m_isLosingForce = false;
        
        m_ctsCombo = new CancellationTokenSource();
        CancellationToken token = m_ctsCombo.Token;
        
        bool cancelled = await UniTask
            .Delay(TimeSpan.FromSeconds(m_stats.ChainDelayBeforeIncrementation), cancellationToken: token)
            .SuppressCancellationThrow();
        
        if (!cancelled) m_canIncreaseChain = true;
    } 

    public void IncrementChain()
    {
        if (!m_canIncreaseChain) return;
        
        UpdateChainForce(m_stats.ChainIncrementation * Time.deltaTime);
        m_isLosingForce = false;
        RestartComboTimer();
    }

    public void RestartComboTimer() => RestartComboTimerAsync().Forget();
    
    private async UniTaskVoid RestartComboTimerAsync()
    {
        CancelTimer();

        m_ctsCombo = new CancellationTokenSource();
        CancellationToken token = m_ctsCombo.Token;

        bool cancelled = await UniTask
            .Delay(TimeSpan.FromSeconds(m_stats.ChainDurationBeforeClear), cancellationToken: token)
            .SuppressCancellationThrow();

        if (!cancelled) m_isLosingForce = true;
    }

    private void CancelTimer()
    {
        m_ctsCombo?.Cancel();
        m_ctsCombo?.Dispose();
        m_ctsCombo = null;
    }
    
    private void UpdateChainForce(float force) => ForceMultiplier += force;
    
    public void OnEnemyHit(float comboDecrementation) => UpdateChainForce(-comboDecrementation);
}