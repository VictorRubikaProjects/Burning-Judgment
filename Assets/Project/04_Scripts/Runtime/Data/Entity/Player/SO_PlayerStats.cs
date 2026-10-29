using FMODUnity;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Actors/Player Stats", fileName = "SO_PlayerStats")]
public class SO_PlayerStats : SO_ActorsStats
{
    private const float MinGain = 0.1f;
    private const float MaxGain = 4f;
    private const float MinDuration = 0.01f;

    [field: Space, Header("Roll")]
    [field: SerializeField, Range(0f, 20f)]
    [field: Tooltip("Distance maximale parcourue par le roll, en unités. Atteinte quand le swipe est à MaxSwipeDistance de l'origine.")]
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

    [field: SerializeField, Range(0.1f, 3f)]
    [field: Tooltip("Rayon de la zone de frappe autour du joueur pendant le roll. Plus c'est grand, plus le roll touche facilement.")]
    public float RollStrikeRadius { get; private set; } = 0.75f;

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

    [field: SerializeField, Range(0.1f, 3f)]
    [field: Tooltip("Rayon de la zone de frappe autour du joueur pendant le dash. Plus c'est grand, plus le dash pardonne une visée approximative.")]
    public float DashStrikeRadius { get; private set; } = 0.75f;

    [field: Space, Header("Combat")]
    [field: SerializeField]
    [field: Tooltip("Layers des ennemis que le joueur peut toucher avec le dash et le roll.")]
    public LayerMask EnemyLayer { get; private set; }

    [field: Space, Header("Input")]
    [field: SerializeField, Range(0.1f, 3f)]
    [field: Tooltip("Durée en secondes pour atteindre la charge maximale du joystick. Au-delà, la distance du dash n'augmente plus.")]
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

    [field: SerializeField, Range(10f, 500f)]
    [field: Tooltip("Distance minimale en pixels du swipe pour qu'il soit pris en compte (déclenchement du roll).")]
    public float MinSwipeDistance { get; private set; } = 50f;

    [field: SerializeField, Range(50f, 1500f)]
    [field: Tooltip("Distance maximale en pixels du swipe. Au-delà, le roll atteint sa distance maximale.")]
    public float MaxSwipeDistance { get; private set; } = 400f;

    [field: SerializeField, Range(0.05f, 1f)]
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
    [field: Tooltip("Transition et vitesse de l'animation de roll.")]
    public StateAnimationData RollAnimation { get; private set; } = new() { CrossFadeDuration = 0.1f, Speed = 1f };

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
    [field: Tooltip("Transition et vitesse de l'animation de mort.")]
    public StateAnimationData DeathAnimation { get; private set; } = new() { CrossFadeDuration = 0.1f, Speed = 1f };

    [field: Space, Header("Audio")]
    [field: SerializeField, Tooltip("Son joué quand le joueur prend un coup.")]
    public EventReference HitEvent { get; private set; }

    [field: SerializeField, Tooltip("Son joué pendant le dash.")]
    public EventReference DashEvent { get; private set; }

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
        PushForce *= Gain(deltas.PushForce);
        InvincibilityDuration *= Gain(deltas.Invincibility);
        DashDuration = Mathf.Max(MinDuration, DashDuration / Gain(deltas.DashSpeed));
    }

    private static float Gain(float percent) => Mathf.Clamp(1f + percent / 100f, MinGain, MaxGain);
}