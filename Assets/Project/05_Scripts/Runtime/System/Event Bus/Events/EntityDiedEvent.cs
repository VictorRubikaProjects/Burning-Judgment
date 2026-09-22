namespace Event_Bus
{
    public readonly struct EntityDiedEvent : IEvent
    {
        public readonly Actor Entity;

        public EntityDiedEvent(Actor entity)
        {
            Entity = entity;
        }
    }
}