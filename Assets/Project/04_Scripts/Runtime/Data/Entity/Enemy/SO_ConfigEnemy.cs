using UnityEngine;

public class SO_ConfigEnemy: SO_ActorsStats
{
    [field : SerializeField] public float Speed {get; private set;}
    [field : SerializeField] public float RotationSpeed {get; private set;}

    [field : SerializeField] public float DistanceMax {get; private set;}
    [field : SerializeField] public float DistanceMin {get; private set;}
}