using UnityEngine;

namespace Event_Bus
{
    public readonly struct ActorPushedEvent : IEvent
    {
        public readonly Actor Target;
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        public readonly float Force;

        public ActorPushedEvent(Actor target, Vector3 position, Vector3 direction, float force)
        {
            Target = target; Position = position; Direction = direction; Force = force;
        }
    }
}