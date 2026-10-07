using Event_Bus;
using UnityEngine;

public class EnemySniperAttackState : EnemyBaseState
{
    public bool IsFinished { get; private set; }

    private readonly EnemySniper m_sniper;
    private readonly float m_trackingTime;
    private readonly float m_totalTime;

    private float m_elapsed;
    private bool m_isLocked;
    
    private bool IsAimComplete => m_elapsed >= m_totalTime;

    public EnemySniperAttackState(EnemySniper owner) : base(owner)
    {
        m_sniper = owner;
        m_trackingTime = owner.Stats.AimTrackingTime;
        m_totalTime = Mathf.Max(0.01f, owner.Stats.AimTrackingTime + owner.Stats.AimFrozenTime);
    }

    public override void OnEnter()
    {
        base.OnEnter();
        IsFinished = false;
        m_isLocked = false;
        m_elapsed = 0f;

        m_sniper.AimVisual.Show();
        UpdateVisual();
    }

    public override void OnExit()
    {
        base.OnExit();
        m_sniper.AimVisual.Hide();
        m_sniper.ConsumeShootRequest();
    }

    public override void Update()
    {
        base.Update();

        if (IsFinished) return;

        m_elapsed += Time.deltaTime;

        UpdateAiming();
        UpdateVisual();

        if (IsAimComplete) Fire();
    }

    private void UpdateVisual()
    {
        float progress = Mathf.Clamp01(m_elapsed / m_totalTime);

        m_sniper.AimVisual.UpdateAim(
            m_sniper.ShootPoint.position,
            m_sniper.TransformCache.forward,
            progress);
    }
    
    private void UpdateAiming()
    {
        if (m_elapsed < m_trackingTime)
        {
            m_sniper.LookAtPlayer();
            return;
        }

        LockAim();
    }

    private void LockAim()
    {
        if (m_isLocked) return;

        m_isLocked = true;
        m_sniper.AimVisual.SetLocked(true);
    }

    private void Fire()
    {
        ExecuteShoot();
        IsFinished = true;
    }

    private void ExecuteShoot()
    {
        Ray r = new Ray(m_sniper.ShootPoint.position, m_sniper.TransformCache.forward);

        if (!Physics.Raycast(
                r.origin,
                r.direction,
                out RaycastHit hit,
                m_sniper.Stats.ShotRange,
                m_sniper.Stats.TargetLayer,
                QueryTriggerInteraction.Ignore)) return;

        PlayerCharacter player = hit.transform.GetComponentInParent<PlayerCharacter>();

        if (!player) return;

        EventBus<ActorPushedEvent>.Raise(new ActorPushedEvent(
            player,
            m_sniper.TransformCache.forward,
            m_sniper.Stats.Force));
    }
}