using UnityEngine;

[CreateAssetMenu(menuName = "Config/Actors/Enemy/Dasher", fileName = "SO_ConfigEnemyDash")]
public class SO_ConfigEnemyDash : SO_ConfigEnemy
{
    [field: SerializeField] public float DashDuration { get; private set; } = 0.3f;
    [field: SerializeField] public float LockTime { get; private set; } = 0.6f;
    [field : SerializeField] public AnimationCurve CurveDash {get; private set;}
    [field: SerializeField] public float DashCooldown { get; private set; } = 2.5f;
    [field : SerializeField] public LayerMask PlayerLayer {get; private set;}
    [field: SerializeField] public float StrikeRadius { get; private set; } = 1;
    [field: SerializeField] public float StrikeForwardOffset { get; private set; } = 0.5f;
}