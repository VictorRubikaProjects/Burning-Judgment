using UnityEngine;

namespace Event_Bus
{
    public readonly struct PlayerHitEvent : IEvent
    {
        public readonly Vector3 Position;

        public PlayerHitEvent(Vector3 position)
        {
            Position = position;
        }
    }
}