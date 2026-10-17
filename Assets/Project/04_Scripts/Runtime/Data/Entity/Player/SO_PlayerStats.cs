using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Actors/Player Stats", fileName = "SO_PlayerStats")]
public class SO_PlayerStats : SO_ActorsStats
{
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
    [field: SerializeField, Range(0f, 150f), FormerlySerializedAs("swipeThreshold")]
    [field: Tooltip("Longueur minimale d'un swipe pour être reconnu comme un geste, en pixels.")]
    public float SwipeThreshold { get; private set; } = 50f;

    [field: SerializeField, Range(0f, 0.3f), FormerlySerializedAs("maxTimeSwipe")]
    [field: Tooltip("Durée maximale d'un swipe en secondes. Un geste plus lent n'est pas reconnu comme un swipe.")]
    public float MaxTimeSwipe { get; private set; } = 0.25f;

    [field: SerializeField, Range(0f, 0.25f), FormerlySerializedAs("thresholdTimerAttack")]
    [field: Tooltip("Durée minimale d'appui en secondes avant que l'appui compte comme une attaque.")]
    public float ThresholdTimerAttack { get; private set; } = 0.1f;

    [field: SerializeField, Range(0f, 1f), FormerlySerializedAs("timerAttack")]
    [field: Tooltip("Durée maximale d'appui en secondes pour déclencher une attaque.")]
    public float TimerAttack { get; private set; } = 0.5f;

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

    [field: Space, Header("Audio")]
    [field: SerializeField, Tooltip("Son joué quand le joueur prend un coup.")]
    public EventReference HitEvent { get; private set; }

    [field: SerializeField, Tooltip("Son joué pendant le dash.")]
    public EventReference DashEvent { get; private set; }

    [field: SerializeField, Tooltip("Son joué pendant le dash d'attaque.")]
    public EventReference AttackDashEvent { get; private set; }

    public LayerMask AttackStateLayer() => EnemyLayer | ObstacleLayer;
}