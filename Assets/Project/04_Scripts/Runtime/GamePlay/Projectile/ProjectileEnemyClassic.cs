using Event_Bus;
using UnityEngine;

public class ProjectileEnemyClassic : AbstractProjectile
{
    protected override bool TryHandleHit(Collider other)
    {
        if (!other.CompareTag("Player")) return false;
        
        Actor playerActor = other.GetComponent<Actor>();
        
        if (playerActor == null) return false;
        
        //TODO FIX MAGIC NUMBER
        EventBus<ActorPushedEvent>.Raise(new ActorPushedEvent(playerActor,m_owner,5f));
        
        return true;
    }
}