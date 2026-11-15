using UnityEngine;

public class BouncyWall : MonoBehaviour, IForcedMovementObstacle
{
    private const float DirectionEpsilon = 0.0001f;

    [SerializeField, Range(0f, 1.5f)] private float restitution = 0.8f;
    [SerializeField, Min(0f)] private float minBounceDistance = 1f;
    [SerializeField, Min(0f)] private float maxBounceDistance = 8f;
    [SerializeField, Min(1)] private int maxBounces = 3;

    private void OnValidate()
    {
        maxBounceDistance = Mathf.Max(minBounceDistance, maxBounceDistance);
    }

    public bool OnForcedMovementHit(in ForcedMovementHit hit)
    {
        if (hit.Mover.GetComponentInParent<IBounceable>() is not { } bounceable) return false;
        if (bounceable.BounceCount >= maxBounces) return false;

        Vector3 normal = hit.Normal;
        normal.y = 0f;
        if (normal.sqrMagnitude < DirectionEpsilon) return false;
        normal.Normalize();

        Vector3 direction = Vector3.Reflect(hit.Direction, normal);
        direction.y = 0f;
        if (direction.sqrMagnitude < DirectionEpsilon) return false;
        direction.Normalize();

        float distance = Mathf.Clamp(hit.Distance * restitution, minBounceDistance, maxBounceDistance);

        bounceable.OnBounced(direction, distance);
        return true;
    }
}