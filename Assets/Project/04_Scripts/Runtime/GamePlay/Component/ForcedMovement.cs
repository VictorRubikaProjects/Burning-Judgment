using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public static class ForcedMovement
{
    private const float SkinWidth = 0.05f;
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
        GroundCheckComponent groundCheck = null)
    {
        if (duration <= 0f || distance <= 0f) return false;
        if (direction.sqrMagnitude < 0.0001f) return false;

        direction.Normalize();

        Vector3 start = rb.position;
        Vector3 position = start;

        Bounds bounds = collider.bounds;
        Vector3 centerOffset = bounds.center - start;
        float radius = Mathf.Max(0.05f, Mathf.Min(bounds.extents.x, bounds.extents.z) - SkinWidth);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, token);

            elapsed = Mathf.Min(elapsed + Time.fixedDeltaTime, duration);

            Vector3 target = start + direction * (distance * curve.Evaluate(elapsed / duration));
            Vector3 intended = target;

            bool blocked = TryClampToObstacle(position + centerOffset, radius, ref target, position, obstacleLayer);

            DrawStep(position + centerOffset, intended + centerOffset, target + centerOffset, radius, blocked);

            if (groundCheck != null && !groundCheck.IsGrounded(target)) return false;

            rb.MovePosition(target);
            position = target;

            if (blocked) return false;
        }

        return true;
    }

    private static bool TryClampToObstacle(
        Vector3 origin,
        float radius,
        ref Vector3 target,
        Vector3 from,
        LayerMask obstacleLayer)
    {
        Vector3 step = target - from;
        float stepDistance = step.magnitude;

        if (stepDistance < 0.0001f) return false;

        Vector3 stepDirection = step / stepDistance;

        if (!Physics.SphereCast(
                origin,
                radius,
                stepDirection,
                out RaycastHit hit,
                stepDistance + SkinWidth,
                obstacleLayer,
                QueryTriggerInteraction.Ignore))
            return false;

        target = from + stepDirection * Mathf.Max(0f, hit.distance - SkinWidth);
        return true;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void DrawStep(Vector3 origin, Vector3 intended, Vector3 final, float radius, bool blocked)
    {
        if (!DebugDraw) return;

        Color color = blocked ? Color.red : Color.green;

        Debug.DrawLine(origin, intended, Color.yellow, DebugDuration);
        Debug.DrawLine(origin, final, color, DebugDuration);

        DrawWireSphere(origin, radius, Color.cyan);
        DrawWireSphere(final, radius, color);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private static void DrawWireSphere(Vector3 center, float radius, Color color)
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
}