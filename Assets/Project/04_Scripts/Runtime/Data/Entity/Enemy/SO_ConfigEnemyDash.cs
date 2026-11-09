using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Actors/Enemy/Dasher", fileName = "SO_ConfigEnemyDash")]
public class SO_ConfigEnemyDash : SO_ConfigEnemy
{
    [field: Header("Lock")]
    [field: SerializeField, FormerlySerializedAs("<LockTime>k__BackingField")]
    [field: Tooltip("Durée en secondes pendant laquelle l'ennemi vise le joueur avant de dasher.")]
    public float LockTime { get; private set; } = 0.6f;

    [field: SerializeField, FormerlySerializedAs("<TimeTransitionLockToAttack>k__BackingField")]
    [field: Tooltip("Pause en secondes entre la fin de la visée et le début du dash. Laisse au joueur un dernier instant pour réagir.")]
    public float LockToDashDelay { get; private set; } = 0.15f;

    [field: Space, Header("Dash")]
    [field: SerializeField, FormerlySerializedAs("<DashDistance>k__BackingField")]
    [field: Tooltip("Distance parcourue par le dash, en unités.")]
    public float DashDistance { get; private set; } = 7f;

    [field: SerializeField, FormerlySerializedAs("<DashDuration>k__BackingField")]
    [field: Tooltip("Durée du dash en secondes. Plus c'est court, plus le dash est rapide.")]
    public float DashDuration { get; private set; } = 0.3f;

    [field: SerializeField, FormerlySerializedAs("<CurveDash>k__BackingField")]
    [field: Tooltip("Forme de l'accélération du dash. X : temps (0 = début, 1 = fin). Y : distance parcourue (0 = départ, 1 = arrivée).")]
    public AnimationCurve DashCurve { get; private set; }

    [field: SerializeField, FormerlySerializedAs("<DashCooldown>k__BackingField")]
    [field: Tooltip("Temps d'attente en secondes avant que l'ennemi puisse dasher à nouveau.")]
    public float DashCooldown { get; private set; } = 2.5f;

    [field: Space, Header("Strike")]
    [field: SerializeField, FormerlySerializedAs("<PlayerLayer>k__BackingField")]
    [field: Tooltip("Layer du joueur. Sert à détecter si le dash touche le joueur.")]
    public LayerMask PlayerLayer { get; private set; }

    [field: SerializeField, FormerlySerializedAs("<StrikeRadius>k__BackingField")]
    [field: Tooltip("Rayon de la zone de dégâts du dash.")]
    public float StrikeRadius { get; private set; } = 1f;

    [field: SerializeField, FormerlySerializedAs("<StrikeForwardOffset>k__BackingField")]
    [field: Tooltip("Décalage de la zone de dégâts vers l'avant de l'ennemi.")]
    public float StrikeForwardOffset { get; private set; } = 0.5f;
}