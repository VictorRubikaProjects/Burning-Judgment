using UnityEngine;

public class DummyProjectile : AbstractProjectile
{
    protected override bool TryHandleHit(Collider other)
    {
        if (!other.CompareTag("Player") || !other.TryGetComponent(out PlayerCharacter pc)) return false;
        
        pc.Health.TakeDamage();
        return true;

    }
}