using UnityEngine;

public class SteeringComponent : ActorComponent
{
    private readonly Rigidbody m_rigidbody;
    private readonly LayerMask m_obstacleLayer;
    private readonly GroundCheckComponent m_groundCheck;

    private const float EnemyRadius = 1f;
    private const float RaycastHeightOffset = 0.5f;
    private const float MinSlideSqrMagnitude = 0.0001f;

    private static readonly float[] VoidAvoidanceAngles = { 45f, -45f, 90f, -90f };

    public SteeringComponent(Actor owner, Rigidbody rigidbody, LayerMask obstacleLayer, GroundCheckComponent groundCheck) : base(owner)
    {
        m_rigidbody = rigidbody;
        m_obstacleLayer = obstacleLayer;
        m_groundCheck = groundCheck;
    }

    public void MoveTowards(Vector3 targetPosition, float moveSpeed)
    {
        Vector3 currentPosition = Owner.TransformCache.position;
        Vector3 desiredDir = GetDesiredDirection(currentPosition, targetPosition);

        if (desiredDir == Vector3.zero) return;

        float stepDistance = moveSpeed * Time.fixedDeltaTime;
        desiredDir = CalculateObstacleAvoidance(currentPosition, desiredDir, stepDistance + 0.8f);

        if (!TryResolveVoid(currentPosition, desiredDir, stepDistance, out Vector3 finalDir)) return;

        m_rigidbody.MovePosition(currentPosition + finalDir * stepDistance);
    }

    public void Stop() { }

    private Vector3 GetDesiredDirection(Vector3 currentPosition, Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - currentPosition;
        toTarget.y = 0f;
        return toTarget == Vector3.zero ? Vector3.zero : toTarget.normalized;
    }

    private Vector3 CalculateObstacleAvoidance(Vector3 currentPosition, Vector3 desiredDir, float checkDistance)
    {
        Vector3 rayOrigin = currentPosition + Vector3.up * RaycastHeightOffset;

        if (!Physics.SphereCast(rayOrigin, EnemyRadius, desiredDir, out RaycastHit hit, checkDistance, m_obstacleLayer, QueryTriggerInteraction.Ignore))
        {
            return desiredDir;
        }

        Vector3 leftDir = Quaternion.Euler(0f, -45f, 0f) * desiredDir;
        Vector3 rightDir = Quaternion.Euler(0f, 45f, 0f) * desiredDir;

        bool hitLeft = Physics.SphereCast(rayOrigin, EnemyRadius, leftDir, out _, checkDistance * 0.8f, m_obstacleLayer, QueryTriggerInteraction.Ignore);
        bool hitRight = Physics.SphereCast(rayOrigin, EnemyRadius, rightDir, out _, checkDistance * 0.8f, m_obstacleLayer, QueryTriggerInteraction.Ignore);

        if (!hitLeft) return leftDir;
        if (!hitRight) return rightDir;

        return CalculateSlideDirection(desiredDir, hit.normal);
    }

    private Vector3 CalculateSlideDirection(Vector3 desiredDir, Vector3 hitNormal)
    {
        hitNormal.y = 0f;
        if (hitNormal.sqrMagnitude < MinSlideSqrMagnitude) return desiredDir;
        hitNormal.Normalize();

        Vector3 slide = Vector3.ProjectOnPlane(desiredDir, hitNormal);
        if (slide.sqrMagnitude < MinSlideSqrMagnitude) slide = Vector3.Cross(Vector3.up, hitNormal);

        return slide.normalized;
    }

    private bool TryResolveVoid(Vector3 currentPosition, Vector3 desiredDir, float stepDistance, out Vector3 resolvedDir)
    {
        resolvedDir = desiredDir;

        if (m_groundCheck == null) return true;
        if (m_groundCheck.IsGrounded(currentPosition + desiredDir * stepDistance)) return true;

        foreach (float angle in VoidAvoidanceAngles)
        {
            Vector3 candidate = Quaternion.Euler(0f, angle, 0f) * desiredDir;
            if (!m_groundCheck.IsGrounded(currentPosition + candidate * stepDistance)) continue;

            resolvedDir = candidate;
            return true;
        }

        return false;
    }
}