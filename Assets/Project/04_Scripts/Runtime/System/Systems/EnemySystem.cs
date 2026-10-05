using System.Collections.Generic;
using Event_Bus;
using UnityEngine;

public class EnemySystem
{
    private readonly List<Actor> m_aliveEnemies = new();
    
    private readonly EnemyBase m_enemyPrefab;
    
    EventBinding<EnemyDiedEvent> m_eventBindingEnemyDied;

    public EnemySystem(EnemyBase enemyPrefab)
    {
        m_enemyPrefab = enemyPrefab;
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

    public void SetupFloor(Vector3[] spawnPositions, PlayerCharacter player)
    {
        m_aliveEnemies.Clear();

        foreach (Vector3 spawnPosition in spawnPositions)
        {
            EnemyBase enemyGo = Object.Instantiate(m_enemyPrefab, spawnPosition, Quaternion.identity);
            
            enemyGo.SetupPlayerRef(player);
            
            Actor enemyActor = enemyGo.GetComponent<Actor>();

            m_aliveEnemies.Add(enemyActor);
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
}