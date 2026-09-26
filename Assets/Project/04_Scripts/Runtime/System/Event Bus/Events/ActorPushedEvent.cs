using UnityEngine;

namespace Event_Bus
{
    public readonly struct ActorPushedEvent : IEvent
    {
        public readonly Actor Target;
        public readonly float Force;
        public readonly Vector3 Direction;

        public ActorPushedEvent(Actor target, Vector3 direction, float force)
        {
            Target = target; 
            Force = force;
            Direction = direction;
        }
    }
}