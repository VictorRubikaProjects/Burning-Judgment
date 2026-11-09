using UnityEngine;

public class EnemyShootComponent : ActorComponent
{
    private readonly FrequencyTimer m_timerShoot;
    private readonly string m_poolKey;
    private readonly Transform m_shootPoint;
    private readonly EnemyShooter m_enemy;

    private bool m_isSubscribed;
    private bool m_isDisposed;

    public EnemyShootComponent(Actor owner, AbstractProjectile projectilePrefab, float frequencyShoot, Transform shootPoint) : base(owner)
    {
        m_timerShoot = new FrequencyTimer(1f / frequencyShoot);

        m_poolKey = $"projectile_{projectilePrefab.GetInstanceID()}";

        ObjectPooler.SetupPool(projectilePrefab, 10, m_poolKey);

        m_shootPoint = shootPoint;

        m_enemy = (EnemyShooter)owner;
    }

    public void Enable(bool on)
    {
        if (m_isDisposed) return;

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

        if (m_isSubscribed) return;

        m_timerShoot.OnTick += Shoot;
        m_isSubscribed = true;
    }

    public override void Dispose()
    {
        if (m_isDisposed) return;
        m_isDisposed = true;

        base.Dispose();

        m_timerShoot.Stop();
        m_timerShoot.OnTick -= Shoot;
        m_timerShoot.Dispose();
    }

    private void Shoot()
    {
        if (m_isDisposed) return;

        if (m_shootPoint == null || m_enemy == null)
        {
            m_timerShoot.Stop();
            return;
        }

        AbstractProjectile projectile = ObjectPooler.DequeueObject<AbstractProjectile>(m_poolKey);

        if (projectile == null) return;

        projectile.transform.SetPositionAndRotation(m_shootPoint.position, m_enemy.transform.rotation);
        projectile.gameObject.SetActive(true);

        projectile.Launch(m_enemy.transform.forward, m_poolKey, m_enemy);
    }
}