using UnityEngine;

namespace Event_Bus
{
    public readonly struct EnemyHitEvent : IEvent
    {
        public readonly Transform Target;
        public readonly Vector3 Position;

        public EnemyHitEvent(Transform target, Vector3 position)
        {
            Target = target;
            Position = position;
        }
    }
}