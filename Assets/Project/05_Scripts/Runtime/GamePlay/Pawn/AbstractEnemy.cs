

public class AbstractEnemy : Actor, IDamageable
{
    public virtual void TakeDamage()
    {
        
    }

    public virtual bool CanTakeDamage() => true;
}
