using System;
using UnityEngine;

public abstract class SO_ActorsStats : ScriptableObject
{
    [Header("Base Physic")]
    [SerializeField] protected float weight = 1f;
    [SerializeField] protected float knockbackResistance = 1f;
    
    [Space] [Header("Hit")]
    [SerializeField] [Range(0f, 2f)] private float hitDuration = 0.3f;
    [SerializeField] private AnimationCurve hitKnockbackCurve = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    
    [Space] [Header("Death")]
    [SerializeField] [Range(0f, 5f)] private float deathDelay = 1.5f;
    public float DeathDelay => deathDelay;

    public float Weight => weight;
    public float KnockbackResistance => knockbackResistance;
    
    public float HitDuration => hitDuration;
    public AnimationCurve HitKnockbackCurve => hitKnockbackCurve;

#if UNITY_EDITOR    
    protected virtual void OnValidate()
    {
        if (hitDuration < 0f) hitDuration = 0.01f; 
    }
#endif
}