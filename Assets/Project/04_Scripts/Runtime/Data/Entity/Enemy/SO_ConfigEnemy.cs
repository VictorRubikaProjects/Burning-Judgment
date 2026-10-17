using UnityEngine;
using UnityEngine.Serialization;

public class SO_ConfigEnemy : SO_ActorsStats
{
    [field: Header("Movement")]
    [field: SerializeField, FormerlySerializedAs("<Speed>k__BackingField")]
    [field: Tooltip("Vitesse de déplacement de l'ennemi, en unités par seconde.")]
    public float Speed { get; private set; }

    [field: SerializeField, FormerlySerializedAs("<RotationSpeed>k__BackingField")]
    [field: Tooltip("Vitesse à laquelle l'ennemi se tourne vers sa cible. Plus c'est haut, plus il pivote vite.")]
    public float RotationSpeed { get; private set; }

    [field: Space, Header("Distances")]
    [field: SerializeField, FormerlySerializedAs("<DistanceMin>k__BackingField")]
    [field: Tooltip("Distance minimale au joueur pour que l'ennemi agisse (attaque). En dessous, il n'agit pas.")]
    public float DistanceMin { get; private set; }

    [field: SerializeField, FormerlySerializedAs("<DistanceMax>k__BackingField")]
    [field: Tooltip("Distance maximale au joueur pour que l'ennemi agisse (attaque). Au-delà, il se rapproche.")]
    public float DistanceMax { get; private set; }

    [field: Space, Header("Hit Detection")]
    [field: SerializeField, FormerlySerializedAs("<HitRadius>k__BackingField")]
    [field: Tooltip("Rayon de la zone dans laquelle l'ennemi peut être touché par le joueur.")]
    public float HitRadius { get; private set; } = 1f;

    [field: SerializeField, FormerlySerializedAs("<EnemyLayer>k__BackingField")]
    [field: Tooltip("Layer des ennemis. Sert à détecter les autres ennemis autour de celui-ci.")]
    public LayerMask EnemyLayer { get; private set; }
    
    [field: Space, Header("Sight")]
    [field: SerializeField]
    [field: Tooltip("Hauteur au-dessus des pieds d'où part le rayon de vue vers le joueur. Un obstacle plus bas que cette hauteur ne bloque pas la vue.")]
    public float SightHeight { get; private set; } = 0.5f;
}