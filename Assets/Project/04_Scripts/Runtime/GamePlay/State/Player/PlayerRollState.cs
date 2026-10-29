using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class PlayerRollState : PlayerBaseState
{
    private AudioService m_audioService;
    private CameraService m_cameraService;
    private FxService m_fxService;

    private CancellationTokenSource m_ctsRoll;

    public bool IsFinished { get; private set; }
    
    public bool HasHit { get; private set; }

    public PlayerRollState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats, playerStats.RollAnimation)
    {
        m_audioService = ServiceLocator.Get<AudioService>();
        m_cameraService = ServiceLocator.Get<CameraService>();
        m_fxService = ServiceLocator.Get<FxService>();
    }

    public override void OnEnter()
    {
        IsFinished = false;
        HasHit = false;

        Vector3 direction = m_owner.MoveDir;
        float distance = m_stats.MaxRollDistance;
        m_owner.ConsumeRollRequest();
        PlayAnimation(RollHash);
        m_ctsRoll = new CancellationTokenSource();
        RollAsync(direction, distance, m_ctsRoll.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsRoll?.Cancel();
        m_ctsRoll?.Dispose();
        m_ctsRoll = null;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleRollStrike();
    }

    private async UniTaskVoid RollAsync(Vector3 direction, float distance, CancellationToken token)
    {
        
        try
        {
            await m_owner.Dash.RollAsync(
                direction,
                token,
                m_stats.RollDuration,
                distance,
                m_stats.RollCurve,
                m_stats.ObstacleLayer);
        }
        catch (OperationCanceledException)
        {
        }

        IsFinished = true;
    }
    
    private void HandleRollStrike()
    {
        if (!m_owner.Dash.IsDashing || HasHit) return;
        
        HasHit = m_owner.Controller.TryStrike(m_stats.RollStrikeRadius);

        if (!HasHit) return;
        
        m_cameraService.Shake();
        m_fxService.HitStop();
        m_audioService.PlaySfx(m_stats.HitEvent);
        m_ctsRoll?.Cancel();
    }
}