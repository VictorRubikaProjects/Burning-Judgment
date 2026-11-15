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
    private Vector3 m_direction;

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
        HasHit = false;
        m_owner.ConsumeDashRequest();
        StartDash(m_owner.MoveDir);
    }

    public override void OnExit()
    {
        CancelDash();
        m_owner.ResetDashDistance();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleDashStrike();
    }

    public void UpdateDashParameters(Vector3 newDirection, float newDistance)
    {
        HasHit = false;
        StartDash(newDirection);
    }

    private void StartDash(Vector3 direction)
    {
        CancelDash();

        IsFinished = false;
        m_direction = direction;
        m_ctsDash = new CancellationTokenSource();

        RunDashAsync(direction, m_ctsDash.Token).Forget();
    }

    private void CancelDash()
    {
        m_ctsDash?.Cancel();
        m_ctsDash?.Dispose();
        m_ctsDash = null;
    }

    private async UniTaskVoid RunDashAsync(Vector3 direction, CancellationToken token)
    {
        float distance = m_owner.DashDistance;
        float duration = ComputeDuration(distance);

        PlayAnimation(DashHash);
        m_audioService.PlaySfx(m_audioService.Bank.PlayerDashEvent, m_owner.TransformCache.position);
        m_fxService.PlayDashEffect(m_owner.TransformCache, 0.4f);

        try
        {
            await m_owner.Dash.RollAsync(direction, token, duration, distance, m_stats.DashCurve, m_stats.ObstacleLayer);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        if (token.IsCancellationRequested) return;

        IsFinished = true;
    }

    private float ComputeDuration(float distance)
    {
        if (m_stats.DashDistance <= 0f) return m_stats.DashDuration;
        return Mathf.Clamp01(distance / m_stats.DashDistance) * m_stats.DashDuration;
    }

    private PlayerControllerComponent.StrikeData CreateAttackData(float forceMultiplier)
    {
        return new PlayerControllerComponent.StrikeData(
            m_stats.DashAttackRadius,
            m_stats.DashAttackLayer,
            m_stats.DashAttackOffset,
            true,
            forceMultiplier);
    }

    private PlayerControllerComponent.StrikeData CreateStrikeData(float forceMultiplier)
    {
        return new PlayerControllerComponent.StrikeData(
            m_stats.DashStrikeRadius,
            m_stats.DashStrikeLayer,
            m_stats.DashStrikeOffset,
            false,
            forceMultiplier);
    }

    private void HandleDashStrike()
    {
        if (HasHit || !m_owner.Dash.IsDashing) return;

        float forceMultiplier = m_owner.Chain.GetForceMultiplier;

        StrikeResult resultAttack = m_owner.Controller.TryStrike(CreateAttackData(forceMultiplier), m_direction);
        
        StrikeResult resultStrike = resultAttack.Accepted
            ? StrikeResult.Ignored
            : m_owner.Controller.TryStrike(CreateStrikeData(forceMultiplier), m_direction);

        if (!resultAttack.Accepted && !resultStrike.Accepted) return;

        HasHit = true;
        
        PlayAttackAnimation();
        
        if (resultAttack.AllowsChain) m_owner.Chain.IncrementChain();

        m_fxService.PlayerAttackPlayerEffect(m_owner.TransformCache, 0.5f);
        m_cameraService.Shake();
        m_fxService.HitStop();
        m_audioService.PlaySfx(m_audioService.Bank.PlayerAttackEvent, m_owner.TransformCache.position);

        if (resultAttack is { Accepted: true, CancelsDash: true } || resultStrike is { Accepted: true, CancelsDash: true })
        {
            m_ctsDash?.Cancel();
            IsFinished = true;
        }
    }
}