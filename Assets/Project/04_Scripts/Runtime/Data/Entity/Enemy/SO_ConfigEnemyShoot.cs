using UnityEngine;

[CreateAssetMenu(fileName = "SO_ConfigEnemyShoot", menuName = "Config/Enemy/Shooter")]
public class SO_ConfigEnemyShoot : SO_ConfigEnemy
{
    [field: SerializeField] public ProjectileEnemyClassic ProjectilePrefabs {get; private set;}
    [field : SerializeField] public float FrequencyShoot {get; private set;}
}