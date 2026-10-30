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
        public readonly float Destabilization;
        public readonly Vector3 Direction;

        public ActorPushedEvent(Actor target, Vector3 direction, float force, float destabilization, PushType pushType =  PushType.Classic)
        {
            Target = target; 
            Force = force;
            Direction = direction;
            Destabilization = destabilization;
            Type = pushType;
        }
    }
}