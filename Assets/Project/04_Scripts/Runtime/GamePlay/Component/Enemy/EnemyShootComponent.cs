

using UnityEngine;

public class EnemyShootComponent : ActorComponent
{
    private FrequencyTimer m_timerShoot;

    private readonly string m_poolKey;

    private Transform m_shootPoint;
    
    private EnemyShooter m_enemy;
    
    public EnemyShootComponent(Actor owner,AbstractProjectile projectilePrefab, float frequencyShoot,Transform shootPoint) : base(owner)
    {
        m_timerShoot = new FrequencyTimer(1f / frequencyShoot);
        
        m_poolKey = $"projectile_{projectilePrefab.GetInstanceID()}";
        
        ObjectPooler.SetupPool(projectilePrefab, 10, m_poolKey);
        
        m_shootPoint = shootPoint;
        
        m_enemy = (EnemyShooter)owner;
    }

    public void Enable(bool on)
    {
        if (on)
        {
            m_timerShoot.Reset();
            m_timerShoot.Start();
        }
        else
        {
            m_timerShoot.Stop();
        }
    }

    public override void Initialize()
    {
        base.Initialize();

        m_timerShoot.OnTick += Shoot;
        
        m_timerShoot.Start();
    }

    public override void Dispose()
    {
        base.Dispose();
        m_timerShoot.OnTick -= Shoot;
        m_timerShoot.Dispose();
    }


    private void Shoot()
    {
        AbstractProjectile projectile = ObjectPooler.DequeueObject<AbstractProjectile>(m_poolKey);
        
        projectile.gameObject.SetActive(true);
        
        projectile.transform.position = m_shootPoint.position;
        
        projectile.transform.rotation = Owner.transform.rotation;
        
        projectile.Launch(Owner.transform.forward,m_poolKey,m_enemy);
    }
}