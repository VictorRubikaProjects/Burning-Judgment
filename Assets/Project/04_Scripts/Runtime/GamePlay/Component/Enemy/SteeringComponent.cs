using UnityEngine;

public class SteeringComponent : ActorComponent
{
    private readonly Rigidbody m_rigidbody;
    private readonly Collider m_collider;
    private readonly LayerMask m_obstacleLayer;
    private readonly GroundCheckComponent m_groundCheck;

    private const float AvoidanceDistanceMargin = 0.8f;
    private const float SkinWidth = 0.05f;

    private static readonly float[] ObstacleAvoidanceAngles = { 0f, -30f, 30f, -60f, 60f, -90f, 90f };
    private static readonly float[] VoidAvoidanceAngles = { 0f, 45f, -45f, 90f, -90f };
    private static readonly Collider[] s_overlapBuffer = new Collider[8];

    public SteeringComponent(Actor owner, Rigidbody rigidbody, Collider collider, LayerMask obstacleLayer, GroundCheckComponent groundCheck) : base(owner)
    {
        m_rigidbody = rigidbody;
        m_collider = collider;
        m_obstacleLayer = obstacleLayer;
        m_groundCheck = groundCheck;
    }

    public void MoveTowards(Vector3 targetPosition, float moveSpeed)
    {
        Vector3 startPosition = m_rigidbody.position;
        Vector3 position = ResolvePenetration(startPosition);
        Vector3 desiredDir = GetDesiredDirection(position, targetPosition);

        if (desiredDir == Vector3.zero)
        {
            if (position != startPosition) m_rigidbody.MovePosition(position);
            return;
        }

        float radius = MovementComponentExtension.GetCastRadius(m_collider);
        Vector3 origin = position + MovementComponentExtension.GetCenterOffset(m_collider, startPosition);
        float stepDistance = moveSpeed * Time.fixedDeltaTime;

        Vector3 baseDir = PickFreeDirection(origin, radius, desiredDir, stepDistance + AvoidanceDistanceMargin);
        bool found = TryFindMove(position, origin, radius, baseDir, stepDistance, out Vector3 moved);

        MovementComponentExtension.DrawStep(origin, desiredDir * stepDistance, found ? moved : Vector3.zero, radius, !found);

        m_rigidbody.MovePosition(found ? position + moved : position);
    }

    public void Stop() { }

    private Vector3 GetDesiredDirection(Vector3 currentPosition, Vector3 targetPosition)
    {
        Vector3 toTarget = targetPosition - currentPosition;
        toTarget.y = 0f;
        return toTarget == Vector3.zero ? Vector3.zero : toTarget.normalized;
    }

    private Vector3 PickFreeDirection(Vector3 origin, float radius, Vector3 desiredDir, float checkDistance)
    {
        foreach (float angle in ObstacleAvoidanceAngles)
        {
            Vector3 candidate = Quaternion.Euler(0f, angle, 0f) * desiredDir;

            if (!Physics.SphereCast(origin, radius, candidate, out _, checkDistance, m_obstacleLayer, QueryTriggerInteraction.Ignore))
            {
                return candidate;
            }
        }

        return desiredDir;
    }

    private bool TryFindMove(Vector3 position, Vector3 origin, float radius, Vector3 baseDir, float stepDistance, out Vector3 moved)
    {
        foreach (float angle in VoidAvoidanceAngles)
        {
            Vector3 candidate = Quaternion.Euler(0f, angle, 0f) * baseDir;
            moved = MovementComponentExtension.ResolveMovement(origin, radius, candidate * stepDistance, m_obstacleLayer, MovementComponentExtension.CollisionMode.Slide, out _);

            if (m_groundCheck == null || m_groundCheck.IsGrounded(position + moved)) return true;
        }

        moved = Vector3.zero;
        return false;
    }

    private Vector3 ResolvePenetration(Vector3 position)
    {
        float radius = MovementComponentExtension.GetCastRadius(m_collider);
        Vector3 origin = position + MovementComponentExtension.GetCenterOffset(m_collider, m_rigidbody.position);
        Quaternion rotation = m_rigidbody.rotation;

        int count = Physics.OverlapSphereNonAlloc(origin, radius + SkinWidth, s_overlapBuffer, m_obstacleLayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            Collider other = s_overlapBuffer[i];
            if (other == m_collider) continue;

            bool penetrating = Physics.ComputePenetration(
                m_collider, position, rotation,
                other, other.transform.position, other.transform.rotation,
                out Vector3 direction, out float distance);

            if (!penetrating) continue;

            direction.y = 0f;
            position += direction * distance;
        }

        return position;
    }
}