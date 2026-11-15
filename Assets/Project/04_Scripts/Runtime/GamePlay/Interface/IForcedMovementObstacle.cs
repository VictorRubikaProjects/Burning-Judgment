using UnityEngine;

public readonly struct ForcedMovementHit
{
    public GameObject Mover { get; }
    public Vector3 Direction { get; }
    public Vector3 Normal { get; }
    public Vector3 Point { get; }
    public float Distance { get; }

    public ForcedMovementHit(GameObject mover, Vector3 direction, Vector3 normal, Vector3 point, float distance)
    {
        Mover = mover;
        Direction = direction;
        Normal = normal;
        Point = point;
        Distance = distance;
    }
}

public interface IForcedMovementObstacle
{
    bool OnForcedMovementHit(in ForcedMovementHit hit);
}

public readonly struct ObstacleContact
{
    public IForcedMovementObstacle Obstacle { get; }
    public Vector3 Direction { get; }
    public Vector3 Normal { get; }
    public Vector3 Point { get; }
    public bool HasObstacle => Obstacle != null;

    public ObstacleContact(IForcedMovementObstacle obstacle, Vector3 direction, Vector3 normal, Vector3 point)
    {
        Obstacle = obstacle;
        Direction = direction;
        Normal = normal;
        Point = point;
    }
}