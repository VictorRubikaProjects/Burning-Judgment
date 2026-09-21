using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AttackState : BaseState
{
    private CancellationTokenSource m_cts;

    private Vector3 m_attackDirection;
    private bool m_isFinished;

    public bool IsFinished => m_isFinished;

    public AttackState(PlayerCharacter owner, Animator animator, ConfigStatsPlayer configStats) : base(owner, animator, configStats)
    {
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
        
        IDamageable targetDamageable = target.GetComponent<IDamageable>();

        if (targetDamageable == null || !targetDamageable.CanTakeDamage())
        {
            m_isFinished = true;
            m_owner.RequestDash();
            return;
        }
        
        Vector3 dashOrigin = m_owner.TransformCache.position;
        float rawDistance = Vector3.Distance(dashOrigin, target.position);
        float dashDistance = rawDistance - m_configStats.AttackStopOffset;

        if (dashDistance > 0f)
        {
            DashData attackDash = m_configStats.AttackDash;

            await m_owner.Controller.DashAsync(
                m_attackDirection,
                token,
                attackDash.Duration,
                dashDistance,
                attackDash.Curve
            );

            if (token.IsCancellationRequested) return;
        }

        if (targetDamageable.TakeDamage())
        {
            ServiceLocator.Get<CameraService>().Shake();
        }
        
        m_isFinished = true;
    }

    //TODO SphereCastAll n'est pas alloc et donc très couteux dans le futur réfléchir à une meilleur solution.
    private Transform FindEnemyInDirection()
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            m_owner.TransformCache.position,
            m_configStats.AttackCastRadius,
            m_attackDirection,
            m_configStats.AttackCastDistance,
            m_configStats.AttackStateLayer()
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (((1 << hit.collider.gameObject.layer) & m_configStats.ObstacleLayer.value) != 0)
                return null;

            if (((1 << hit.collider.gameObject.layer) & m_configStats.EnemyLayer.value) != 0)
                return hit.collider.transform;
        }

        return null;
    }
}