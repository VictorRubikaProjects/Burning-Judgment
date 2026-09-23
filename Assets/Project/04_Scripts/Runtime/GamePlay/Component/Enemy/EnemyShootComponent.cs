

public class EnemyShootComponent : ActorComponent
{
    private CountdownTimer m_timerShoot;
    
    public EnemyShootComponent(Actor owner) : base(owner)
    {
        m_timerShoot = new CountdownTimer(2f);
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

        m_timerShoot.OnTimerStop += Shoot;
        
        m_timerShoot.Start();
    }

    public override void Dispose()
    {
        base.Dispose();
        
        m_timerShoot.OnTimerStop -= Shoot;
    }


    private void Shoot()
    {
        m_timerShoot.Reset();
        m_timerShoot.Start();
    }
}