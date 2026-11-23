namespace Event_Bus
{
    public struct SpawnerSpawnedEvent : IEvent
    {
        public readonly StrikeableSpawnerEnemy Spawner;

        public SpawnerSpawnedEvent(StrikeableSpawnerEnemy spawner)
        {
            Spawner = spawner;
        }
    }

    public struct SpawnerDiedEvent : IEvent
    {
        public readonly StrikeableSpawnerEnemy Spawner;

        public SpawnerDiedEvent(StrikeableSpawnerEnemy spawner)
        {
            Spawner = spawner;
        }
    }
}