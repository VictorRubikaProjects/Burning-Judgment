using UnityEngine;

[CreateAssetMenu(fileName = "SO_ConfigEnemyShoot", menuName = "Config/Enemy/Shooter")]
public class SO_ConfigEnemyShoot : ScriptableObject
{
    [field: SerializeField] public ProjectileEnemyClassic projectilePrefabs {get; private set;}
    
    [field : SerializeField] public float Weight {get; private set;}
    
    [field : SerializeField] public float KnockbackResistance {get; private set;}
    
    [field : SerializeField] public float Speed {get; private set;}
    
    [field : SerializeField] public float DistanceMax {get; private set;}
    
    [field : SerializeField] public float DistanceMin {get; private set;}
    [field : SerializeField] public float Force {get; private set;}
    [field : SerializeField] public float FrequencyShoot {get; private set;}
}
