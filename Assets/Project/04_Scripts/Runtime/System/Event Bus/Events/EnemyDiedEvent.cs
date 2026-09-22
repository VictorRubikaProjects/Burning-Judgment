namespace Event_Bus
{
    public struct EnemyDiedEvent
    {
        public readonly Actor Enemy;

        public EnemyDiedEvent(Actor enemy)
        {
            Enemy = enemy;
        }
    }
}