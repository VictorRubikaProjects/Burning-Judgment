using Event_Bus;
using UnityEngine;

public class ProjectileEnemyClassic : AbstractProjectile
{
    protected override bool TryHandleHit(Collider other)
    {
        if (!other.CompareTag("Player")) return false;
        
        Actor playerActor = other.GetComponent<Actor>();
        
        if (playerActor == null) return false;
        
        EventBus<ActorPushedEvent>.Raise(new ActorPushedEvent(playerActor,m_owner,m_owner.Stats.Force));
        
        return true;
    }
}