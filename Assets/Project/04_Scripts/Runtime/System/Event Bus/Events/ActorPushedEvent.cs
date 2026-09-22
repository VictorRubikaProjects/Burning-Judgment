using UnityEngine;

namespace Event_Bus
{
    public readonly struct ActorPushedEvent : IEvent
    {
        public readonly Actor Target;
        public readonly Actor From;
        public readonly float Force;
        public readonly Vector3 Direction;

        public ActorPushedEvent(Actor target, Actor from, float force)
        {
            Target = target; 
            From = from;
            Force = force;
            Direction = (target.transform.position - from.transform.position).normalized;
        }
    }
}