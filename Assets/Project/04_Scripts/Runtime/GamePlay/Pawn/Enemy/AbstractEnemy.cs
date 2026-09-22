

public class AbstractEnemy : Actor, IDamageable
{
    public virtual bool TakeDamage() => true;

    public virtual bool CanTakeDamage() => true;
}
