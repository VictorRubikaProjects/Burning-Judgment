using Helpers.Runtime.Physics;
using UnityEngine;

public class PlayerBounceComponent : ActorComponent
{
    private readonly SO_PlayerStats m_stats;

    private float m_openTime;
    private float m_closeTime = -1f;
    private int m_combo;

    public IBouncy Target { get; private set; }
    private Transform m_currentTarget;

    public bool CanBounce =>
        Target == null &&
        Time.time >= m_openTime &&
        Time.time <= m_closeTime;

    public PlayerBounceComponent(Actor owner, SO_PlayerStats stats) : base(owner)
    {
        m_stats = stats;
    }

    public void StartBounceSequence(float delayBeforeOpen, float openDuration, Transform currentTarget)
    {
        m_openTime = Time.time + delayBeforeOpen;
        m_closeTime = m_openTime + openDuration;
        m_currentTarget = currentTarget;
    }

    public void Break()
    {
        m_openTime = 0f;
        m_closeTime = -1f;
        m_combo = 0;
        Target = null;
        m_currentTarget = null;
    }

    public void TryBounce(Vector3 direction)
    {
        if (!CanBounce) return;

        bool found = PhysicsHelper.TryFindElementInDirection(
            Owner.TransformCache.position,
            m_stats.AttackCastRadius,
            direction,
            m_stats.AttackCastDistance,
            m_stats.AttackStateLayer(),
            out IBouncy bouncy,
            out _,
            m_currentTarget);

        if (!found) return;

        Target = bouncy;
    }

    public Transform ClearBufferBouncy()
    {
        Transform targetTransform = Target.TargetTransform;
        Target = null;
        return targetTransform;
    }

    public void SuccessBounce() => m_combo++;
}