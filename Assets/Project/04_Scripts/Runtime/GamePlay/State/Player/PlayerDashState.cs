using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private readonly AudioService m_audioService;
    private readonly CameraService m_cameraService;
    private readonly FxService m_fxService;

    private CancellationTokenSource m_ctsDash;

    public bool IsFinished { get; private set; }
    public bool HasHit { get; private set; }

    public PlayerDashState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats, playerStats.DashAnimation)
    {
        m_audioService = ServiceLocator.Get<AudioService>();
        m_cameraService = ServiceLocator.Get<CameraService>();
        m_fxService = ServiceLocator.Get<FxService>();
    }

    public override void OnEnter()
    {
        IsFinished = false;
        HasHit = false;

        PlayAnimation(DashHash);
        
        m_fxService.PlayDashEffect(m_owner.TransformCache,0.4f);
        
        m_owner.ConsumeDashRequest();

        m_ctsDash = new CancellationTokenSource();
        
        RunDashAsync(m_owner.MoveDir, m_ctsDash.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsDash?.Cancel();
        m_ctsDash?.Dispose();
        m_ctsDash = null;

        m_owner.ResetDashDistance();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleDashStrike();
    }

    private async UniTaskVoid RunDashAsync(Vector3 direction, CancellationToken token)
    {

        float durationDash = Mathf.Clamp01(m_owner.DashDistance / m_stats.DashDistance) * m_stats.DashDuration;

        m_audioService.PlaySfx(m_audioService.Bank.PlayerDashEvent,m_owner.TransformCache.position);
        
        try
        {
            await m_owner.Dash.RollAsync(direction, token, durationDash, m_owner.DashDistance, m_stats.DashCurve, m_stats.ObstacleLayer);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        IsFinished = true;
    }

    private void HandleDashStrike()
    {
        if (!m_owner.Dash.IsDashing || HasHit) return;

        StrikeResult result = m_owner.Controller.TryStrike(
            m_stats.DashStrikeRadius,
            m_stats.DashStrikeLayer,
            PlayerControllerComponent.StrikeType.Dash,
            m_owner.Chain.GetForceMultiplier);

        if (!result.Accepted) return;

        HasHit = true;
        
        m_fxService.PlayerAttackPlayerEffect(m_owner.TransformCache, 0.5f);

        if (result.AllowsChain) m_owner.Chain.IncrementChain();

        m_cameraService.Shake();
        m_fxService.HitStop();
        m_audioService.PlaySfx(m_audioService.Bank.PlayerAttackEvent,m_owner.TransformCache.position);

        if (result.CancelsDash)
        {
            m_ctsDash?.Cancel();
            IsFinished = true;
        }
    }
    
    public void UpdateDashParameters(Vector3 newDirection, float newDistance)
    {
        IsFinished = false;
        PlayAnimation(DashHash);

        m_ctsDash?.Cancel();
        m_ctsDash?.Dispose();
        m_ctsDash = new CancellationTokenSource();

        RunDashAsync(newDirection, m_ctsDash.Token).Forget();
    }
}