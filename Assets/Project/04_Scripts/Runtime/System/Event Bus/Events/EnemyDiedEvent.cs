using Event_Bus;

public struct EnemyDiedEvent : IEvent
{
    public readonly Actor Enemy;

    public EnemyDiedEvent(Actor enemy)
    {
        Enemy = enemy;
    }
}