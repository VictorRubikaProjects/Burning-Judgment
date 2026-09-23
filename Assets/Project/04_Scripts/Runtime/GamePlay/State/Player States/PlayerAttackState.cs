using System.Threading;
using Cysharp.Threading.Tasks;
using Event_Bus;
using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private CancellationTokenSource m_cts;

    private Vector3 m_attackDirection;
    private bool m_isFinished;

    public bool IsFinished => m_isFinished;
    
    private CameraService m_cameraService;
    private AudioService m_audioService;

    public PlayerAttackState(PlayerCharacter owner, Animator animator, SO_PlayerStats playerStats) : base(owner, animator, playerStats)
    {
        m_cameraService = ServiceLocator.Get<CameraService>();
        m_audioService = ServiceLocator.Get<AudioService>();
    }

    public void SetAttackDirection(Vector3 direction) => m_attackDirection = direction;

    public override void OnEnter()
    {
        base.OnEnter();

        m_isFinished = false;
        
        m_owner.ConsumeAttackRequest();
        m_animator.CrossFade(DashHash, m_crossFadeDuration); // TODO remplacer par un AttackHash 

        m_cts = new CancellationTokenSource();
        RunAttackAsync(m_cts.Token).Forget();
    }

    public override void OnExit()
    {
        base.OnExit();
        m_cts?.Cancel();
        m_cts?.Dispose();
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

        m_audioService.PlaySfx(m_stats.AttackDashEvent);

        Vector3 dashOrigin = m_owner.TransformCache.position;
        float rawDistance = Vector3.Distance(dashOrigin, target.position);
        float dashDistance = rawDistance - m_stats.AttackStopOffset;

        if (dashDistance > 0f)
        {
            DashData attackDash = m_stats.AttackDash;

            bool dashSucceeded = await m_owner.Controller.DashAsync(
                m_attackDirection,
                token,
                attackDash.Duration,
                dashDistance,
                attackDash.Curve,
                m_stats.GroundLayer,
                m_stats.GroundCheckHeight,
                m_stats.GroundCheckDistance);

            if (token.IsCancellationRequested) return;

            if (!dashSucceeded)
            {
                m_isFinished = true;
                return;
            }
        }

        EventBus<ActorPushedEvent>.Raise(new ActorPushedEvent(targetActor,m_owner, 5f));
        
        m_cameraService.Shake();
        m_audioService.PlaySfx(m_stats.HitEvent);

        m_isFinished = true;
    }

    //TODO SphereCastAll n'est pas alloc et donc très couteux dans le futur réfléchir à une meilleur solution.
    private Transform FindEnemyInDirection()
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            m_owner.TransformCache.position,
            m_stats.AttackCastRadius,
            m_attackDirection,
            m_stats.AttackCastDistance,
            m_stats.AttackStateLayer()
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (((1 << hit.collider.gameObject.layer) & m_stats.ObstacleLayer.value) != 0)
                return null;

            if (((1 << hit.collider.gameObject.layer) & m_stats.EnemyLayer.value) != 0)
                return hit.collider.transform;
        }

        return null;
    }
}