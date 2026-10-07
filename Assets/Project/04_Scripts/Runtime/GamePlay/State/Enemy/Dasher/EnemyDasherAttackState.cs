using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public class EnemyDashAttackState : EnemyBaseState
{
    public bool IsFinished { get; private set; }

    private readonly EnemyDasher m_dasher;
    private readonly CountdownTimer m_timerChargeAttack;
    private CancellationTokenSource m_cts;

    private Vector3 m_targetPosition;
    private readonly Collider[] m_strikeHits = new Collider[4];
    
    private CameraService m_cameraService;

    public EnemyDashAttackState(EnemyDasher owner) : base(owner)
    {
        m_dasher = owner;
        m_timerChargeAttack = new CountdownTimer(owner.Stats.LockTime);
        m_cameraService = ServiceLocator.Get<CameraService>();
    }

    public override void OnEnter()
    {
        base.OnEnter();
        IsFinished = false;
        m_cts = new CancellationTokenSource();
        m_dasher.ConsumeDashRequest();
        m_timerChargeAttack.OnTimerStop += Attack;
        m_timerChargeAttack.Start();
    }

    public override void OnExit()
    {
        m_timerChargeAttack.OnTimerStop -= Attack;
        m_timerChargeAttack.Stop();
        m_cts?.Cancel();
        m_cts?.Dispose();
        m_cts = null;
    }

    public override void Update()
    {
        base.Update();

        if (!m_timerChargeAttack.IsRunning) return;

        m_dasher.LookAtPlayer();
        m_targetPosition = m_dasher.Player.TransformCache.position;
    }

    private void Attack() => AttackAsync(m_cts.Token).Forget();

    private async UniTask AttackAsync(CancellationToken token)
    {
        Vector3 toTarget = m_targetPosition - m_dasher.TransformCache.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        Vector3 direction = distance > 0.001f ? toTarget / distance : m_dasher.TransformCache.forward;

        try
        {
            await m_dasher.Dash.DashAsync(
                direction,
                token,
                m_dasher.Stats.DashDuration,
                distance,
                m_dasher.Stats.CurveDash);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        ExecuteStrike();
        IsFinished = true;
    }

    private void ExecuteStrike()
    {
        Transform dasherTransform = m_dasher.TransformCache;
        Vector3 center = dasherTransform.position + dasherTransform.forward * m_dasher.Stats.StrikeForwardOffset;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            m_dasher.Stats.StrikeRadius,
            m_strikeHits,
            m_dasher.Stats.PlayerLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            PlayerCharacter player = m_strikeHits[i].GetComponentInParent<PlayerCharacter>();
            if (!player) continue;

            EventBus<ActorPushedEvent>.Raise(new ActorPushedEvent(
                player,
                Owner.TransformCache.forward,
                m_dasher.Stats.Force));
            
            m_cameraService.Shake();
            
            return;
        }
    }
}