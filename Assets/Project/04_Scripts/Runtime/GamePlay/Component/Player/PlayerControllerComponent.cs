using UnityEngine;

public class PlayerControllerComponent : ActorComponent
{
    private const float DirectionEpsilon = 0.0001f;
    private const float EnemyConeTolerance = 0f;
    private const float StrikeableConeTolerance = 0.3f;

    private readonly Rigidbody m_rb;
    private readonly PlayerCharacter m_pc;
    private readonly SO_PlayerStats m_stats;
    private readonly Collider[] m_strikeHits = new Collider[16];

    private Quaternion m_targetRotation;
    private bool m_hasTargetRotation;

    public PlayerControllerComponent(Actor owner, Rigidbody rb, SO_PlayerStats stats) : base(owner)
    {
        m_rb = rb;
        m_pc = (PlayerCharacter)owner;
        m_stats = stats;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        HandleRotation();
    }

    public override void Update()
    {
        base.Update();
        CalculateRotation();
    }

    private void CalculateRotation()
    {
        if (m_pc.MoveDir == Vector3.zero) return;
        m_targetRotation = Quaternion.LookRotation(m_pc.MoveDir);
        m_hasTargetRotation = true;
    }

    private void HandleRotation()
    {
        if (!m_hasTargetRotation) return;
        m_rb.MoveRotation(m_targetRotation);
    }

    public readonly struct StrikeData
    {
        public float Radius { get; }
        public LayerMask LayerMask { get; }
        public float Offset { get; }
        public bool IsAttacking { get; }
        public float ForceMultiplier { get; }

        public StrikeData(float radius, LayerMask layerMask, float offset, bool isAttacking, float forceMultiplier = 1)
        {
            Radius = radius;
            LayerMask = layerMask;
            Offset = offset;
            IsAttacking = isAttacking;
            ForceMultiplier = forceMultiplier;
        }
    }

    public StrikeResult TryStrike(StrikeData data) => TryStrike(data, m_pc.MoveDir);

    public StrikeResult TryStrike(StrikeData data, Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < DirectionEpsilon) return StrikeResult.Ignored;
        direction.Normalize();

        Vector3 origin = m_pc.TransformCache.position;
        Vector3 center = origin + direction * data.Offset;
        float tolerance = data.IsAttacking ? EnemyConeTolerance : StrikeableConeTolerance;

        int count = Physics.OverlapSphereNonAlloc(
            center,
            data.Radius,
            m_strikeHits,
            data.LayerMask,
            QueryTriggerInteraction.Collide);

        StrikeContext context = new(origin, direction, m_stats.PushForce * data.ForceMultiplier, m_stats.Destabilization);

        for (int i = 0; i < count; i++)
        {
            Collider hit = m_strikeHits[i];

            if (!IsInFront(hit, origin, direction, tolerance)) continue;

            StrikeResult result = data.IsAttacking
                ? StrikeEnemy(hit, context)
                : StrikeTarget(hit, context);

            if (result.Accepted) return result;
        }

        return StrikeResult.Ignored;
    }

    private static StrikeResult StrikeEnemy(Collider hit, in StrikeContext context)
    {
        return hit.TryGetComponent(out EnemyBase enemy) ? enemy.OnStruck(context) : StrikeResult.Ignored;
    }

    private static StrikeResult StrikeTarget(Collider hit, in StrikeContext context)
    {
        return hit.TryGetComponent(out IStrikeable target) ? target.OnStruck(context) : StrikeResult.Ignored;
    }

    private static bool IsInFront(Collider collider, Vector3 origin, Vector3 direction, float tolerance)
    {
        Vector3 toClosest = collider.ClosestPoint(origin) - origin;
        toClosest.y = 0f;

        if (toClosest.sqrMagnitude < DirectionEpsilon) return true;

        return Vector3.Dot(toClosest.normalized, direction) > tolerance;
    }
}