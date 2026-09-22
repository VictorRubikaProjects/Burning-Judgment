using UnityEngine;

namespace Event_Bus
{
    public readonly struct PlayerDamagedEvent : IEvent
    {
        public readonly Vector3 Position;
        public readonly Vector3 KnockbackDirection;
        public readonly float KnockbackForce;

        public PlayerDamagedEvent(Vector3 position, Vector3 knockbackDirection, float knockbackForce)
        {
            Position = position;
            KnockbackDirection = knockbackDirection;
            KnockbackForce = knockbackForce;
        }
    }
}