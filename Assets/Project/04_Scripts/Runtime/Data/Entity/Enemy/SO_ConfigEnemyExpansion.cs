using UnityEngine;

[CreateAssetMenu(menuName = "Config/Actors/Enemy/Expansion", fileName = "SO_ConfigEnemyExpansion")]
public class SO_ConfigEnemyExpansion : SO_ConfigEnemy
{
    [field: SerializeField] public float CooldownAttack { get; private set; } = 1f;
    [field: SerializeField] public float TimerChargeAttack { get; private set; } = 0.3f;
    [field: SerializeField] public float RadiusAttack { get; private set; } = 3f;
    [field: SerializeField] public LayerMask PlayerLayer { get; private set; }
    [field: SerializeField] public AnimationCurve FalloffCurve { get; private set; } = AnimationCurve.Linear(0f, 1f, 1f, 0.25f);

    public float GetFalloffMultiplier(float distance)
    {
        float normalized = Mathf.Clamp01(distance / RadiusAttack);
        return FalloffCurve.Evaluate(normalized);
    }
}