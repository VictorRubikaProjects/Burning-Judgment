using UnityEngine;

public class BouncyWall : MonoBehaviour, IForcedMovementObstacle
{
    private const float DirectionEpsilon = 0.0001f;

    [SerializeField, Min(0f)] private float bounceDistance = 4f;
    [SerializeField, Min(1)] private int maxBounces = 3;

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

        bounceable.OnBounced(direction, bounceDistance);
        return true;
    }
}