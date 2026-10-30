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
    private float m_forceMultiplier;
    private bool m_chainStruck;

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
        m_chainStruck = false;
        m_forceMultiplier = m_owner.Chain.ForceMultiplier;

        PlayAnimation(DashHash);
        m_owner.ConsumeDashRequest();
        m_owner.Chain.OnDashStarted();

        m_ctsDash = new CancellationTokenSource();
        RunDashAsync(m_owner.MoveDir, m_ctsDash.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsDash?.Cancel();
        m_ctsDash?.Dispose();
        m_ctsDash = null;

        m_owner.ResetDashDistance();

        if (!m_chainStruck) m_owner.Chain.Reset();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleDashStrike();
    }

    private async UniTaskVoid RunDashAsync(Vector3 direction, CancellationToken token)
    {
        m_audioService.PlaySfx(m_stats.DashEvent);

        try
        {
            await m_owner.Dash.RollAsync(
                direction,
                token,
                m_stats.DashDuration,
                m_owner.DashDistance,
                m_stats.DashCurve,
                m_stats.ObstacleLayer);
        }
        catch (OperationCanceledException)
        {
        }

        IsFinished = true;
    }

    private void HandleDashStrike()
    {
        if (!m_owner.Dash.IsDashing || HasHit) return;

        StrikeResult result = m_owner.Controller.TryStrike(
            m_stats.DashStrikeRadius,
            m_stats.DashStrikeLayer,
            m_forceMultiplier);

        if (!result.Accepted) return;

        HasHit = true;

        if (result.AllowsChain)
        {
            m_chainStruck = true;
            m_owner.Chain.OpenWindow();
        }

        m_cameraService.Shake();
        m_fxService.HitStop();
        m_audioService.PlaySfx(m_stats.HitEvent);

        if (result.CancelsDash) m_ctsDash?.Cancel();
    }
}