using System.Threading;
using Cysharp.Threading.Tasks;
using Helpers.Runtime.Math;
using UnityEngine;

public static class MovementComponentExtension
{
    public enum CollisionMode
    {
        Stop,
        Slide
    }

    private const float SkinWidth = 0.05f;
    private const float MinStep = 0.0001f;
    private const float MinSlideRatio = 0.15f;
    private const int MaxIterations = 3;
    private const float DebugDuration = 1f;
    private const int DebugSegments = 16;
    private const float GlueSpeedMultiplier = 0.4f;

    public static bool DebugDraw = true;

    public static async UniTask<bool> MoveAsync(
        Rigidbody rb,
        Collider collider,
        Vector3 direction,
        CancellationToken token,
        float duration,
        float distance,
        AnimationCurve curve,
        LayerMask obstacleLayer,
        CollisionMode mode,
        GroundCheckComponent groundCheck = null,
        Actor actor = null)
    {
        if (duration <= 0f || distance <= 0f) return false;
        if (direction.sqrMagnitude < 0.0001f) return false;

        direction.Normalize();

        Vector3 position = rb.position;

        Vector3 centerOffset = GetCenterOffset(collider, position);
        float radius = GetCastRadius(collider);

        float elapsed = 0f;
        float previousProgress = 0f;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            elapsed = Mathf.Min(elapsed + Time.fixedDeltaTime, duration);

            float progress = curve.Evaluate(elapsed / duration);
            Vector3 delta = direction * (distance * (progress - previousProgress));
            previousProgress = progress;

            float zoneMultiplier = GetZoneMovementMultiplier(actor);
            delta *= zoneMultiplier;

            Vector3 origin = position + centerOffset;
            Vector3 moved = ResolveMovement(origin, radius, delta, obstacleLayer, mode, out bool stopped, out ObstacleContact contact);
            Vector3 target = position + moved;

            if (groundCheck != null && !groundCheck.IsGrounded(target))
            {
                target = ResolveVoidSlide(
                    position,
                    target,
                    delta,
                    centerOffset,
                    radius,
                    obstacleLayer,
                    mode,
                    groundCheck,
                    out stopped,
                    out contact);
            }

            rb.MovePosition(target);
            position = target;

            if (contact.HasObstacle && contact.Obstacle.OnForcedMovementHit(
                    new ForcedMovementHit(rb.gameObject, contact.Direction, contact.Normal, contact.Point, distance)))
            {
                return false;
            }

            if (stopped) return false;
        }

        return true;
    }

    private static Vector3 ResolveVoidSlide(
        Vector3 currentPosition,
        Vector3 candidatePosition,
        Vector3 delta,
        Vector3 centerOffset,
        float radius,
        LayerMask obstacleLayer,
        CollisionMode mode,
        GroundCheckComponent groundCheck,
        out bool stopped,
        out ObstacleContact contact)
    {
        stopped = false;
        contact = default;

        Vector3 dropOffset = new Vector3(0f, -2f, 0f);

        if (!Physics.Linecast(candidatePosition + dropOffset, currentPosition + dropOffset, out RaycastHit hit)) return currentPosition;

        Vector3 edgeNormal = Vector3.ProjectOnPlane(hit.normal, Vector3.up);
        
        if (edgeNormal.sqrMagnitude < 0.0001f) return currentPosition;

        edgeNormal.Normalize();

        Vector3 towardSafe = Vector3.ProjectOnPlane(currentPosition - candidatePosition, Vector3.up);
        
        if (towardSafe.sqrMagnitude > 0.0001f && Vector3.Dot(edgeNormal, towardSafe.normalized) < 0f) edgeNormal = -edgeNormal;

        float towardSafeDot = Vector3.Dot(delta, edgeNormal);
        
        Vector3 slideDelta = towardSafeDot < 0f ? Vector3.ProjectOnPlane(delta, edgeNormal) : delta;

        Vector3 origin = currentPosition + centerOffset;
        Vector3 moved = ResolveMovement(origin, radius, slideDelta, obstacleLayer, mode, out stopped, out contact);
        Vector3 resolvedTarget = currentPosition + moved;

        if (groundCheck != null && !groundCheck.IsGrounded(resolvedTarget)) return currentPosition;

        return resolvedTarget;
    }

    public static float GetZoneMovementMultiplier(Actor actor)
    {
        if (actor == null) return 1f;

        if (actor.ZoneMask.ContainsFlag((int)ZoneType.Glue))
        {
            return GlueSpeedMultiplier;
        }

        return 1f;
    }

    public static Vector3 ResolveMovement(
        Vector3 origin,
        float radius,
        Vector3 delta,
        LayerMask obstacleLayer,
        CollisionMode mode,
        out bool stopped)
    {
        return ResolveMovement(origin, radius, delta, obstacleLayer, mode, out stopped, out _);
    }

    public static Vector3 ResolveMovement(
        Vector3 origin,
        float radius,
        Vector3 delta,
        LayerMask obstacleLayer,
        CollisionMode mode,
        out bool stopped,
        out ObstacleContact contact)
    {
        stopped = false;
        contact = default;

        Vector3 moved = Vector3.zero;
        Vector3 remaining = delta;

        for (int i = 0; i < MaxIterations; i++)
        {
            float remainingDistance = remaining.magnitude;

            if (remainingDistance < MinStep) return moved;

            Vector3 stepDirection = remaining / remainingDistance;

            bool hasHit = Physics.SphereCast(
                origin + moved,
                radius,
                stepDirection,
                out RaycastHit hit,
                remainingDistance + SkinWidth,
                obstacleLayer,
                QueryTriggerInteraction.Ignore);

            if (!hasHit) return moved + remaining;

            float allowed = Mathf.Max(0f, hit.distance - SkinWidth);
            moved += stepDirection * allowed;

            IForcedMovementObstacle obstacle = hit.collider.GetComponentInParent<IForcedMovementObstacle>();

            if (obstacle != null)
            {
                contact = new ObstacleContact(obstacle, stepDirection, hit.normal, hit.point);
                stopped = true;
                return moved;
            }

            Vector3 leftover = stepDirection * (remainingDistance - allowed);

            if (mode == CollisionMode.Stop || !TrySlide(leftover, hit.normal, out remaining))
            {
                stopped = true;
                return moved;
            }
        }

        stopped = true;
        return moved;
    }

    private static bool TrySlide(Vector3 leftover, Vector3 normal, out Vector3 slide)
    {
        slide = Vector3.zero;

        normal.y = 0f;

        if (normal.sqrMagnitude < 0.0001f) return false;

        normal.Normalize();

        slide = Vector3.ProjectOnPlane(leftover, normal);

        return slide.magnitude >= leftover.magnitude * MinSlideRatio;
    }

    public static float GetCastRadius(Collider collider)
    {
        Bounds bounds = collider.bounds;
        return Mathf.Max(0.05f, Mathf.Min(bounds.extents.x, bounds.extents.z) - SkinWidth);
    }

    public static Vector3 GetCenterOffset(Collider collider, Vector3 position)
    {
        return collider.bounds.center - position;
    }

}