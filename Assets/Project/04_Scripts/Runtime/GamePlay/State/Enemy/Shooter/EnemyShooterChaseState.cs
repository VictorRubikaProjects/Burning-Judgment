using UnityEngine;

public class EnemyShooterChaseState : EnemyBaseState
{
    public bool IsFinished {get; private set;}
    
    private EnemyShooter m_owner;
    
    public EnemyShooterChaseState(EnemyShooter owner)
    {
        m_owner = owner;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        IsFinished = false;
        m_owner.Shoot.Enable(true);
        m_owner.ConsumeChaseRequest();
        m_owner.Agent.stoppingDistance = m_owner.Stats.DistanceMax;
        m_owner.Agent.SetDestination(m_owner.Player.TransformCache.position);
    }

    public override void OnExit()
    {
        base.OnExit();
        m_owner.Shoot.Enable(false);
    }

    public override void Update()
    {
        base.Update();
        
        m_owner.LookAtPlayer();

        m_owner.Agent.SetDestination(m_owner.Player.TransformCache.position);

        float distance = Vector3.Distance(m_owner.TransformCache.position,m_owner.Player.TransformCache.position);

        if (distance < m_owner.Stats.DistanceMax)
        {
            IsFinished = true;
            return;
        }
    }
}