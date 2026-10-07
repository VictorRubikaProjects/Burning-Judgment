using UnityEngine;

[CreateAssetMenu(menuName = "Config/Actors/Enemy/Shooter", fileName = "SO_ConfigEnemyShoot")]
public class SO_ConfigEnemyShoot : SO_ConfigEnemy
{
    [field: SerializeField] public ProjectileEnemyClassic ProjectilePrefabs {get; private set;}
    [field : SerializeField] public float FrequencyShoot {get; private set;}
}