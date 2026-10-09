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

        List<EnemyBase> enemyBases = m_configCurrentRound.RoundData.GetRandomEnemiesPalette().TeamEnemies;

        foreach (var @base in enemyBases)
        {
            Vector3 spawnPos = m_configCurrentRound.GetRandomPosition();
            
            EnemyBase enemyObj = Object.Instantiate(@base, spawnPos, Quaternion.identity);
            
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