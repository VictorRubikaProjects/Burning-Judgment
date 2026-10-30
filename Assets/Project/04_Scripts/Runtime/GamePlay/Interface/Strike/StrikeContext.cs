using UnityEngine;

public readonly struct StrikeContext
{
    public readonly Vector3 Direction;
    public readonly float Force;

    public StrikeContext(Vector3 direction, float force)
    {
        Direction = direction;
        Force = force;
    }
}