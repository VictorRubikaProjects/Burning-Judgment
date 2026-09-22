using UnityEngine;

public abstract class SO_ActorsStats : ScriptableObject
{
    [Header("Base Physic")]
    [SerializeField] protected float weight = 1f;
    [SerializeField] protected float knockbackResistance = 1f;

    public float Weight => weight;
    public float KnockbackResistance => knockbackResistance;
}