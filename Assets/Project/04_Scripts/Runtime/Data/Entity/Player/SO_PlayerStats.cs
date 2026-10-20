using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Actors/Player Stats", fileName = "SO_PlayerStats")]
public class SO_PlayerStats : SO_ActorsStats
{
    private const float MinGain = 0.1f;
    private const float MaxGain = 4f;
    private const float MinDuration = 0.01f;
    
    [field: Space, Header("Roll")]
    [field: SerializeField, Range(0f, 20f)]
    [field: Tooltip("Distance maximale parcourue par le roll, en unités. Atteinte quand le tap est à MaxTapDistance de l'origine.")]
    public float MaxRollDistance { get; private set; } = 5f;

    [field: SerializeField, Range(0.01f, 1f)]
    [field: Tooltip("Durée du roll en secondes. Plus c'est court, plus le roll est rapide.")]
    public float RollDuration { get; private set; } = 0.25f;

    [field: SerializeField]
    [field: Tooltip("Forme de l'accélération du roll. X : temps (0 = début, 1 = fin). Y : distance parcourue (0 = départ, 1 = arrivée).")]
    public AnimationCurve RollCurve { get; private set; } = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    
    [field: Header("Dash")]
    [field: SerializeField, Range(0f, 20f), FormerlySerializedAs("dashDistance")]
    [field: Tooltip("Distance parcourue par le dash, en unités.")]
    public float DashDistance { get; private set; } = 7f;

    [field: SerializeField, Range(0.01f, 1f), FormerlySerializedAs("dashDuration")]
    [field: Tooltip("Durée du dash en secondes. Plus c'est court, plus le dash est rapide.")]
    public float DashDuration { get; private set; } = 0.2f;

    [field: SerializeField, FormerlySerializedAs("dashCurve")]
    [field: Tooltip("Forme de l'accélération du dash. X : temps (0 = début, 1 = fin). Y : distance parcourue (0 = départ, 1 = arrivée).")]
    public AnimationCurve DashCurve { get; private set; } = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    [field: SerializeField, FormerlySerializedAs("enemyLayer")]
    [field: Tooltip("Layers des ennemis que le joueur peut toucher avec le dash et l'attaque.")]
    public LayerMask EnemyLayer { get; private set; }

    [field: SerializeField, Range(0f, 5f), FormerlySerializedAs("dashRadius")]
    [field: Tooltip("Rayon de la zone de détection autour du joueur pendant le dash.")]
    public float DashRadius { get; private set; } = 2f;

    [field: SerializeField, Range(0f, 3f), FormerlySerializedAs("offsetDash")]
    [field: Tooltip("Décalage de la zone de détection du dash vers l'avant du joueur.")]
    public float OffsetDash { get; private set; } = 2f;

    [field: Space, Header("Attack")]
    [field: SerializeField, Range(0f, 5f), FormerlySerializedAs("attackCastRadius")]
    [field: Tooltip("Épaisseur de la zone de détection de l'attaque. Plus c'est large, plus il est facile de viser un ennemi.")]
    public float AttackCastRadius { get; private set; } = 0.5f;

    [field: SerializeField, Range(0f, 40f), FormerlySerializedAs("attackCastDistance")]
    [field: Tooltip("Portée maximale de l'attaque, en unités. Au-delà, aucun ennemi n'est ciblé.")]
    public float AttackCastDistance { get; private set; } = 5f;

    [field: SerializeField, FormerlySerializedAs("attackDash")]
    [field: Tooltip("Paramètres du dash effectué pendant l'attaque (durée, courbe...).")]
    public DashData AttackDash { get; private set; } = new() { Duration = 0.15f };

    [field: SerializeField, Range(0f, 3f), FormerlySerializedAs("attackStopOffset")]
    [field: Tooltip("Distance à laquelle le joueur s'arrête avant la cible pendant l'attaque.")]
    public float AttackStopOffset { get; private set; } = 0.5f;

    [field: Space, Header("Input")]
    [field: SerializeField, Range(0.1f, 3f)]
    [field: Tooltip("Durée en secondes pour atteindre la charge maximale du joystick. Au-delà, la distance du dash ne augmente plus.")]
    public float MaxChargeTime { get; private set; } = 1.5f;

    [field: SerializeField, Range(0f, 1f)]
    [field: Tooltip("Ratio minimal de charge appliqué même pour un relâchement immédiat. 0.2 = 20% de la distance de dash maximale.")]
    public float MinChargeRatio { get; private set; } = 0.2f;

    [field: SerializeField, Range(0f, 1f)]
    [field: Tooltip("Inclinaison minimale du joystick pour que la direction soit prise en compte.")]
    public float MinStickMagnitude { get; private set; } = 0.3f;
    
    [field: SerializeField]
    [field: Tooltip("Inverse la direction du dash : tirer le joystick vers l'arrière lance le dash vers l'avant, comme un lance-pierre.")]
    public bool InvertStickDirection { get; private set; } = false;
    [field: SerializeField, Range(50f, 1500f)]
    [field: Tooltip("Distance maximale en pixels entre l'origine et le tap. Au-delà, le roll est à sa distance maximale.")]
    public float MaxTapDistance { get; private set; } = 400f;

    [field: SerializeField, Range(0.05f, 1f)]
    [field: Tooltip("Durée maximale en secondes entre l'appui et le relèvement pour que ce soit un tap. Au-delà, ce n'est plus un tap.")]
    public float MaxTapDuration { get; private set; } = 0.2f;

    [field: Space, Header("Animation")]
    [field: SerializeField, Range(0f, 0.9f), FormerlySerializedAs("crossFadeDuration")]
    [field: Tooltip("Durée de la transition entre deux animations, en secondes.")]
    public float CrossFadeDuration { get; private set; } = 0.1f;

    [field: Space, Header("Recover")]
    [field: SerializeField, Range(0f, 2f), FormerlySerializedAs("recoverDuration")]
    [field: Tooltip("Durée en secondes pendant laquelle le joueur est en récupération après une attaque qui touche.")]
    public float RecoverDuration { get; private set; } = 0.2f;

    [field: SerializeField, Range(0f, 0.5f), FormerlySerializedAs("recoverCancelThreshold")]
    [field: Tooltip("Délai en secondes après le début de la récupération avant de pouvoir l'annuler avec un dash vers l'arrière.")]
    public float RecoverCancelThreshold { get; private set; } = 0.1f;

    [field: Space, Header("Bounce")]
    [field: SerializeField, Range(0.1f, 1f)]
    [field: Tooltip("Fenêtre de temps en secondes pour enchaîner un rebond.")]
    public float BounceThresholdTimer { get; private set; } = 0.1f;

    [field: SerializeField, Range(1f, 2f)]
    [field: Tooltip("Multiplicateur appliqué à chaque rebond réussi. 1 = aucun bonus, 2 = double.")]
    public float BounceMultiplier { get; private set; } = 1.15f;

    [field: SerializeField, Range(0.1f, 3f)]
    [field: Tooltip("Rayon de la zone de frappe à l'arrivée du dash. Elle est centrée à la distance d'arrêt devant le joueur : plus c'est grand, plus l'attaque pardonne un dash raccourci.")]
    public float AttackStrikeRadius { get; private set; } = 0.75f;
    
    [field: Space, Header("Audio")]
    [field: SerializeField, Tooltip("Son joué quand le joueur prend un coup.")]
    public EventReference HitEvent { get; private set; }

    [field: SerializeField, Tooltip("Son joué pendant le dash.")]
    public EventReference DashEvent { get; private set; }

    [field: SerializeField, Tooltip("Son joué pendant le dash d'attaque.")]
    public EventReference AttackDashEvent { get; private set; }

    public LayerMask AttackStateLayer() => EnemyLayer | ObstacleLayer;

    public SO_PlayerStats CreateRuntimeCopy(in AugmentDeltas deltas)
    {
        SO_PlayerStats copy = Instantiate(this);
        copy.name = $"{name} (Runtime)";
        copy.Apply(deltas);
        return copy;
    }

    private void Apply(in AugmentDeltas deltas)
    {
        DashDistance *= Gain(deltas.DashDistance);
        AttackCastDistance *= Gain(deltas.AttackRange);
        PushForce *= Gain(deltas.PushForce);
        InvincibilityDuration *= Gain(deltas.Invincibility);

        DashDuration = Mathf.Max(MinDuration, DashDuration / Gain(deltas.DashSpeed));
        RecoverDuration = Mathf.Max(0f, RecoverDuration / Gain(deltas.RecoverSpeed));

        DashData attackDash = AttackDash;
        attackDash.Duration = Mathf.Max(MinDuration, attackDash.Duration / Gain(deltas.AttackSpeed));
        AttackDash = attackDash;

        BounceMultiplier = 1f + (BounceMultiplier - 1f) * Gain(deltas.BounceMultiplier);
    }

    private static float Gain(float percent) => Mathf.Clamp(1f + percent / 100f, MinGain, MaxGain);
}