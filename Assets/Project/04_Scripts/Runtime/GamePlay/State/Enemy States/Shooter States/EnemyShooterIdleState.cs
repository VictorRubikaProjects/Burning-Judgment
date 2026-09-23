using UnityEngine;

public class EnemyShooterIdleState : EnemyBaseState
{
    private EnemyShooter m_owner;
    public EnemyShooterIdleState(EnemyShooter owner)
    {
        m_owner = owner;
    }

    public override void OnEnter()
    {
        base.OnEnter();
        Debug.Log($"[{m_owner.name}] Enter Idle");
        m_owner.Shoot.Enable(true);
        m_owner.Agent.ResetPath();
    }

    public override void OnExit()
    {
        base.OnExit();
        Debug.Log($"[{m_owner.name}] Exit Idle");
        m_owner.Shoot.Enable(false);
    }

    public override void Update()
    {
        base.Update();
        m_owner.LookAtPlayer();
        EvaluateSituation();
    }

    private void EvaluateSituation()
    {
        float distance = Vector3.Distance(m_owner.TransformCache.position,m_owner.Player.TransformCache.position);

        if (distance < m_owner.Stats.DistanceMax && distance > m_owner.Stats.DistanceMin) return;
    
        if (distance > m_owner.Stats.DistanceMax)
        {
            Debug.Log($"[{m_owner.name}] RequestChase");
            m_owner.RequestChase();
            return;
        }

        if (distance < m_owner.Stats.DistanceMin)
        {
            Debug.Log($"[{m_owner.name}] RequestFlee");
            m_owner.RequestFlee();
            return;
        }
    }
}