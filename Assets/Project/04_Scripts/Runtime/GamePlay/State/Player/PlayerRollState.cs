using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using Project._05_Scripts.Runtime.System.ServiceLocator.Services;
using UnityEngine;

public class PlayerRollState : PlayerBaseState
{
    private readonly Rigidbody m_rb;

    private CancellationTokenSource m_ctsDash;
    
    private AudioService m_audioService;

    public bool IsFinished { get; private set; }
    
    public bool HasHit { get; private set; }
    
    private readonly Collider[] m_strikeHits = new Collider[8];
    

    public PlayerRollState(PlayerCharacter owner, Animator animator, Rigidbody rb, SO_PlayerStats playerStats)
        : base(owner, animator, playerStats)
    {
        m_rb = rb;
        m_audioService = ServiceLocator.Get<AudioService>();
    }

    public override void OnEnter()
    {
        IsFinished = false;
        HasHit = false;

        m_animator.CrossFade(DashHash, m_crossFadeDuration);
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

        if (m_owner.Dash.IsDashing && !HasHit)
        {
            TryStrike();
        }
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

    private void TryStrike()
    {
        Vector3 center = m_owner.TransformCache.position;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            m_stats.AttackStrikeRadius,
            m_strikeHits,
            m_stats.EnemyLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (!m_strikeHits[i].TryGetComponent(out EnemyBase enemy)) continue;

            EventBus<ActorPushedEvent>.Raise(
                new ActorPushedEvent(
                    enemy,
                    m_owner.MoveDir,
                    m_stats.PushForce));

            ServiceLocator.Get<CameraService>()?.Shake();
            ServiceLocator.Get<FxService>()?.HitStop();
            m_audioService.PlaySfx(m_stats.HitEvent);

            HasHit = true;
            m_ctsDash?.Cancel();
            return;
        }
    }
}