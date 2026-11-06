using System.Threading;
using Cysharp.Threading.Tasks;
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
        GroundCheckComponent groundCheck = null)
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

            Vector3 origin = position + centerOffset;
            Vector3 moved = ResolveMovement(origin, radius, delta, obstacleLayer, mode, out bool stopped);
            Vector3 target = position + moved;
            
            if (groundCheck != null && !groundCheck.IsGrounded(target)) return false;

            rb.MovePosition(target);
            position = target;

            if (stopped) return false;
        }

        return true;
    }

    public static Vector3 ResolveMovement(
        Vector3 origin,
        float radius,
        Vector3 delta,
        LayerMask obstacleLayer,
        CollisionMode mode,
        out bool stopped)
    {
        stopped = false;

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

#if UNITY_EDITOR
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DrawStep(Vector3 origin, Vector3 intended, Vector3 moved, float radius, bool stopped)
    {
        if (!DebugDraw) return;

        bool slid = (moved - intended).sqrMagnitude > 0.000001f;
        Color color = stopped ? Color.red : slid ? new Color(1f, 0.5f, 0f) : Color.green;

        Debug.DrawLine(origin, origin + intended, Color.yellow, DebugDuration);
        Debug.DrawLine(origin, origin + moved, color, DebugDuration);

        DrawWireSphere(origin, radius, Color.cyan);
        DrawWireSphere(origin + moved, radius, color);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DrawWireSphere(Vector3 center, float radius, Color color)
    {
        float angleStep = Mathf.PI * 2f / DebugSegments;

        for (int i = 0; i < DebugSegments; i++)
        {
            float a0 = i * angleStep;
            float a1 = (i + 1) * angleStep;

            float c0 = Mathf.Cos(a0) * radius;
            float s0 = Mathf.Sin(a0) * radius;
            float c1 = Mathf.Cos(a1) * radius;
            float s1 = Mathf.Sin(a1) * radius;

            Debug.DrawLine(center + new Vector3(c0, 0f, s0), center + new Vector3(c1, 0f, s1), color, DebugDuration);
            Debug.DrawLine(center + new Vector3(c0, s0, 0f), center + new Vector3(c1, s1, 0f), color, DebugDuration);
            Debug.DrawLine(center + new Vector3(0f, c0, s0), center + new Vector3(0f, c1, s1), color, DebugDuration);
        }
    }
    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DrawWireCapsule(Vector3 center, Vector3 direction, float distance, float radius, Color color)
    {
        direction = direction.normalized;
        float halfDistance = distance / 2f;
    
        Vector3 point1 = center + direction * halfDistance;
        Vector3 point2 = center - direction * halfDistance;

        Vector3 forward = direction;
        Vector3 up = Vector3.Slerp(forward, -forward, 0.5f);
        Vector3.OrthoNormalize(ref forward, ref up);
        Vector3 right = Vector3.Cross(forward, up).normalized * radius;
        up *= radius;

        float angleStep = Mathf.PI * 2f / DebugSegments;

        for (int i = 0; i < DebugSegments; i++)
        {
            float angle = i * angleStep;
            Vector3 offset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up);
            Debug.DrawLine(point1 + offset, point2 + offset, color, DebugDuration);
        }

        for (int i = 0; i < DebugSegments; i++)
        {
            float a0 = i * angleStep;
            float a1 = (i + 1) * angleStep;

            float c0 = Mathf.Cos(a0);
            float s0 = Mathf.Sin(a0);
            float c1 = Mathf.Cos(a1);
            float s1 = Mathf.Sin(a1);

            Debug.DrawLine(point1 + c0 * right + s0 * up, point1 + c1 * right + s1 * up, color, DebugDuration);
            Debug.DrawLine(point2 + c0 * right + s0 * up, point2 + c1 * right + s1 * up, color, DebugDuration);
        }
    }
#endif
    
}