using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project._05_Scripts.Runtime.System.ServiceLocator.Services;
using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private readonly Rigidbody m_rb;

    private CancellationTokenSource m_ctsDash;
    
    private AudioService m_audioService;
    private CameraService m_cameraService;
    private FxService m_fxService;

    public bool IsFinished { get; private set; }
    
    public bool HasHit { get; private set; }
    
    private readonly Collider[] m_strikeHits = new Collider[8];
    

    public PlayerDashState(PlayerCharacter owner, Animator animator, Rigidbody rb, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats, playerStats.DashAnimation)
    {
        m_rb = rb;
        m_audioService = ServiceLocator.Get<AudioService>();
        m_cameraService = ServiceLocator.Get<CameraService>();
        m_fxService = ServiceLocator.Get<FxService>();
    }

    public override void OnEnter()
    {
        IsFinished = false;
        HasHit = false;

        PlayAnimation(DashHash);
        m_owner.ConsumeDashRequest();

        m_ctsDash = new CancellationTokenSource();
        RollAsync(m_owner.MoveDir, m_ctsDash.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsDash?.Cancel();
        m_ctsDash?.Dispose();
        m_owner.ResetDashDistance();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleDashStrike();
    }

    private async UniTaskVoid RollAsync(Vector3 direction, CancellationToken token)
    {
        m_audioService.PlaySfx(m_stats.DashEvent);

        try
        {
            bool dashSucceeded = await m_owner.Dash.RollAsync(
                direction,
                token,
                m_stats.DashDuration,
                m_owner.DashDistance,
                m_stats.DashCurve,
                m_stats.ObstacleLayer);
        
            IsFinished = true;
        }
        catch (OperationCanceledException)
        {
            IsFinished = true;
        }
    }

    private void HandleDashStrike()
    {
        if (!m_owner.Dash.IsDashing || HasHit) return;
        
        HasHit = m_owner.Controller.TryStrike(m_stats.DashStrikeRadius);

        if (!HasHit) return;
        
        m_cameraService.Shake();
        m_fxService.HitStop();
        m_audioService.PlaySfx(m_stats.HitEvent);
        m_ctsDash?.Cancel();
    }
    
}