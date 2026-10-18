using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using Helpers.Runtime.Physics;
using Project._05_Scripts.Runtime.System.ServiceLocator.Services;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private CancellationTokenSource m_cts;

    private Vector3 m_attackDirection;
    
    private bool m_isFinished;
    private bool m_hasHit;
    private bool m_isBouncing;

    private CameraService m_cameraService;
    private AudioService m_audioService;

    private readonly CountdownTimer m_recoverCancelTimer;
    
    private readonly Collider[] m_strikeHits = new Collider[8];

    public bool IsFinished => m_isFinished;
    public bool HasHit => m_hasHit;

    public PlayerAttackState(
        PlayerCharacter owner,
        Animator animator,
        SO_PlayerStats playerStats) : base(owner, animator, playerStats)
    {
        m_cameraService = ServiceLocator.Get<CameraService>();
        m_audioService = ServiceLocator.Get<AudioService>();

        m_recoverCancelTimer = new CountdownTimer(0f);
    }

    public void SetAttackDirection(Vector3 direction)
    {
        m_attackDirection = direction;
    }

    public override void OnEnter()
    {
        m_isFinished = false;
        m_hasHit = false;
        m_isBouncing = false;

        m_owner.ResetRecoverCancel();
        m_owner.SetCanCancelRecover(false);

        m_owner.ConsumeAttackRequest();

        m_animator.CrossFade(
            DashHash,
            m_crossFadeDuration);

        m_cts = new CancellationTokenSource();

        RunAttackAsync(m_cts.Token).Forget();
    }

    public override void OnExit()
    {
        base.OnExit();

        m_recoverCancelTimer.OnTimerStop -= EnableRecoverCancel;

        m_recoverCancelTimer.Stop();

        m_owner.SetCanCancelRecover(false);

        m_cts?.Cancel();
        m_cts?.Dispose();
        m_cts = null;
    }

    private async UniTaskVoid RunAttackAsync(CancellationToken token)
    {
        Transform target = ResolveTarget();

        if (!TryGetTargetActor(target, out Actor targetActor))
        {
            FinishWithoutHit();
            return;
        }

        m_audioService.PlaySfx(m_stats.AttackDashEvent);

        await DashToTargetAsync(target, token);
        
        await UniTask.WaitForFixedUpdate(token);

        if (token.IsCancellationRequested) return;

        if (TryStrike(targetActor))
        {
            m_hasHit = true;
            ApplyImpact(targetActor);
        }
        else
        {
            m_owner.Bounce.Break();
        }

        Finish();
    }

    private Transform ResolveTarget()
    {
        if (m_owner.Bounce.Target != null)
        {
            Transform bounceTarget = m_owner.Bounce.ClearBufferBouncy();
            AimAt(bounceTarget);
            m_isBouncing = true;
            return bounceTarget;
        }

        m_owner.Bounce.Break();

        return PhysicsHelper.FindElementInDirectionByLayer(
            m_owner.TransformCache.position,
            m_stats.AttackCastRadius,
            m_attackDirection,
            m_stats.AttackCastDistance,
            m_stats.EnemyLayer,
            m_stats.ObstacleLayer);
    }
    
    private void AimAt(Transform target)
    {
        Vector3 toTarget = target.position - m_owner.TransformCache.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > 0.0001f)
            m_attackDirection = toTarget.normalized;
    }

    private static bool TryGetTargetActor(Transform target, out Actor targetActor)
    {
        targetActor = target ? target.GetComponent<Actor>() : null;
        return targetActor != null;
    }

    private async UniTask DashToTargetAsync(Transform target, CancellationToken token)
    {
        float dashDistance = GetDashDistance(target);
        DashData attackDash = m_stats.AttackDash;

        float threshold = m_stats.BounceThresholdTimer;
        float frontThreshold = Mathf.Min(threshold, attackDash.Duration);

        if (dashDistance <= 0f)
        {
            m_owner.Bounce.StartBounceSequence(0f, threshold, target);
            return;
        }

        m_owner.Bounce.StartBounceSequence(
            attackDash.Duration - frontThreshold,
            frontThreshold + threshold,
            target);

        StartRecoverCancelTimer(GetRecoverCancelDelay(attackDash));

        await m_owner.Dash.DashAsync(
            m_attackDirection,
            token,
            attackDash.Duration,
            dashDistance,
            attackDash.Curve,
            m_stats.ObstacleLayer);
    }
    
    private bool TryStrike(Actor targetActor)
    {
        Vector3 center = m_owner.TransformCache.position + m_attackDirection * m_stats.AttackStopOffset;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            m_stats.AttackStrikeRadius,
            m_strikeHits,
            m_stats.EnemyLayer,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            if (m_strikeHits[i].GetComponentInParent<Actor>() == targetActor) return true;
        }

        return false;
    }


    private float GetDashDistance(Transform target)
    {
        float rawDistance = Vector3.Distance(m_owner.TransformCache.position, target.position);
        return rawDistance - m_stats.AttackStopOffset;
    }

    private float GetRecoverCancelDelay(DashData attackDash)
    {
        float threshold = Mathf.Min(m_stats.RecoverCancelThreshold, attackDash.Duration);
        return attackDash.Duration - threshold;
    }

    private void ApplyImpact(Actor targetActor)
    {
        float finalForce = 0;

        finalForce = m_isBouncing
            ? m_stats.PushForce + m_owner.Bounce.Combo * (m_stats.BounceMultiplier - 1) * m_stats.PushForce
            : m_stats.PushForce;

        EventBus<ActorPushedEvent>.Raise(
            new ActorPushedEvent(
                targetActor,
                m_attackDirection,
                finalForce));

        m_cameraService.Shake();
        ServiceLocator.Get<FxService>()?.HitStop();
        m_audioService.PlaySfx(m_stats.HitEvent);
    }

    private void FinishWithoutHit()
    {
        m_isFinished = true;
        m_owner.RequestDash();
    }

    private void Finish() => m_isFinished = true;


    private void StartRecoverCancelTimer(float duration)
    {
        m_recoverCancelTimer.Reset(duration);

        m_recoverCancelTimer.OnTimerStop += EnableRecoverCancel;

        m_recoverCancelTimer.Start();
    }

    private void EnableRecoverCancel() => m_owner.SetCanCancelRecover(true);
    
}