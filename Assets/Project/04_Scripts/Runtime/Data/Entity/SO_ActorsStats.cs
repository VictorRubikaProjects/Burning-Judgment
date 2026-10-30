using UnityEngine;
using UnityEngine.Serialization;

public abstract class SO_ActorsStats : ScriptableObject
{
    [Header("Base Physic")]
    [field:SerializeField, Tooltip("Poids de l'acteur. Plus il est élevé, plus l'acteur est difficile à repousser.")]
    public float Weight { get; protected set; } = 10f;

    [field: SerializeField, FormerlySerializedAs("<Force>k__BackingField")]
    [field: Tooltip("Force de poussée appliquée aux autres acteurs quand celui-ci les repousse (attaque, impact).")]
    public float PushForce { get; protected set; } = 10f;
    
    [field: SerializeField, Tooltip("Échelle du knockback réel (x) pour tout les états sauf cas particuliés par rapport à la stabilité (St) d’une entité.")]
    public float DefaultScaling { get; protected set; } = 0.5f;
    
    [field: SerializeField, Tooltip("Échelle du knockback réel (x) pour les états de faiblesse hit par rapport à la stabilité (St) d’une entité.")]
    public float BadScaling { get; protected set; } = 0.25f;
    
    [field: SerializeField, Tooltip("Multiplicateur sur le knockback réel (x). Diminue de (D) à chaque coup reçu.")]
    public float Stability { get; protected set; } = 1f;
    
    [field: SerializeField, Tooltip(" Facteur définissant l’ampleur de la perte de stabilité (St) de la cible après chaque coup.")]
    public float Destabilization { get; protected set; } = 0.33f;
    
    [field: Space, Header("Obstacles")]
    [field: SerializeField]
    [field: Tooltip("Layers des obstacles qui bloquent les déplacements forcés de l'ennemi (dash).")]
    public LayerMask ObstacleLayer { get; protected set; }

    [field:Space, Header("Hit")]
    [field: SerializeField, Range(0f, 2f), Tooltip("Durée en secondes pendant laquelle l'acteur est étourdi et repoussé après avoir pris un coup.")]
    public float HitDuration { get; protected set; } = 0.3f;

    [field:SerializeField, Tooltip("Forme du recul pendant le hit. X : progression du hit (0 = début, 1 = fin). Y : progression du recul (0 = position de départ, 1 = distance totale).")]
    public AnimationCurve HitKnockbackCurve { get; protected set; } = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    [field:SerializeField, Range(0f, 2f), Tooltip("Durée en secondes pendant laquelle l'acteur ne peut pas reprendre de dégâts après un hit.")]
    public float InvincibilityDuration { get; protected set; } = 0.5f;

    [field:Space, Header("Ground Check")]
    [field:SerializeField, Tooltip("Layers considérés comme du sol. L'acteur tombe s'il ne touche aucun de ces layers.")]
    public LayerMask GroundLayer { get; protected set; }

    [field: SerializeField, Range(0f, 3f), Tooltip("Hauteur au-dessus des pieds d'où part la détection du sol.")]
    public float GroundCheckHeight { get; protected set; } = 0.0f;

    [field: SerializeField, Range(0f, 3f), Tooltip("Longueur de la détection du sol. Trop court : l'acteur tombe sur les pentes. Trop long : il ne tombe pas au bord du vide.")]
    public float GroundCheckDistance { get; protected set; } = 1.5f;

    [field: Space, Header("Death")]
    [field: SerializeField, Range(0f, 5f), Tooltip("Délai en secondes entre la mort de l'acteur et sa disparition.")]
    public float DeathDelay { get; protected set; } = 1.5f;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (HitDuration < 0f) HitDuration = 0.01f;
    }
#endif
}