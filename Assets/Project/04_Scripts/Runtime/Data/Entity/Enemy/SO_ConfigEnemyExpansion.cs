using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Actors/Enemy/Expansion", fileName = "SO_ConfigEnemyExpansion")]
public class SO_ConfigEnemyExpansion : SO_ConfigEnemy
{
    [field: Header("Attack")]
    [field: SerializeField, FormerlySerializedAs("<TimerChargeAttack>k__BackingField")]
    [field: Tooltip("Durée de charge en secondes avant l'explosion. Le joueur peut l'esquiver pendant ce temps.")]
    public float ChargeDuration { get; private set; } = 0.3f;

    [field: SerializeField, FormerlySerializedAs("<CooldownAttack>k__BackingField")]
    [field: Tooltip("Temps d'attente en secondes avant de pouvoir attaquer à nouveau.")]
    public float AttackCooldown { get; private set; } = 1f;

    [field: SerializeField, FormerlySerializedAs("<RadiusAttack>k__BackingField")]
    [field: Tooltip("Rayon de l'explosion autour de l'ennemi.")]
    public float AttackRadius { get; private set; } = 3f;

    [field: SerializeField, FormerlySerializedAs("<PlayerLayer>k__BackingField")]
    [field: Tooltip("Layer du joueur. Sert à détecter si l'explosion touche le joueur.")]
    public LayerMask PlayerLayer { get; private set; }

    [field: Space, Header("Falloff")]
    [field: SerializeField, FormerlySerializedAs("<FalloffCurve>k__BackingField")]
    [field: Tooltip("Force de l'explosion selon la distance. X : 0 = au centre, 1 = au bord du rayon. Y : multiplicateur de la force (1 = 100 %).")]
    public AnimationCurve FalloffCurve { get; private set; } = AnimationCurve.Linear(0f, 1f, 1f, 0.25f);

    public float GetFalloffMultiplier(float distance)
    {
        float normalized = Mathf.Clamp01(distance / AttackRadius);
        return FalloffCurve.Evaluate(normalized);
    }
}