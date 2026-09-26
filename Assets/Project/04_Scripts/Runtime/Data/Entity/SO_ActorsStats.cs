using System;
using UnityEngine;

public abstract class SO_ActorsStats : ScriptableObject
{
    [Header("Base Physic")]
    [SerializeField] protected float weight = 1f;
    [SerializeField] protected float knockbackResistance = 1f;
    [field : SerializeField] public float Force {get; private set;}
    
    [Space] [Header("Hit")]
    [SerializeField] [Range(0f, 2f)] private float hitDuration = 0.3f;
    [SerializeField] private AnimationCurve hitKnockbackCurve = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    [SerializeField] [Range(0f, 2f)] private float invincibilityDuration = 0.5f;

    [Space] [Header("Ground Check")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] [Range(0f, 3f)] private float groundCheckHeight = 0.5f;
    [SerializeField] [Range(0f, 3f)] private float groundCheckDistance = 1f;
    
    [Space] [Header("Death")]
    [SerializeField] [Range(0f, 5f)] private float deathDelay = 1.5f;
    public float DeathDelay => deathDelay;

    public float Weight => weight;
    public float KnockbackResistance => knockbackResistance;
    
    public float HitDuration => hitDuration;
    public AnimationCurve HitKnockbackCurve => hitKnockbackCurve;
    public float InvincibilityDuration => invincibilityDuration;

    public LayerMask GroundLayer => groundLayer;
    public float GroundCheckHeight => groundCheckHeight;
    public float GroundCheckDistance => groundCheckDistance;

#if UNITY_EDITOR    
    protected virtual void OnValidate()
    {
        if (hitDuration < 0f) hitDuration = 0.01f; 
    }
#endif
}