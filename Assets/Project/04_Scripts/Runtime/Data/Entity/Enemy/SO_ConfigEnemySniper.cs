using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Actors/Enemy/Sniper", fileName = "SO_ConfigEnemySniper")]
public class SO_ConfigEnemySniper : SO_ConfigEnemy
{
    [field: Header("Shot")]
    [field: SerializeField, FormerlySerializedAs("<Cooldown>k__BackingField")]
    [field: Tooltip("Temps d'attente en secondes entre deux tirs.")]
    public float ShotCooldown { get; private set; } = 3f;

    [field: SerializeField, FormerlySerializedAs("<ShotRange>k__BackingField")]
    [field: Tooltip("Portée maximale du tir, en unités.")]
    public float ShotRange { get; private set; } = 25f;

    [field: SerializeField, FormerlySerializedAs("<TargetLayer>k__BackingField")]
    [field: Tooltip("Layers que le tir peut toucher.")]
    public LayerMask TargetLayer { get; private set; }

    [field: Space, Header("Aim")]
    [field: SerializeField, FormerlySerializedAs("<AimTrackingTime>k__BackingField")]
    [field: Tooltip("Durée en secondes pendant laquelle le sniper suit le joueur avec sa visée.")]
    public float AimTrackingTime { get; private set; } = 1.5f;

    [field: SerializeField, FormerlySerializedAs("<AimFrozenTime>k__BackingField")]
    [field: Tooltip("Durée en secondes pendant laquelle la visée reste figée avant le tir. Le joueur peut esquiver pendant ce temps.")]
    public float AimFrozenTime { get; private set; } = 0.7f;

    [field: SerializeField, FormerlySerializedAs("<ConeAngle>k__BackingField")]
    [field: Tooltip("Angle en degrés du cône de visée devant le sniper. Le joueur doit être dans ce cône pour être ciblé.")]
    public float ConeAngle { get; private set; } = 30f;
}