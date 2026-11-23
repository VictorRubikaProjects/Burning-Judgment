using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
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
    private FxService m_fxService;
    
    private static readonly int m_idPropMaterialProgressionHash = Shader.PropertyToID("_Progression");

    public EnemyDashAttackState(EnemyDasher owner) : base(owner)
    {
        m_dasher = owner;
        m_timerChargeAttack = new CountdownTimer(owner.Stats.LockTime);
        m_cameraService = ServiceLocator.Get<CameraService>();
        m_fxService = ServiceLocator.Get<FxService>();
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
        
        if (Owner.Animator != null)
        {
            Owner.Animator.Play(ChargeAttackHash);
            Owner.Animator.speed = m_dasher.Stats.LockTime;
            Owner.Animator.CrossFade(ChargeAttackHash, 0.1f);
        }
    }

    public override void OnExit()
    {
        m_isAttacking = false;
        m_timerChargeAttack.OnTimerStop -= Attack;
        m_timerChargeAttack.Stop();

        if (Owner.Animator != null) Owner.Animator.speed = 1;

        m_dasher.DashIndicator.Hide();
        
        m_cts?.Cancel();
        m_cts?.Dispose();
        m_cts = null;
    }

    public override void Update()
    {
        base.Update();

        if (!m_timerChargeAttack.IsRunning) return;

        m_dasher.LookAtPlayer();
        UpdateIndicator();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!m_isAttacking) return;
        ExecuteStrike();
    }

    private void UpdateIndicator()
    {
        Vector3 start = m_dasher.FeetTransform.position;
        Vector3 direction = m_dasher.TransformCache.forward;
        direction.y = 0f;
        direction.Normalize();

        float chargeProgress = 1f - (m_timerChargeAttack.CurrentTime / m_dasher.Stats.LockTime);
        float currentDashDistance = Mathf.Lerp(0f, m_dasher.Stats.DashDistance, chargeProgress);

        Vector3 end = start + direction * currentDashDistance;
        
        m_dasher.DashIndicator.Show(start, end);
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

            m_dasher.DashIndicator.Hide();
            
            if (Owner.Animator)
            {
                Owner.Animator.Play(AttackHash);
                Owner.Animator.CrossFade(AttackHash, 0.1f);
            }

            m_isAttacking = true;
            
            m_fxService.PlayDasherAttack(Owner.TransformCache,0.35f);

            var dashTask = m_dasher.Dash.DashAsync(
                direction,
                token,
                m_dasher.Stats.DashDuration,
                m_dasher.Stats.DashDistance,
                m_dasher.Stats.DashCurve,
                m_dasher.Stats.ObstacleLayer);

            await dashTask;

            m_isAttacking = false;
        }
        catch (OperationCanceledException)
        {
            m_dasher.DashIndicator.Hide();
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
                m_dasher.Stats.PushForce,
                m_dasher.Stats.Destabilization,
                m_dasher.TransformCache.position,
                m_dasher.Stats.ComboPlayerDecrease));

            m_cameraService.Shake();
            m_isAttacking = false;
            
            return;
        }
    }
}