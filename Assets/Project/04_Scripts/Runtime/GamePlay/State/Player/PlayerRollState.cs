using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Project._05_Scripts.Runtime.System.ServiceLocator.Services;
using UnityEngine;

public class PlayerRollState : PlayerBaseState
{
    private readonly AudioService m_audioService;

    private CancellationTokenSource m_ctsRoll;

    public bool IsFinished { get; private set; }

    public PlayerRollState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats)
    {
        m_audioService = ServiceLocator.Get<AudioService>();
    }

    public override void OnEnter()
    {
        IsFinished = false;

        Vector3 direction = m_owner.MoveDir;
        float distance = m_owner.RollRatio * m_stats.MaxRollDistance;
        m_owner.ConsumeRollRequest();
        m_animator.CrossFade(DashHash, m_crossFadeDuration);
        m_ctsRoll = new CancellationTokenSource();
        RollAsync(direction, distance, m_ctsRoll.Token).Forget();
    }

    public override void OnExit()
    {
        m_ctsRoll?.Cancel();
        m_ctsRoll?.Dispose();
        m_ctsRoll = null;
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
}