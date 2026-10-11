using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Config/Actors/Player Stats", fileName = "SO_PlayerStats")]
public class SO_PlayerStats : SO_ActorsStats
{
    [field: Header("Dash")]
    [field: SerializeField, Range(0f, 20f), FormerlySerializedAs("dashDistance")]
    public float DashDistance { get; private set; } = 7f;

    [field: SerializeField, Range(0.01f, 1f), FormerlySerializedAs("dashDuration")]
    public float DashDuration { get; private set; } = 0.2f;

    [field: SerializeField, FormerlySerializedAs("dashCurve")]
    public AnimationCurve DashCurve { get; private set; } = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    [field: SerializeField, FormerlySerializedAs("enemyLayer")]
    public LayerMask EnemyLayer { get; private set; }

    [field: SerializeField, FormerlySerializedAs("obstacleLayer")]
    public LayerMask ObstacleLayer { get; private set; }

    [field: SerializeField, Range(0f, 5f), FormerlySerializedAs("dashRadius")]
    public float DashRadius { get; private set; } = 2f;

    [field: SerializeField, Range(0f, 3f), FormerlySerializedAs("offsetDash")]
    public float OffsetDash { get; private set; } = 2f;

    [field: Space, Header("Attack")]
    [field: SerializeField, Range(0f, 5f), FormerlySerializedAs("attackCastRadius")]
    public float AttackCastRadius { get; private set; } = 0.5f;

    [field: SerializeField, Range(0f, 40f), FormerlySerializedAs("attackCastDistance")]
    public float AttackCastDistance { get; private set; } = 5f;

    [field: SerializeField, FormerlySerializedAs("attackDash")]
    public DashData AttackDash { get; private set; } = new() { Duration = 0.15f };

    [field: SerializeField, Range(0f, 3f), FormerlySerializedAs("attackStopOffset")]
    public float AttackStopOffset { get; private set; } = 0.5f;

    [field: Space, Header("Input")]
    [field: SerializeField, Range(0f, 150f), FormerlySerializedAs("swipeThreshold")]
    public float SwipeThreshold { get; private set; } = 50f;

    [field: SerializeField, Range(0f, 0.3f), FormerlySerializedAs("maxTimeSwipe")]
    public float MaxTimeSwipe { get; private set; } = 0.25f;

    [field: SerializeField, Range(0f, 0.25f), FormerlySerializedAs("thresholdTimerAttack")]
    public float ThresholdTimerAttack { get; private set; } = 0.1f;

    [field: SerializeField, Range(0f, 1f), FormerlySerializedAs("timerAttack")]
    public float TimerAttack { get; private set; } = 0.5f;

    [field: Space, Header("Animation")]
    [field: SerializeField, Range(0f, 0.9f), FormerlySerializedAs("crossFadeDuration")]
    public float CrossFadeDuration { get; private set; } = 0.1f;

    [field: Space, Header("Recover")]
    [field: SerializeField, Range(0f, 2f), FormerlySerializedAs("recoverDuration")]
    public float RecoverDuration { get; private set; } = 0.2f;

    [field: SerializeField, Range(0f, 0.5f), FormerlySerializedAs("recoverCancelThreshold")]
    public float RecoverCancelThreshold { get; private set; } = 0.1f;
    
    [field: Space, Header("Bounce")]
    [field:SerializeField, Range(0.1f,1)] public float BounceThresholdTimer { get; private set; } = 0.1f;

    [field: Space, Header("Audio")]
    [field: SerializeField] public EventReference HitEvent { get; private set; }
    [field: SerializeField] public EventReference DashEvent { get; private set; }
    [field: SerializeField] public EventReference AttackDashEvent { get; private set; }

    public LayerMask AttackStateLayer() => EnemyLayer | ObstacleLayer;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (DashDuration <= 0f) DashDuration = 0.01f;
    }
#endif
}