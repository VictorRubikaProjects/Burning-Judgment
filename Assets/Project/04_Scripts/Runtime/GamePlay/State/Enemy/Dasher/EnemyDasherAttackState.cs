using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using PrimeTween;
using UnityEngine;

public class EnemyDashAttackState : EnemyBaseState
{
    public bool IsFinished { get; private set; }
    
    private bool m_isAttacking;

    private readonly EnemyDasher m_dasher;
    private readonly CountdownTimer m_timerChargeAttack;
    
    private CancellationTokenSource m_cts;
    
    private readonly Collider[] m_strikeHits = new Collider[4];
    
    private CameraService m_cameraService;
    
    private static readonly int m_idPropMaterialProgressionHash = Shader.PropertyToID("_Progression");

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
        
        m_isAttacking = false;
        
        m_cts = new CancellationTokenSource();
        
        m_dasher.ConsumeDashRequest();
        
        m_timerChargeAttack.OnTimerStop += Attack;
        
        m_timerChargeAttack.Start();

        FeedBackAttack(m_cts.Token, 1f, m_dasher.Stats.LockTime);
    }

    public override void OnExit()
    {
        m_isAttacking = false;
        
        m_timerChargeAttack.OnTimerStop -= Attack;
        
        m_timerChargeAttack.Stop();
        
        m_cts?.Cancel();
        
        m_cts?.Dispose();
        
        m_cts = null;
        
        m_dasher.MaterialIndicatorDash.SetFloat(m_idPropMaterialProgressionHash, 0f);
    }

    public override void Update()
    {
        base.Update();

        if (!m_timerChargeAttack.IsRunning) return;

        m_dasher.LookAtPlayer();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!m_isAttacking) return;
        ExecuteStrike();
    }

    private void Attack() => AttackAsync(m_cts.Token).Forget();

    private async UniTask AttackAsync(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(m_dasher.Stats.LockToDashDelay), cancellationToken: token);

            Vector3 direction = m_dasher.TransformCache.forward;
            direction.y = 0f;
            direction.Normalize();

            m_isAttacking = true;

            await m_dasher.Dash.DashAsync(
                direction,
                token,
                m_dasher.Stats.DashDuration,
                m_dasher.Stats.DashDistance,
                m_dasher.Stats.DashCurve);

            m_isAttacking = false;
        }
        catch (OperationCanceledException)
        {
            return;
        }

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
                m_dasher.Stats.PushForce));

            m_cameraService.Shake();
            m_isAttacking = false;
            
            return;
        }
    }
    
    private void FeedBackAttack(CancellationToken token, float endValue, float duration)
    {
        Tween tween = Tween.MaterialProperty(
            m_dasher.MaterialIndicatorDash,
            m_idPropMaterialProgressionHash,
            endValue,
            duration);

        token.Register(tween.Stop);
    }
}