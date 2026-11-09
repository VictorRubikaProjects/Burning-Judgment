using System.Collections.Generic;
using Event_Bus;
using UnityEngine;

public class EnemySystem
{
    public SO_ConfigRound ConfigCurrentRound => m_configCurrentRound;
    
    private readonly List<EnemyBase> m_aliveEnemies = new();
    private readonly SO_EntityPooling m_entityPooling;
    
    private SO_ConfigRound m_configCurrentRound;
    
    private EventBinding<EnemyDiedEvent> m_eventBindingEnemyDied;

    public EnemySystem(SO_EntityPooling entityPoolingData)
    {
        m_entityPooling = entityPoolingData;
    }

    public void Initialize()
    {
        m_eventBindingEnemyDied = new EventBinding<EnemyDiedEvent>(OnEnemyDied);
        EventBus<EnemyDiedEvent>.Register(m_eventBindingEnemyDied);
    }

    public void Dispose()
    {
        EventBus<EnemyDiedEvent>.Unregister(m_eventBindingEnemyDied);
    }

    public void SetupFloor(PlayerCharacter player)
    {
        m_aliveEnemies.Clear();

        List<EnemyBase> enemyBases = m_configCurrentRound.RoundData.GetRandomEnemyTeam().TeamEnemies;
        int spawnCount = m_configCurrentRound.EnemySpawnPositionsCount;

        if (spawnCount == 0)
        {
            Debug.LogWarning("Aucun point de spawn défini dans le round courant !");
            return;
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
        }
    }

    private void OnEnemyDied(EnemyDiedEvent e)
    {
        if (!m_aliveEnemies.Remove(e.Enemy)) return;

        if (m_aliveEnemies.Count == 0)
        {
            EventBus<FloorClearedEvent>.Raise(new FloorClearedEvent());
        }
    }

    public void SetupRound() => m_configCurrentRound = m_entityPooling.GetRandomRound();
}