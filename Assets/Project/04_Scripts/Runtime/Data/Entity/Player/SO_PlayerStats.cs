using UnityEngine;

[CreateAssetMenu(menuName = "Config/Actors/Player Stats", fileName = "SO_PlayerStats")]
public class SO_PlayerStats : SO_ActorsStats
{
    private const float MinGain = 0.1f;
    private const float MaxGain = 4f;
    private const float MinDuration = 0.01f;

    [field: Space, Header("Dash")]
    [field: SerializeField, Range(0f, 20f)]
    [field: Tooltip("Distance parcourue par le dash, en unités.")]
    public float DashDistance { get; private set; } = 7f;

    [field: SerializeField, Range(0.01f, 1f)]
    [field: Tooltip("Durée du dash en secondes. Plus c'est court, plus le dash est rapide.")]
    public float DashDuration { get; private set; } = 0.2f;

    [field: SerializeField]
    [field: Tooltip("Forme de l'accélération du dash. X : temps (0 = début, 1 = fin). Y : distance parcourue (0 = départ, 1 = arrivée).")]
    public AnimationCurve DashCurve { get; private set; } = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    [field: Space, Header("Combat & Stats")]
    [field: SerializeField, Range(0f, 100f)]
    [field: Tooltip("Chance critique en pourcent.")]
    public float CriticalChance { get; private set; } = 5f;

    [field: SerializeField, Range(1f, 5f)]
    [field: Tooltip("Multiplicateur des dégâts critiques.")]
    public float CriticalDamage { get; private set; } = 1.5f;

    [field: SerializeField, Range(1f, 5f)]
    [field: Tooltip("Multiplicateur du combo.")]
    public float ComboMultiplier { get; private set; } = 1f;

    [field: Space, Header("Dash - Frappe des ennemis")]
    [field: SerializeField, Range(0.1f, 3f)]
    [field: Tooltip("Rayon de la zone qui touche les ennemis pendant le dash. Plus c'est grand, plus le dash pardonne une visée approximative.")]
    public float DashAttackRadius { get; private set; } = 0.75f;

    [field: SerializeField, Range(0f, 3f)]
    [field: Tooltip("Décalage vers l'avant de la zone qui touche les ennemis, en unités. 0 = centrée sur le joueur.")]
    public float DashAttackOffset { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Layers des ennemis que le dash peut frapper.")]
    public LayerMask DashAttackLayer { get; private set; }

    [field: Space, Header("Dash - Frappe des éléments de LD")]
    [field: SerializeField, Range(0.1f, 3f)]
    [field: Tooltip("Rayon de la zone qui touche les éléments de LD frappables pendant le dash.")]
    public float DashStrikeRadius { get; private set; } = 0.75f;

    [field: SerializeField, Range(0f, 3f)]
    [field: Tooltip("Décalage vers l'avant de la zone qui touche les éléments de LD, en unités. 0 = centrée sur le joueur (risque de frapper un élément simplement frôlé).")]
    public float DashStrikeOffset { get; private set; }

    [field: SerializeField]
    [field: Tooltip("Layers des éléments de LD frappables avec lesquels on peut enchaîner.")]
    public LayerMask DashStrikeLayer { get; private set; }

    [field: Space, Header("Input")]
    [field: SerializeField, Range(0f, 1f)]
    [field: Tooltip("Ratio minimal de charge appliqué même pour un relâchement immédiat ou un joystick à peine poussé. 0.2 = 20% de la distance max.")]
    public float MinChargeRatio { get; private set; } = 0.2f;

    [field: SerializeField, Range(0f, 1f)]
    [field: Tooltip("Inclinaison minimale du joystick pour que la direction soit prise en compte.")]
    public float MinStickMagnitude { get; private set; } = 0.3f;

    [field: SerializeField]
    [field: Tooltip("Inverse la direction du dash : tirer le joystick vers l'arrière lance le dash vers l'avant, comme un lance-pierre.")]
    public bool InvertStickDirection { get; private set; } = false;

    [field: SerializeField, Range(10f, 500f)]
    [field: Tooltip("Distance minimale en pixels du swipe pour qu'il soit pris en compte (déclenchement du roll).")]
    public float MinSwipeDistance { get; private set; } = 50f;

    [field: SerializeField, Range(50f, 1500f)]
    [field: Tooltip("Distance maximale en pixels de l'étirement du joystick (ou swipe). Au-delà, la distance de dash/tir atteint 100% (ratio 1f).")]
    public float MaxSwipeDistance { get; private set; } = 400f;

    [field: SerializeField, Range(0f, 1f)]
    [field: Tooltip("Durée maximale en secondes pour effectuer un swipe. Au-delà, le joystick apparaît.")]
    public float MaxSwipeDuration { get; private set; } = 0.2f;

    [field: Space, Header("Time Control (Slow Motion)")]
    [field: SerializeField, Range(0.01f, 1f)]
    [field: Tooltip("Facteur de dilatation du temps cible lorsque le joueur maintient et charge son joystick.")]
    public float SlowMotionTargetDilation { get; private set; } = 0.2f;

    [field: SerializeField, Range(0.01f, 2f)]
    [field: Tooltip("Durée de la transition pour entrer dans le slow motion.")]
    public float SlowMotionEnterDuration { get; private set; } = 1f;

    [field: SerializeField, Range(0.01f, 2f)]
    [field: Tooltip("Durée de la transition pour revenir à la vitesse normale du temps.")]
    public float SlowMotionExitDuration { get; private set; } = 0.5f;

    [field: Space, Header("Animation")]
    [field: SerializeField]
    [field: Tooltip("Transition et vitesse de l'animation Idle.")]
    public StateAnimationData IdleAnimation { get; private set; } = new() { CrossFadeDuration = 0.1f, Speed = 1f };

    [field: SerializeField]
    [field: Tooltip("Transition et vitesse de l'animation de dash.")]
    public StateAnimationData DashAnimation { get; private set; } = new() { CrossFadeDuration = 0.1f, Speed = 1f };

    [field: SerializeField]
    [field: Tooltip("Transition et vitesse de l'animation de chute.")]
    public StateAnimationData FallAnimation { get; private set; } = new() { CrossFadeDuration = 0.1f, Speed = 1f };

    [field: SerializeField]
    [field: Tooltip("Transition et vitesse de l'animation quand le joueur prend un coup.")]
    public StateAnimationData HitAnimation { get; private set; } = new() { CrossFadeDuration = 0.05f, Speed = 1f };

    [field: SerializeField]
    [field: Tooltip("Transition et vitesse de l'animation quand le joueur prend un coup.")]
    public StateAnimationData AttackAnimation { get; private set; } = new() { CrossFadeDuration = 0.05f, Speed = 1f };

    [field: Space, Header("Chain")]
    [field: SerializeField, Range(0.0f, 5f), Tooltip("Offset de temps pour valider un combo de chain.")]
    public float ChainDurationBeforeClear { get; private set; } = 3f;
    [field: SerializeField, Range(0f, 1f)]
    [field: Tooltip("Bonus de force en pourcentage ajouté par palier de chaîne. 15 = +15% de la force de base par chaîne (3 chaînes = +45%).")]
    public float ChainBonusPercent { get; private set; } = 0.25f;

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
        DashAttackRadius *= Gain(deltas.DashAttackRadius);
        DashDuration = Mathf.Max(MinDuration, DashDuration / Gain(deltas.DashSpeed));
        
        PushForce *= Gain(deltas.PushForce);
        Weight *= Gain(deltas.Weight);
        Stability *= Gain(deltas.Stability);
        CriticalChance += deltas.CriticalChance; 
        CriticalDamage *= Gain(deltas.CriticalDamage);
        ComboMultiplier *= Gain(deltas.ComboMultiplier);
    }

    private static float Gain(float percent) => Mathf.Clamp(1f + percent / 100f, MinGain, MaxGain);
}