using System.Collections.Generic;
using Event_Bus;
using UnityEngine;

public class EnemySystem
{
    public SO_ConfigRound ConfigCurrentRound => m_configCurrentRound;

    private readonly List<EnemyBase> m_aliveEnemies = new();
    private readonly List<StrikeableSpawnerEnemy> m_aliveSpawner = new();
    private readonly SO_EntityPooling m_entityPooling;

    private SO_ConfigRound m_configCurrentRound;

    private EventBinding<EnemyDiedEvent> m_eventBindingEnemyDied;
    private EventBinding<EnemySpawnedEvent> m_eventBindingEnemySpawned;
    private EventBinding<SpawnerSpawnedEvent> m_eventBindingSpawnerSpawned;
    private EventBinding<SpawnerDiedEvent> m_eventBindingSpawnerDied;

    private readonly AudioService m_audioService;

    public EnemySystem(SO_EntityPooling entityPoolingData)
    {
        m_entityPooling = entityPoolingData;
        m_audioService = ServiceLocator.Get<AudioService>();
    }

    public void Initialize()
    {
        m_eventBindingEnemyDied = new EventBinding<EnemyDiedEvent>(OnEnemyDied);
        EventBus<EnemyDiedEvent>.Register(m_eventBindingEnemyDied);

        m_eventBindingEnemySpawned = new EventBinding<EnemySpawnedEvent>(OnEnemySpawned);
        EventBus<EnemySpawnedEvent>.Register(m_eventBindingEnemySpawned);

        m_eventBindingSpawnerSpawned = new EventBinding<SpawnerSpawnedEvent>(OnSpawnerSpawned);
        EventBus<SpawnerSpawnedEvent>.Register(m_eventBindingSpawnerSpawned);

        m_eventBindingSpawnerDied = new EventBinding<SpawnerDiedEvent>(OnSpawnerDied);
        EventBus<SpawnerDiedEvent>.Register(m_eventBindingSpawnerDied);
    }

    public void Dispose()
    {
        EventBus<EnemyDiedEvent>.Unregister(m_eventBindingEnemyDied);
        EventBus<EnemySpawnedEvent>.Unregister(m_eventBindingEnemySpawned);
        EventBus<SpawnerSpawnedEvent>.Unregister(m_eventBindingSpawnerSpawned);
        EventBus<SpawnerDiedEvent>.Unregister(m_eventBindingSpawnerDied);
    }

    public List<EnemyBase> SetupFloor(PlayerCharacter player)
    {
        m_aliveEnemies.Clear();
        m_aliveSpawner.RemoveAll(spawner => spawner == null);

        List<EnemyBase> enemyBases = m_configCurrentRound.RoundData.GetRandomEnemyTeam().TeamEnemies;
        int spawnCount = m_configCurrentRound.EnemySpawnPositionsCount;

        if (spawnCount == 0)
        {
            Debug.LogWarning("Aucun point de spawn défini dans le round courant !");
            CheckFloorCleared();
            return m_aliveEnemies;
        }

        List<int> spawnIndices = new List<int>();

        for (int i = 0; i < spawnCount; i++)
        {
            spawnIndices.Add(i);
        }

        for (int i = 0; i < spawnIndices.Count; i++)
        {
            int randomIndex = Random.Range(i, spawnIndices.Count);
            (spawnIndices[i], spawnIndices[randomIndex]) = (spawnIndices[randomIndex], spawnIndices[i]);
        }

        for (int i = 0; i < enemyBases.Count; i++)
        {
            int pointIndex = spawnIndices[i % spawnCount];
            Vector3 spawnPos = m_configCurrentRound.GetPosition(pointIndex);

            EnemyBase enemyObj = Object.Instantiate(enemyBases[i], spawnPos, Quaternion.identity);
            enemyObj.SetupPlayerRef(player);

            m_aliveEnemies.Add(enemyObj);
            enemyObj.gameObject.SetActive(false);
        }
        return m_aliveEnemies;
    }

    private void OnEnemySpawned(EnemySpawnedEvent e)
    {
        if (e.Enemy == null || m_aliveEnemies.Contains(e.Enemy)) return;
        m_aliveEnemies.Add(e.Enemy);
    }

    private void OnEnemyDied(EnemyDiedEvent e)
    {
        if (!m_aliveEnemies.Remove(e.Enemy)) return;

        CheckFloorCleared();
    }

    private void OnSpawnerSpawned(SpawnerSpawnedEvent e)
    {
        if (e.Spawner == null || m_aliveSpawner.Contains(e.Spawner)) return;
        m_aliveSpawner.Add(e.Spawner);
    }

    private void OnSpawnerDied(SpawnerDiedEvent e)
    {
        if (e.Spawner == null) return;
        if (!m_aliveSpawner.Remove(e.Spawner)) return;

        CheckFloorCleared();
    }

    private void CheckFloorCleared()
    {
        m_aliveEnemies.RemoveAll(enemy => enemy == null);
        m_aliveSpawner.RemoveAll(spawner => spawner == null);

        if (m_aliveEnemies.Count == 0 && m_aliveSpawner.Count == 0)
        {
            EventBus<FloorClearedEvent>.Raise(new FloorClearedEvent());
            m_audioService.PlaySfx(m_audioService.Bank.RoundWonEvent);
        }
    }

    public void SetupRound() => m_configCurrentRound = m_entityPooling.GetRandomRound();
}