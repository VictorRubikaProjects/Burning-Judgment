using System;
using UnityEngine;

[Serializable]
public struct AugmentDeltas
{
    [field: Header("Dash")]
    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la distance du dash. +20 = dash 20 % plus long. 0 = aucun effet.")]
    public float DashDistance { get; private set; }

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la vitesse du dash. +20 = dash 20 % plus rapide. 0 = aucun effet.")]
    public float DashSpeed { get; private set; }

    [field: Space, Header("Attack")]
    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la portée de l'attaque. +20 = ennemis ciblés 20 % plus loin.")]
    public float AttackRange { get; private set; }

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la vitesse du dash d'attaque. +20 = attaque 20 % plus rapide.")]
    public float AttackSpeed { get; private set; }

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la force de poussée appliquée aux ennemis. +20 = ennemis repoussés 20 % plus loin.")]
    public float PushForce { get; private set; }

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % du multiplicateur de rebond. +20 = enchaînements de rebonds plus puissants.")]
    public float BounceMultiplier { get; private set; }

    [field: Space, Header("Defense")]
    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la vitesse de récupération après une attaque. +20 = récupération 20 % plus courte.")]
    public float RecoverSpeed { get; private set; }

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la durée d'invincibilité après un coup. +20 = invincibilité 20 % plus longue.")]
    public float Invincibility { get; private set; }

    public static AugmentDeltas operator +(AugmentDeltas a, AugmentDeltas b)
    {
        return new AugmentDeltas
        {
            DashDistance = a.DashDistance + b.DashDistance,
            DashSpeed = a.DashSpeed + b.DashSpeed,
            AttackRange = a.AttackRange + b.AttackRange,
            AttackSpeed = a.AttackSpeed + b.AttackSpeed,
            PushForce = a.PushForce + b.PushForce,
            BounceMultiplier = a.BounceMultiplier + b.BounceMultiplier,
            RecoverSpeed = a.RecoverSpeed + b.RecoverSpeed,
            Invincibility = a.Invincibility + b.Invincibility
        };
    }
}