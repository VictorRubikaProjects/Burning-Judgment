using UnityEngine;

public readonly struct StrikeContext
{
    public readonly Vector3 Direction;
    public readonly float Force;
    public readonly float Destabilization;

    public StrikeContext(Vector3 direction, float force, float destabilization)
    {
        Direction = direction;
        Force = force;
        Destabilization = destabilization;
    }
}