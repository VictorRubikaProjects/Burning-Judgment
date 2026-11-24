using System.Collections.Generic;
using Event_Bus;
using PrimeTween;
using UnityEngine;

public class StrikeableSpawnerEnemy : StrikeableElement, IDamageable
{
    private const float MinFrequency = 0.01f;
    private const float TargetHeight = 2f;
    private const float GroundOffset = 0.1f;
    private const int PlacementAttempts = 100;
    private const float RotationAngle = 90f;
    private const float RotationStepDuration = 0.8f;

    [Header("Spawner Settings")]
    [SerializeField] protected int life;
    [SerializeField, Min(MinFrequency)] protected float frequency = 1f;
    [SerializeField] protected float range;
    [SerializeField, Min(1)] protected int maxAlive = 10;
    [Space]
    [SerializeField] protected EnemyBase[] enemies;
    [SerializeField] protected LayerMask layerGround;
    [SerializeField] protected LayerMask layerBlocking;
    [Space]
    [SerializeField] private Transform visual;

    private readonly List<EnemyBase> m_spawned = new();

    private FrequencyTimer m_frequencyTimer;
    private GameService m_gameService;
    private Tween m_currentTween;
    private Sequence m_rotationSequence;

    private bool m_isDestroyed;
    private int m_maxLife;

    private void Awake()
    {
        m_frequencyTimer = new FrequencyTimer(1f / Mathf.Max(MinFrequency, frequency));
        m_frequencyTimer.OnTick += SpawnEnemy;
        m_frequencyTimer.Reset();
        m_frequencyTimer.Start();

        m_maxLife = life;

        onStruck.AddListener(TakeDamage);

        EventBus<SpawnerSpawnedEvent>.Raise(new SpawnerSpawnedEvent(this));
    }

    private void Start()
    {
        m_gameService = ServiceLocator.Get<GameService>();

        if (!visual) return;

        float startY = visual.localPosition.y;

        m_currentTween = Tween.LocalPositionY(
            visual,
            startValue: startY,
            endValue: startY + TargetHeight,
            duration: 1f,
            cycles: -1,
            cycleMode: CycleMode.Yoyo,
            ease: Ease.InOutSine);

        StartRotation();
    }

    private void StartRotation()
    {
        Vector3 baseAngles = visual.localEulerAngles;
        Vector3 stepX = baseAngles + new Vector3(RotationAngle, 0f, 0f);
        Vector3 stepY = stepX + new Vector3(0f, RotationAngle, 0f);
        Vector3 stepZ = stepY + new Vector3(0f, 0f, RotationAngle);

        m_rotationSequence = Sequence.Create(cycles: -1, cycleMode: Sequence.SequenceCycleMode.Yoyo)
            .Chain(Tween.LocalEulerAngles(visual, baseAngles, stepX, RotationStepDuration, Ease.InOutSine))
            .Chain(Tween.LocalEulerAngles(visual, stepX, stepY, RotationStepDuration, Ease.InOutSine))
            .Chain(Tween.LocalEulerAngles(visual, stepY, stepZ, RotationStepDuration, Ease.InOutSine));
    }

    private void OnDestroy()
    {
        if (m_frequencyTimer != null)
        {
            m_frequencyTimer.OnTick -= SpawnEnemy;
            m_frequencyTimer.Stop();
            m_frequencyTimer.Dispose();
            m_frequencyTimer = null;
        }

        if (m_currentTween.isAlive)
        {
            m_currentTween.Stop();
        }

        if (m_rotationSequence.isAlive)
        {
            m_rotationSequence.Stop();
        }

        onStruck.RemoveListener(TakeDamage);
    }

    private void SpawnEnemy()
    {
        if (m_isDestroyed) return;

        if (enemies == null || enemies.Length == 0)
        {
            Debug.LogWarning("StrikeableSpawnerEnemy: No enemies assigned to spawn.", this);
            return;
        }

        m_spawned.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);
        if (m_spawned.Count >= maxAlive) return;

        EnemyBase prefab = enemies[Random.Range(0, enemies.Length)];
        float height = prefab.StatsBase.Height;

        if (!TryFindSpawnPosition(height, out Vector3 spawnPosition)) return;

        EnemyBase enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
        enemy.SetupPlayerRef(m_gameService.Player);
        enemy.SetControlContext(EnemyControlContext.RoundGameplay);
        m_spawned.Add(enemy);
        EventBus<EnemySpawnedEvent>.Raise(new EnemySpawnedEvent(enemy));
    }

    private bool TryFindSpawnPosition(float height, out Vector3 position)
    {
        float halfHeight = height * 0.5f;

        for (int i = 0; i < PlacementAttempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * range;
            Vector3 rayStart = transform.position + new Vector3(randomCircle.x, range, randomCircle.y);

            if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, range * 2f, layerGround)) continue;

            Vector3 candidate = hit.point + new Vector3(0f, halfHeight + GroundOffset, 0f);

            if (Physics.CheckSphere(candidate, halfHeight, layerBlocking, QueryTriggerInteraction.Ignore)) continue;

            position = candidate;
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    public void TakeDamage()
    {
        if (m_isDestroyed) return;

        life = Mathf.Max(0, life - 1);

        if (life <= 0)
        {
            Kill();
        }
    }

    public bool CanTakeDamage() => life > 0;

    private void Kill()
    {
        if (m_isDestroyed) return;
        m_isDestroyed = true;

        EventBus<SpawnerDiedEvent>.Raise(new SpawnerDiedEvent(this));
        Destroy(gameObject);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.aquamarine;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}