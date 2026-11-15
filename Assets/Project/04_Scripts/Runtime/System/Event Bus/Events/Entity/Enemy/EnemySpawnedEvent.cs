using Event_Bus;

public readonly struct EnemySpawnedEvent : IEvent
{
    public readonly EnemyBase Enemy;

    public EnemySpawnedEvent(EnemyBase enemy)
    {
        Enemy = enemy;
    }
}