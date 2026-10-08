using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private CancellationTokenSource m_cts;

    private Vector3 m_attackDirection;
    private bool m_isFinished;
    private bool m_hasHit;

    private CameraService m_cameraService;
    private AudioService m_audioService;

    private readonly CountdownTimer m_recoverCancelTimer;

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
        Transform target = FindEnemyInDirection();

        if (target == null)
        {
            m_isFinished = true;
            m_owner.RequestDash();
            return;
        }

        Actor targetActor = target.GetComponent<Actor>();

        if (targetActor == null)
        {
            m_isFinished = true;
            m_owner.RequestDash();
            return;
        }

        m_hasHit = true;

        m_audioService.PlaySfx(m_stats.AttackDashEvent);

        Vector3 dashOrigin = m_owner.TransformCache.position;

        float rawDistance = Vector3.Distance(
            dashOrigin,
            target.position);

        float dashDistance =
            rawDistance - m_stats.AttackStopOffset;

        if (dashDistance > 0f)
        {
            DashData attackDash = m_stats.AttackDash;

            float threshold = Mathf.Min(
                m_stats.RecoverCancelThreshold,
                attackDash.Duration);

            StartRecoverCancelTimer(
                attackDash.Duration - threshold);

            await m_owner.Dash.DashAsync(
                m_attackDirection,
                token,
                attackDash.Duration,
                dashDistance,
                attackDash.Curve);

            if (token.IsCancellationRequested)
                return;
        }

        EventBus<ActorPushedEvent>.Raise(
            new ActorPushedEvent(
                targetActor,
                m_attackDirection,
                m_stats.Force));

        m_cameraService.Shake();
        m_audioService.PlaySfx(m_stats.HitEvent);

        m_isFinished = true;
    }

    private void StartRecoverCancelTimer(float duration)
    {
        m_recoverCancelTimer.Reset(duration);

        m_recoverCancelTimer.OnTimerStop += EnableRecoverCancel;

        m_recoverCancelTimer.Start();
    }

    private void EnableRecoverCancel() => m_owner.SetCanCancelRecover(true);

    private Transform FindEnemyInDirection()
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            m_owner.TransformCache.position,
            m_stats.AttackCastRadius,
            m_attackDirection,
            m_stats.AttackCastDistance,
            m_stats.AttackStateLayer());

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (((1 << hit.collider.gameObject.layer) &
                 m_stats.ObstacleLayer.value) != 0)
            {
                return null;
            }

            if (((1 << hit.collider.gameObject.layer) &
                 m_stats.EnemyLayer.value) != 0)
            {
                return hit.collider.transform;
            }
        }

        return null;
    }
}