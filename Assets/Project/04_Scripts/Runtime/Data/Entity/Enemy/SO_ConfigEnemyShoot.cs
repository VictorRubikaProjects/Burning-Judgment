using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Actors/Enemy/Shooter", fileName = "SO_ConfigEnemyShoot")]
public class SO_ConfigEnemyShoot : SO_ConfigEnemy
{
    [field: SerializeField, FormerlySerializedAs("<ProjectilePrefabs>k__BackingField")]
    [field: Tooltip("Prefab du projectile tiré par l'ennemi.")]
    public ProjectileEnemyClassic ProjectilePrefab { get; private set; }

    [field: SerializeField, FormerlySerializedAs("<FrequencyShoot>k__BackingField")]
    [field: Tooltip("Fréquence de tir de l'ennemi.")]
    public float FrequencyShoot { get; private set; }
}