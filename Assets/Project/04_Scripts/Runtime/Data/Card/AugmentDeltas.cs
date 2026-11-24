using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct AugmentDeltas
{
    [field: Header("Dash")]
    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la longueur maximale du dash.")]
    public float DashMaxDistance { get; private set; } 

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la largeur du dash.")]
    public float DashAttackRadius { get; private set; } 

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation en % de la vitesse du dash.")]
    public float DashSpeed { get; private set; } 

    [field: Space, Header("Combat & Stats")]
    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation de la force de poussée.")]
    public float PushForce { get; private set; } 

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation du poids.")]
    public float Weight { get; private set; } 

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation de la chance critique.")]
    public float CriticalChance { get; private set; }

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation des dégâts critiques.")]
    public float CriticalDamage { get; private set; }

    [field: SerializeField, Range(-90f, 100f)]
    [field: Tooltip("Variation du multiplicateur du combo.")]
    public float ComboMultiplier { get; private set; }

    public List<Tuple<string, float>> GetAllAugmentFromDelta()
    {
        List<Tuple<string, float>> delta = new List<Tuple<string, float>>
        {
            new("Dash Distance", DashMaxDistance),
            new("Attack Range", DashAttackRadius),
            new("Speed", DashSpeed),
            new("Strenght", PushForce), 
            new("Weight", Weight),
            new("Crit Chance", CriticalChance),
            new("Crit Damage", CriticalDamage),
            new("Combo Multiplier", ComboMultiplier)
        };
        return delta;
    }

    public static AugmentDeltas operator +(AugmentDeltas a, AugmentDeltas b)
    {
        return new AugmentDeltas
        {
            DashMaxDistance = a.DashMaxDistance + b.DashMaxDistance,
            DashAttackRadius = a.DashAttackRadius + b.DashAttackRadius,
            DashSpeed = a.DashSpeed + b.DashSpeed,
            PushForce = a.PushForce + b.PushForce,
            Weight = a.Weight + b.Weight,
            CriticalChance = a.CriticalChance + b.CriticalChance,
            CriticalDamage = a.CriticalDamage + b.CriticalDamage,
            ComboMultiplier = a.ComboMultiplier + b.ComboMultiplier
        };
    }
}