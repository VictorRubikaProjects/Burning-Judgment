using UnityEngine;

[CreateAssetMenu(fileName = "ConfigStatsPlayer", menuName = "Config/Player Stats")]
public class ConfigStatsPlayer : ScriptableObject
{
    [Header("Dash")]
    [SerializeField] [Range(0f, 20f)] private float dashDistance = 7f;
    
    [SerializeField] [Range(0.01f, 1f)] private float dashDuration = 0.2f;
    
    [SerializeField] private AnimationCurve dashCurve = new(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    [SerializeField] [Range(0f, 5f)] private float dashRadius = 2f;
    
    [SerializeField] [Range(0f, 3f)] private float offsetDash = 2f;

    [Space] [Header("Attack")]
    [SerializeField] [Range(0f, 5f)] private float attackCastRadius = 0.5f;
    [SerializeField] [Range(0f, 40f)] private float attackCastDistance = 5f;
    [SerializeField] private DashData attackDash = new() { Duration = 0.15f };
    [SerializeField] [Range(0f, 3f)] private float attackStopOffset = 0.5f;

    [Space] [Header("Hit")]
    [SerializeField] [Range(0f, 2f)] private float hitDuration = 0.3f;
    
    [SerializeField] [Range(1, 20)] private int maxHealth = 3;

    [Space] [Header("Input")]
    [SerializeField] [Range(0f, 150f)] private float swipeThreshold = 50f;
    [SerializeField] [Range(0f, 0.3f)] private float maxTimeSwipe = 0.25f;
    [SerializeField] [Range(0f, 0.25f)] private float thresholdTimerAttack = 0.1f;
    [SerializeField] [Range(0f, 1f)] private float timerAttack = 0.5f;

    [Space] [Header("Animation")]
    [SerializeField] [Range(0f, 0.9f)] private float crossFadeDuration = 0.1f;


    public float DashDistance => dashDistance;
    public float DashDuration => dashDuration;
    
    public AnimationCurve DashCurve => dashCurve;
    
    public LayerMask EnemyLayer => enemyLayer;
    public LayerMask ObstacleLayer => obstacleLayer;
    public LayerMask AttackStateLayer() => EnemyLayer | ObstacleLayer;
    
    public float DashRadius => dashRadius;
    public float OffsetDash => offsetDash;

    public float AttackCastRadius => attackCastRadius;
    public float AttackCastDistance => attackCastDistance;
    public DashData AttackDash => attackDash;
    public float AttackStopOffset => attackStopOffset;

    public float HitDuration => hitDuration;
    
    public int MaxHealth => maxHealth;
    
    public float SwipeThreshold => swipeThreshold;
    public float MaxTimeSwipe => maxTimeSwipe;
    public float ThresholdTimerAttack => thresholdTimerAttack;
    public float TimerAttack => timerAttack;
    
    public float CrossFadeDuration => crossFadeDuration;
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (dashDuration <= 0f) dashDuration = 0.01f;
        if (hitDuration < 0f) hitDuration = 0f;
    }
#endif
}