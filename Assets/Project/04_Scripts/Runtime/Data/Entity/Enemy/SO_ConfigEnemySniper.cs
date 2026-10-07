using UnityEngine;

[CreateAssetMenu(menuName = "Config/Actors/Enemy/Sniper", fileName = "SO_ConfigEnemySniper")]
public class SO_ConfigEnemySniper : SO_ConfigEnemy
{
    [field: SerializeField] public float Cooldown { get; private set; } = 3;
    [field: SerializeField] public float ShotRange { get; private set; } = 25;
    [field: SerializeField] public float AimTrackingTime { get; private set; } = 1.5f;
    [field: SerializeField] public float AimFrozenTime { get; private set; } = 0.7f;
    [field : SerializeField] public float ConeAngle {get; private set;} = 30f;
    [field : SerializeField] public LayerMask TargetLayer {get; private set;}
}
