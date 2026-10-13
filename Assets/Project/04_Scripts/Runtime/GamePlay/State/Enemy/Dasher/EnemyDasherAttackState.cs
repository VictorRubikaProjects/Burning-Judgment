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

    private Vector3 m_targetPosition;
    
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
        m_targetPosition = m_dasher.Player.TransformCache.position;
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
        Vector3 toTarget = m_targetPosition - m_dasher.TransformCache.position;
        toTarget.y = 0f;

        float distance = m_dasher.Stats.DashDistance;
        
        Vector3 toTargetDirection = toTarget.normalized;
        
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(m_dasher.Stats.TimeTransitionLockToAttack), cancellationToken: token);
            
            m_isAttacking = true;
            
            await m_dasher.Dash.DashAsync(
                toTargetDirection,
                token,
                m_dasher.Stats.DashDuration,
                distance,
                m_dasher.Stats.CurveDash);
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
                m_dasher.Stats.Force));
            
            m_cameraService.Shake();
            
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