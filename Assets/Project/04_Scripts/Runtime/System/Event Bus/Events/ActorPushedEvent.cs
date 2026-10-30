using UnityEngine;

namespace Event_Bus
{
    public readonly struct ActorPushedEvent : IEvent
    {
        public enum PushType
        {
            Roll,
            Dash,
            Classic
        }
        
        public readonly PushType Type;
        public readonly Actor Target;
        public readonly float Force;
        public readonly Vector3 Direction;

        public ActorPushedEvent(Actor target, Vector3 direction, float force, PushType pushType =  PushType.Classic)
        {
            Target = target; 
            Force = force;
            Direction = direction;
            Type = pushType;
        }
    }
}