using Event_Bus;

public struct EnemyDiedEvent : IEvent
{
    public readonly EnemyBase Enemy;

    public EnemyDiedEvent(EnemyBase enemy)
    {
        Enemy = enemy;
    }
}