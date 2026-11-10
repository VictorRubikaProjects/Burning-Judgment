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
        public readonly Vector3 SourcePosition;
        public readonly float ComboDecrease;

        public ActorPushedEvent(Actor target, Vector3 direction, float force, float destabilization, Vector3 sourcePosition, float comboDecrease = 0f, PushType pushType = PushType.Classic)
        {
            Target = target; 
            Force = force;
            Direction = direction;
            Destabilization = destabilization;
            Type = pushType;
            SourcePosition = sourcePosition;
            ComboDecrease =  comboDecrease;
        }
    }
}