using UnityEngine;

public class SO_ConfigEnemyExpansion : SO_ConfigEnemy
{
    [field: SerializeField] public float CooldownAttack { get; private set; } = 1f;
    [field: SerializeField] public float TimerChargeAttack { get; private set; } = 0.3f;
    [field: SerializeField] public float RadiusAttack { get; private set; } = 3f;
    [field: SerializeField] public LayerMask PlayerLayer { get; private set; } 
}